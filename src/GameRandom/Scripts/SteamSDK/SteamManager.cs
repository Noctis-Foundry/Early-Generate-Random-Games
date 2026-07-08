using System;
using Avalonia.Threading;
using GameRandom.Scripts.Service;
using Steamworks;

namespace GameRandom.Scripts.SteamSDK;

public class SteamManager
{
    private const int MaxTryToConnect = 6;
    private static Lazy<SteamManager> _instance = new (() => new SteamManager());
    public bool IsInitialized { get; private set; }= false;

    private DispatcherTimer? _steamCallbackTimer;

    private SteamManager() {}
    
    public void InitSteam()
    {
        if (IsInitialized)
            return;
        
        try
        {
            SteamAPI.Init();
        }
        catch (Exception e)
        {
            Logger.Error($"Error initialize SteamAPI: {e.Message}");
            throw;
        }
        
        StartEventTimer();
        IsInitialized = true;
    }
    private void StartEventTimer()
    {
        _steamCallbackTimer = new DispatcherTimer();
        _steamCallbackTimer.Interval = TimeSpan.FromMilliseconds(10);
        _steamCallbackTimer.Tick += (sender, args) => SteamAPI.RunCallbacks();
        _steamCallbackTimer.Start();
    }

    public void ShutdownSteam()
    {
        if (!IsInitialized) return;

        _steamCallbackTimer?.Stop();
        SteamAPI.Shutdown();
        IsInitialized = false;

        Console.WriteLine("SteamAPI.Shutdown() finished");
    }
    
    public CSteamID GetSteamId()
    {
        if (!IsInitialized)
            throw new Exception("SteamAPI.Init() failed");

        return SteamUser.GetSteamID();
    }

    public static SteamManager GetSteamManager()
    {
        if (_instance == null)
            throw new Exception("failed to get SteamManager");

        return _instance.Value;
    }
    
    public static ulong GetSteamIdAsLong()
    {
        return GetSteamManager().GetSteamId().m_SteamID;
    }
}