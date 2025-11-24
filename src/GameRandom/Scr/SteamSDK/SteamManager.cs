using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.SteamSDK.UserSystem;
using Steamworks;

namespace GameRandom.SteamSDK;

public class SteamManager
{
    private const int MaxTryToConnect = 6;
    private static SteamManager? _instance;
    private bool _isInitialized = false;
    
    private DispatcherTimer? _steamCallbackTimer;

    public SteamManager()
    {
        _instance = this;
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
        
        InitializeUser();
    }
    private void StartEventTimer()
    {
        _steamCallbackTimer = new DispatcherTimer();
        _steamCallbackTimer.Interval = TimeSpan.FromMilliseconds(10);
        _steamCallbackTimer.Tick += (sender, args) =>  SteamAPI.RunCallbacks();
        _steamCallbackTimer.Start();
    }

    private void InitializeUser()
    {
        var playerId = GetSteamId();

        var userData = new UserData(playerId);
        Di.Container.RegisterSingleInstance(userData);
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
    
    public static SteamManager GetSteamManager()
    {
        if (_instance == null)
            throw new Exception("failed to get SteamManager");

        return _instance;
    }
}