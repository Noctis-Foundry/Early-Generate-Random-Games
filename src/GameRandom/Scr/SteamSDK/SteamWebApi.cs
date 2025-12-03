using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.SteamsContexts;

namespace GameRandom.SteamSDK;

public class SteamWebApi
{
    private const string ApiKey = "74FCA50C2D6D2C3ACF641458D135AA3A";
    private readonly HttpClient _client = new HttpClient();

    public async Task<ProfilerContext?> GetUserData(ulong userId)
    {
        try
        {
            var steamUrl =
                $" https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={ApiKey}&steamids={userId}";

            Logger.Debug($"{steamUrl}");

            Logger.Debug("Start steam parsing");
            
            var response = await _client.GetAsync(steamUrl);

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Not find profile json with id {userId}");
                return null;
            }
            else 
                Logger.Debug($"Get profile json with id {userId}");

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement.GetProperty("response");
        
            var players = root.GetProperty("players");
            var player = players[0];
            var avatar = player.GetProperty("avatar").ToString();
        
            var profileCtx = new ProfilerContext
            {
                avatarUrl = avatar,
                MemberID = userId,
            };

            Logger.Debug($"profile json with id {userId} is null {profileCtx is null}");
        
            return profileCtx;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}