using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.CurrentGameSystem.Interface;

namespace GameRandom.ViewModels.CurrentGameSystem;

public class CurrentGameLoad : BaseModelService, ICurrentGameLoad
{
    [Inject] private SteamService _steamService = null!;

    public CurrentGameLoad()
    {
        CheckDependence();
    }
    
    public async Task<CurrentGameLoadData> LoadInfo()
    {
        return await InitializeAppInfo();
    }
    
    /// <summary>
    /// Initializes application information by its ID.
    /// </summary>
    private async Task<CurrentGameLoadData> InitializeAppInfo()
    {
        var userGameInfo = await GetUserGameFromUserId(User.GetInstance().GetUserId());
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultDatabaseTimeLimitSecond));
        var gameInfo = await DatabaseService.GetFirstOrDefaultAsync<GameProgresses>
            (e => e.AppId == userGameInfo.AppId, cts.Token);

        if (gameInfo is null)
        {
            Logger.Error($"Failed to get gameInfo with appID: {userGameInfo.AppId} from database");
            return null!;
        }
        
        return new CurrentGameLoadData(gameInfo, _steamService.GetImageSyncFromBytes(gameInfo.AppHeaderImage), userGameInfo);
    }

    /// <summary>
    /// Retrieves a UserGame object for the specified Steam ID.
    /// </summary>
    private async Task<UserGame> GetUserGameFromUserId(ulong steamId)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultDatabaseTimeLimitSecond));
        var userGameInfo = await DatabaseService.GetUserGameAsync(steamId, cts.Token);

        if (userGameInfo is null)
            throw new NullReferenceException("UserGame is not initialized");

        return userGameInfo;
    }

    private void CheckDependence()
    {
        if (_steamService is null)
            throw new NullReferenceException();
    }

    public override void Dispose()
    {
        _steamService = null!;
        
        base.Dispose();
    }
}