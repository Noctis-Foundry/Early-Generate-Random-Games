using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts.RollGameSystem;
using GameRandom.Scripts.RollGameSystem.Enums;
using GameRandom.Scripts.RollGameSystem.GenerateGames;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.StartupLogic;
using GameRandom.Scripts.SteamSDK.SteamsContexts;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;

namespace GameRandom.Scripts.SteamSDK.SteamWebAPI;

public class SteamWebApi : ISteamWebService
{
    private string? ApiKey;

    IUserOwnerGames _ownerGames = new UserOwnedGames();
    IProfileSummary _profileSummary = new UserProfileSummary();
    
    private Dictionary<SteamWebInterfaces, string> _urlExample //TODO load from json file
        = new()
    {
        [SteamWebInterfaces.GetPlayerSummaries] = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/",
        [SteamWebInterfaces.GetOwnedGames] = "https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/",
    };
    
    public SteamWebApi()
    {
        if (GameEnvLoad._envCollection.TryGetValue(EnvType.SteamApiEnv, out var value))
        {
            ApiKey = value;
        }
        
        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new ArgumentException("Failed load steam web api. Check ethernet connection");
        }
    }
    
    public async Task<ProfileContext?> GetProfile(ulong steamId64)
    {
        Logger.Debug($"Try get profile by id: {steamId64}");
        
        return await _profileSummary.GetUserData(GetSteamUrl(steamId64, SteamWebInterfaces.GetPlayerSummaries),
            steamId64);
    }

    public async Task<JsonDocument?> GetOwnedGames(ulong steamId64)
    {
        return await _ownerGames.GetPlayerLibrary(GetSteamUrl(steamId64, SteamWebInterfaces.GetOwnedGames));
    }

    public async Task<GenerateGameStruct> GetGameFromStore(int appId)
    {
        return await _ownerGames.GetAppInfoFromAppId(appId);
    }

    private string GetSteamUrl(ulong steamId64, SteamWebInterfaces interfaceName)
    {
        var exampleUrl = _urlExample[interfaceName];
        
        if (string.IsNullOrEmpty(exampleUrl) || steamId64 == 0)
        {
            Logger.Error("Invalid url or steamId64");
            return "";
        }

        string url = "";
        
        if (SteamWebInterfaces.GetPlayerSummaries == interfaceName) 
            url = $"{exampleUrl}?key={ApiKey}&steamids={steamId64}";

        if (interfaceName == SteamWebInterfaces.GetOwnedGames)
            url += $"{exampleUrl}?key={ApiKey}&steamid={steamId64}&format=json";

        return url;
    }
}

public class UserOwnedGames : IUserOwnerGames
{
    private static readonly HttpClient _client = new();
    private HttpResponseMessage _response = null!;
    private const int MaxHttpGetInter = 3;

    private readonly HashSet<string> _nonGameTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Photo editing",
        "Utilities",
        "Game development",
        "Animation & Modeling",
        "Illustration"
    };
    
    public async Task<JsonDocument?> GetPlayerLibrary(string steamApiKey)
    {

        try
        {
            var response = await _client.GetAsync(steamApiKey);
            var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            var responseProperty = document.RootElement.GetProperty("response");

            if (!responseProperty.TryGetProperty("games", out var value))
            {
                ShowError("Steam profile is private or not game views");
                return null;
            }
            
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            return null;
        }
    }

    public async Task<GenerateGameStruct> GetAppInfoFromAppId(int appId)
    {
        try
        {
            string url = CreateSteamStoreUrl(appId);

            if (!await CheckResponseAsync(url))
                return new GenerateGameStruct{StatusCode = GenerationStatusCode.GenerateNext};
            
            var json = await _response.Content.ReadAsStreamAsync();
            var document = await JsonDocument.ParseAsync(json);
            var root = document.RootElement;
            var appIdStr = root.EnumerateObject().First().Name;  
            var appDataElement = root.GetProperty(appIdStr).GetProperty("data");

            return CreateAppSavedContext(appDataElement, appId);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to get game from steam store {e.Message}");
            return new GenerateGameStruct {StatusCode = GenerationStatusCode.GenerateNext};
        }
    }

    private async Task<bool> CheckResponseAsync(string url)
    {
        for (int i = 0; i < MaxHttpGetInter; i++)
        {
            _response = await _client.GetAsync(url);

            if (_response.IsSuccessStatusCode)
                return true;
        }
        
        var content = await _response.Content.ReadAsStringAsync();
        ShowError($"{_response.StatusCode}");
        Logger.Error($"HTTP: {_response.StatusCode} {_response.ReasonPhrase} : {content}");

        return false;
    }

    private GenerateGameStruct CreateAppSavedContext(JsonElement jsonElement, int appId)
    {
        string name = jsonElement.GetProperty("name").GetString() ?? "";
        string description = jsonElement.GetProperty("short_description").GetString() ?? "";
        string headerImage = jsonElement.GetProperty("header_image").GetString() ?? "";

        int releaseYear = 0;
        if (jsonElement.TryGetProperty("release_date", out var releaseElement))
        {
            var dateStr = releaseElement.GetProperty("date").GetString(); // например "Aug 21, 2012"
            if (!string.IsNullOrEmpty(dateStr))
            {
                // Парсим дату через DateTime
                if (DateTime.TryParse(dateStr, out var dt))
                    releaseYear = dt.Year;
            }
        }
        
        var genres = new HashSet<string>();
        if (jsonElement.TryGetProperty("genres", out var genresArray) && genresArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var g in genresArray.EnumerateArray())
            {
                var descriptionGenre = g.GetProperty("description").GetString();

                if (string.IsNullOrEmpty(descriptionGenre))
                    continue;

                Logger.Debug($"Genre type = {descriptionGenre} && non game types contains {_nonGameTypes.Contains(descriptionGenre)}");
                
                if (_nonGameTypes.Contains(descriptionGenre))
                {
                    Logger.Error("Failed to create AppSavedContext because not correct game type");
                    return new GenerateGameStruct {StatusCode = GenerationStatusCode.GenerateNext};
                }
                
                genres.Add(descriptionGenre);
            }
               
        }

        var categories = new HashSet<string>();
        if (jsonElement.TryGetProperty("categories", out var categoriesArray) && categoriesArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var c in categoriesArray.EnumerateArray())
                categories.Add(c.GetProperty("description").GetString() ?? "");
        }

        return new GenerateGameStruct {StatusCode = GenerationStatusCode.Successes, AppSavedContext = new AppSavedContext
        {
            AppId = appId,
            AppCategories = categories,
            AppDescription = description,
            AppGenres = genres,
            HeaderImage = headerImage,
            AppName = name,
            AppReleaseYear = releaseYear
        }};
    }

    private string CreateSteamStoreUrl(int appId)
    {
        return $"https://store.steampowered.com/api/appdetails?appids={appId}&cc=EN&l=r";
    }

    private void ShowError(string errorCode)
    {
        if (Di.ResolveInstance.TryGetInstance<IErrorService>() is { } errorService)
            errorService.ShowWindow($"Failed to parse your game with code: {errorCode}");
    }
}

public class UserProfileSummary : IProfileSummary
{
    private static readonly HttpClient _client = new();
    
    public async Task<ProfileContext?> GetUserData(string steamUrl, ulong userId)
    { 
        try
        {
            var response = await _client.GetAsync(steamUrl);

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Not find profile json");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement.GetProperty("response");
        
            var players = root.GetProperty("players");
            var player = players[0];
            var avatar = player.GetProperty("avatar").ToString();
        
            var profileCtx = new ProfileContext
            {
                avatarUrl = avatar,
                MemberID = userId,
            };
        
            return profileCtx;
        }
        catch (Exception e)
        {
            Logger.Debug($"Failed to get user data, profile maybe is private. Log: {e.Message}");
            return null;
        }
    }
}