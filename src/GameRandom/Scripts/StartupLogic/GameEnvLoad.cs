using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameRandom.Src.StartupLogic;

public class GameEnvLoad
{
    private HttpClient _client;
    private const string _url = "http://80.93.62.153/config/config.json";
    public static Dictionary<EnvType, string> _envCollection { get; private set; } = new();

    public async Task InitializeEnv()
    {
        _client = new HttpClient();

        var json = await _client.GetAsync(_url);
        
        var document = await JsonDocument.ParseAsync(await json.Content.ReadAsStreamAsync());
        var root =  document.RootElement;
        
        string databaseApi = root.GetProperty("database_api").GetString() ?? "";
        string steamApi = root.GetProperty("steam_api").GetString() ?? "";

        if (string.IsNullOrEmpty(databaseApi) || string.IsNullOrEmpty(steamApi))
            throw new NullReferenceException("Failed to get config from server. Check connection"); //TODO Add module for check offline mode and online mode [Check summary for this model in docs/TODO]
    }
}

public enum EnvType
{
    DatabaseEnv,
    SteamApiEnv
}