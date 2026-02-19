using System;
using System.Linq;
using Avalonia.Threading;
using GameRandom.Scr.Service;
using Steamworks;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.SteamSDK.UserData;

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
        _isInitialized = true;

        Task.Run(async () => await User.GetInstance().InitializeUser());
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
    
    public static ulong GetSteamIdAsLong()
    {
        return GetSteamManager().GetSteamId().m_SteamID;
    }
}