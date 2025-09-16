using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Steamworks;

namespace GameRandom.SteamSDK;

public class SteamManager
{
    private const string ErrorConvertAppText = "Unknown";
    private const string AppList = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";
    
    public static SteamManager Instance { get; private set; }
    private string listPath;
    
    private bool _isInitialized = false;
    Dictionary<int, string> _appList = new Dictionary<int, string>();
    
    public SteamManager()
    {
        Instance = this;

        listPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SteamApps.json");
    }

    public void InitSteam()
    {
        if (_isInitialized)
            return;

        if (!SteamAPI.Init())
        {
            throw new Exception("SteamAPI.Init() failed");
        }
        
        _isInitialized = true;
        Console.WriteLine("SteamAPI.Init() finished");
    }

    public void ShutdownSteam()
    {
        if (!_isInitialized)
        {
            SteamAPI.Shutdown();
            _isInitialized = false;
            Console.WriteLine("SteamAPI.Shutdown() finished");
        }
    }

    public CSteamID GetSteamID()
    {
        if (!_isInitialized)
            throw new Exception("SteamAPI.Init() failed");
        
        return SteamUser.GetSteamID();
    }

    public async Task<string> GetAppList()
    {
        string jsonContent = "";
        
        if (File.Exists(listPath))
        {
            using (var wr = new StreamReader(listPath))
            {
                jsonContent = await wr.ReadToEndAsync();
            }
        }
        else
        {
            jsonContent = await CreateNewAppList();
        }
        
        return jsonContent;
    }

    private async Task<string> CreateNewAppList()
    {
        using HttpClient client = new HttpClient();

        string response = "";

        try
        {
            string url = AppList;
            response = await client.GetStringAsync(url);
            
            await using (StreamWriter sw = new StreamWriter(listPath, false, new System.Text.UTF8Encoding(false)))
            {
                await sw.WriteAsync(response);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка: {e.Message}");
        }
        
        StreamReader sr = new StreamReader(listPath);

        return response;
    }
}