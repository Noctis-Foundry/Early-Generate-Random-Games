using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.SteamsContexts;

namespace GameRandom.Scripts.SteamSDK;

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
        ApiKey = Environment.GetEnvironmentVariable("Steam_Web_Api_Key");
        
        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new ArgumentException("Steam web api key not found");
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
    
    public async Task<JsonDocument?> GetPlayerLibrary(string steamApiKey)
    {

        try
        {
            var response = await _client.GetAsync(steamApiKey);
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            return null;
        }
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