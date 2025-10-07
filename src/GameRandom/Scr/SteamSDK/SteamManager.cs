using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Steamworks;

namespace GameRandom.SteamSDK;

public class SteamManager
{
    private const int MaxTryToConnect = 6;
    public static SteamManager? Instance;
    private bool _isInitialized = false;
    
    private Timer? _steamCallbackTimer;

    public SteamManager()
    {
        Instance = this;
    }
    
    public void InitSteam()
    {
        if (_isInitialized)
            return;

        int connectCount = 0;
        
        while (true)
        {
            try
            {
                SteamAPI.Init();
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Task.Delay(10000).Wait();
                connectCount++;

                if (connectCount == MaxTryToConnect)
                {
                    throw new Exception("Failed to initialize Steam API");
                }
            }
        }
        
        
        StartEventTimer();
        _isInitialized = true;
    }

    private void StartEventTimer()
    {
        _steamCallbackTimer = new Timer(100);
        _steamCallbackTimer.Elapsed += (s, e) =>
        {
            SteamAPI.RunCallbacks();
        };
        _steamCallbackTimer.Start();
    }

    public void ShutdownSteam()
    {
        if (!_isInitialized) return;
        
        _steamCallbackTimer?.Stop();
        SteamAPI.Shutdown();
        _isInitialized = false;
        
        Console.WriteLine("SteamAPI.Shutdown() finished");
    }

    public CSteamID GetSteamId()
    {
        if (!_isInitialized)
            throw new Exception("SteamAPI.Init() failed");
        
        return SteamUser.GetSteamID();
    }
}