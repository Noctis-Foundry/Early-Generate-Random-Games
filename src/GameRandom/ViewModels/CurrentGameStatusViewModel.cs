using System;
using System.Linq;
using System.Threading;
using System.Timers;
using Avalonia.Controls;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.UserData;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GameRandom.ViewModels.AdminConfirmSystem;

public class CurrentGameStatusViewModel : ViewModelBase
{
    #region Fields and Properties

    [Inject] private DatabaseService? _databaseService;
    [Inject] private FinishedGameDialogService? _finishedGameDialogService;
    [Inject] private PostgresListener? _postgresListener;
    [Inject] private ErrorService? _errorService;
    [Inject] private SteamService? _steamService;

    private const int DatabaseOperationDelay = 5;

    private const int LoadSemaphoreDelayWaiting = 1;
    private SemaphoreSlim _loadInfoSemaphore = new SemaphoreSlim(1, 1);
    private SemaphoreSlim _finishSemaphore = new SemaphoreSlim(1, 1);

    private TimeSpan _currentTime;

    /// <summary>
    /// The current remaining time until the game ends.
    /// </summary>
    public TimeSpan CurrentTime
    {
        get => _currentTime;
        set => SetProperty(ref _currentTime, value);
    }

    private const int TimerIterationDelay = 1000;
    private DispatcherTimer? _timer;

    private EventHandler? _savedHandler;
    private Action<PayloadStructure>? _listener;

    /// <summary>
    /// Current user's game data.
    /// </summary>
    public UserGame? UserGame { get; private set; }

    private GameProgresses? _appInfo;

    /// <summary>
    /// Application (game) information from the database.
    /// </summary>
    public GameProgresses? AppInfo
    {
        get => _appInfo;
        set => SetProperty(ref _appInfo, value);
    }

    private Bitmap? _imageBitmap;

    /// <summary>
    /// Game image (header).
    /// </summary>
    public Bitmap? ImageBitmap
    {
        get => _imageBitmap;
        set => SetProperty(ref _imageBitmap, value);
    }

    public bool IsEmpty = true;

    #endregion

    /// <summary>
    /// Constructor initializes dependencies and subscribes to database notifications.
    /// </summary>
    public CurrentGameStatusViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        CheckInjectProperty();

        InitializePostgresListener();
    }

    #region LoadInfo

    /// <summary>
    /// Loads current user's game information and starts the timer.
    /// </summary>
    public async Task LoadInfo()
    {
        if (!await _loadInfoSemaphore.WaitAsync(TimeSpan.FromSeconds(LoadSemaphoreDelayWaiting)))
        {
            Logger.Debug("Thread is not empty");
            return;
        }

        IsProcess = true;
        StartProcessing?.Invoke();
        
        try
        {
            var userGameInfo = await GetUserGameFromUserId(User.GetInstance().GetUserId());

            bool isFind = await InitializeAppInfo(userGameInfo.AppId);

            if (isFind)
            {
                StartTimer();
                IsEmpty = false;
            }
               
        }
        catch (Exception e)
        {
            Logger.Error("Failed to load info " + e.Message);
        }
        finally
        {
            IsProcess = false;
            _loadInfoSemaphore.Release();
        }
    }

    private async Task PostgresLoadInfo(PayloadStructure payloadStructure)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseOperationDelay));
        var userGame =
            await _databaseService.GetFromRowId<UserGame>(payloadStructure.RowId, cancellationTokenSource.Token);

        if (userGame?.UserId != User.GetInstance().GetUserId())
        {
            Logger.Debug("Is not user game updated");
            return;
        }

        await LoadInfo();
    }

    /// <summary>
    /// Initializes application information by its ID.
    /// </summary>
    private async Task<bool> InitializeAppInfo(int appId)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseOperationDelay));
        var gameInfo = await _databaseService.GetFirstOrDefaultAsync<GameProgresses>
            (e => e.AppId == appId, cts.Token);

        if (gameInfo is null)
        {
            Logger.Error($"Failed to get gameInfo with appID: {appId} from database");
            return false;
        }

        AppInfo = gameInfo;

        ImageBitmap = _steamService.GetImageSyncFromBytes(_appInfo.AppHeaderImage);

        return true;
    }

    /// <summary>
    /// Retrieves a UserGame object for the specified Steam ID.
    /// </summary>
    private async Task<UserGame> GetUserGameFromUserId(ulong steamId)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseOperationDelay));
        var userGameInfo = await _databaseService.GetUserGameAsync(steamId, cts.Token);

        if (userGameInfo is null)
            throw new NullReferenceException("UserGame is not initialized");

        UserGame = userGameInfo;

        return userGameInfo;
    }

    /// <summary>
    /// Loads an "empty" game state if no data is available.
    /// </summary>
    private void LoadEmpty()
    {
        AppInfo = new GameProgresses();
        ImageBitmap = AvaloniaService.Instance.CreateBitmapFromPath("Assets/steamAwatarWithNight.jpg");
        IsEmpty = true;
        
        _timer?.Stop();
        _timer?.Tick -= _savedHandler;
    }

    #endregion

    #region FinishedGame

    /// <summary>
    /// Starts the process of finishing the current game.
    /// </summary>
    public async Task FinishingGame()
    {
        if (!await _finishSemaphore.WaitAsync(0))
        {
            _errorService.ShowWindow("Finishing game is processing....");
            return;
        }
        
        if (!IsCanStartFinishingGame()) return;

        try
        {
            var isAdded = await _finishedGameDialogService.ShowWindowAsync(AppInfo);
            
            if (!isAdded) return;
            
            IsProcess = true;
            StartProcessing?.Invoke();

            var isUpdate = await ChangeUserGame();

            if (isUpdate && UserGame?.AppId == 0)
                ClearingContent();
        }
        catch (Exception e)
        {
            Logger.Warning("Failed to finish game: " + e.Message);
        }
        finally
        {
            IsProcess = false;
            _finishSemaphore.Release();
        }
    }

    /// <summary>
    /// Checks if the game finishing process can be started.
    /// </summary>
    private bool IsCanStartFinishingGame()
    {
        if (AppInfo is null)
        {
            _errorService.ShowWindow(new ErrorStruct { ErrorMessage = "Failed to finish game, your game is empty" });
            return false;
        }

        return true;
    }

    /// <summary>
    /// Updates the user's game status in the database (moves to the next game or resets).
    /// </summary>
    private async Task<bool> ChangeUserGame()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseOperationDelay));
        var userGame = await _databaseService.GetUserGameAsync(User.GetInstance().GetUserId(), cts.Token);

        if (userGame is null)
            throw new NullReferenceException("User game is not initialized");

        if (userGame.AppIdList is not null && userGame.AppIdList.Count > 0)
        {
            userGame.AppId = userGame.AppIdList[0];
            userGame.AppIdList.RemoveAt(0);
        }
        else
            userGame.AppId = 0;

        UserGame = userGame;

        return await _databaseService.UpdateAsync(userGame, cts.Token);
    }

    /// <summary>
    /// Clears content and stops timers when all games are finished.
    /// </summary>
    public void ClearingContent()
    {
        LoadEmpty();

        if (_timer != null)
        {
            _timer.Stop();

            if (_savedHandler is not null)
                _timer.Tick -= _savedHandler;
        }

        _savedHandler = null;
        UserGame = null;
    }

    #endregion

    #region Initialization and Helpers

    /// <summary>
    /// Configures the PostgreSQL change listener.
    /// </summary>
    private void InitializePostgresListener()
    {
        _listener += structure =>
        {
            if (structure.TableCode == (int)TableEnum.UserGames)
            {
                Dispatcher.UIThread.InvokeAsync(async () => await PostgresLoadInfo(structure));
            }
        };

        _postgresListener.Subscribe(TableEnum.UserGames, _listener);
    }

    /// <summary>
    /// Verifies that all dependencies were successfully injected via DI.
    /// </summary>
    private void CheckInjectProperty()
    {
        if (_databaseService is null)
            throw new NullReferenceException("DatabaseService was not injected");

        if (_finishedGameDialogService is null)
            throw new NullReferenceException("FinishedGameDialogService was not injected");

        if (_postgresListener is null)
            throw new NullReferenceException("PostgresListener was not injected");

        if (_errorService is null)
            throw new NullReferenceException("ErrorService was not injected");

        if (_steamService is null)
            throw new NullReferenceException("Steam service was not injected");
    }

    /// <summary>
    /// Initializes and starts the time update timer.
    /// </summary>
    private void StartTimer()
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Tick -= _savedHandler;
        }
        
        _savedHandler = (sender, args) => UpdateDateTimer();

        if (_timer is null) 
            _timer = new DispatcherTimer();
        
        _timer.Interval = TimeSpan.FromMilliseconds(TimerIterationDelay);
        _timer.Tick += _savedHandler;
        _timer.Start();
    }

    /// <summary>
    /// Updates the remaining time until the game ends.
    /// </summary>
    private void UpdateDateTimer()
    {
        if (AppInfo is not null)
            CurrentTime = _appInfo.EndTime - DateTime.Now;
    }

    #endregion

    public override void Dispose()
    {
        ClearingContent();

        _databaseService = null;
        _errorService = null;
        _postgresListener = null;
        
        base.Dispose();
    }
}