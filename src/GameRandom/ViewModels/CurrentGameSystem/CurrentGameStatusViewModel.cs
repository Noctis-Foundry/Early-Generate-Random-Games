using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GameRandom.DbContext;
using GameRandom.DISystem;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts.Database;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.Interfaces;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.UserData;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.ViewModels.BaseClasses;
using GameRandom.ViewModels.CurrentGameSystem.Interface;

namespace GameRandom.ViewModels.CurrentGameSystem;

public sealed class CurrentGameStatusViewModel : ViewModelBase
{
    [Inject] private IRouteManager _routeManager = null!;
    private ICurrentGameLoad _currentGameLoad = new CurrentGameLoad();
    private ICurrentGameFinish _currentGameFinish = new CurrentGameFinish();
    
    private SemaphoreSlim _finishSemaphore = new SemaphoreSlim(1, 1);
    
    private const int TimerIterationDelay = 1000;
    private DispatcherTimer? _timer;

    private EventHandler? _savedHandler;
    private Func<PayloadStructure, Task> _listener;

    private const int DelayAfterUserGameChange = 500;

    #region BindingProperty
    private TimeSpan _currentTime;
    
    /// <summary>
    /// The current remaining time until the game ends.
    /// </summary>
    public TimeSpan CurrentTime
    {
        get => _currentTime;
        set => SetProperty(ref _currentTime, value);
    }
    
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
        InitializeDiContainer();
        InitializePostgresListener();
        InitializeSemaphoreSlim();
    }
    
    /// <summary>
    /// Loads current user's game information and starts the timer.
    /// </summary>
    public async Task LoadInfo()
    {
        if (AppInfo is not null)
            return;
        
        if (!await SemaphoreSlimWaitAsync())
            return;
        
        LoadEmpty();
        StartTaskWaiter();

        try
        {
            var result = await Dispatcher.UIThread.InvokeAsync(async () => await _currentGameLoad.LoadInfo());

            if (result is null)
                return;

            var data = result;

            AppInfo = data.GameInfo;
            ImageBitmap = data.ImageBitmap;
            UserGame = data.UserGame;

            StartTimer();
            IsEmpty = false;
        }
        catch (Exception ex)
        {
            Logger.Error("Error loading game info" + ex);
            ErrorService.ShowWindow("Failed to load game info");
        }
        finally
        {
            CloseTaskWaiterWithSemaphore();
        }
        
    }

    /// <summary>
    /// Starts the process of finishing the current game.
    /// </summary>
    public async Task FinishingGame()
    {
        if (!await SemaphoreSlimWaitAsync())
                return;
        
        if (!IsCanStartFinishingGame()) return;

        try
        {
            var result =
                await Dispatcher.UIThread.InvokeAsync(() =>
                    _currentGameFinish.FinishingGame(AppInfo!)); //AppInfo is checking in IsCanStartFinishingGame 

            if (result is not null)
            {
                UserGame = result;
                ClearingContent();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"{ex}");
            ErrorService.ShowWindow(new ErrorStruct { ErrorMessage = "Failed to finish game" });
        }
        finally
        {
            SemaphoreSlim.Release();
        }
       
    }

    /// <summary>
    /// Checks if the game finishing process can be started.
    /// </summary>
    private bool IsCanStartFinishingGame()
    {
        if (AppInfo is null)
        {
            ErrorService.ShowWindow(new ErrorStruct { ErrorMessage = "Failed to finish game, your game is empty" });
            return false;
        }

        return true;
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

    #region Initialization and Helpers

    protected override void InitializeDiContainer()
    {
        base.InitializeDiContainer();

        if (_routeManager is null)
            throw new NullReferenceException(nameof(_routeManager));
    }
    
    /// <summary>
    /// Configures the PostgreSQL change listener.
    /// </summary>
    private void InitializePostgresListener()
    {
        _listener += async (structure) =>
        {
            if (Di.ResolveInstance.TryGetInstance<DatabaseService>() is not { } databaseService)
                throw new NullReferenceException("Failed to inject database dependence");

            CancellationTokenSource cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseOperationDelay));

            var userGame = await databaseService.GetFromRowId<UserGame>(structure.RowId, cts.Token);

            if (userGame is null || userGame.UserId != User.GetInstance().GetUserId())
                return;

            await Task.Delay(DelayAfterUserGameChange);
            await LoadInfo();
        };

        _routeManager.GetRouteService(TableEnum.UserGames).Subscribe(RouteStage.View, _listener);
    }

    /// <summary>
    /// Loads an "empty" game state if no data is available.
    /// </summary>
    private void LoadEmpty()
    {
        AppInfo = null;
        ImageBitmap = AvaloniaService.Instance.CreateBitmapFromPath("Assets/steamAwatarWithNight.jpg");
        IsEmpty = true;
        
        _timer?.Stop();
        _timer?.Tick -= _savedHandler;
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
        if (AppInfo is not null && _appInfo is not null)
            CurrentTime = _appInfo.EndTime - DateTime.Now;
    }

    #endregion

    public override void Dispose()
    {
        ClearingContent();
        
        _currentGameLoad?.Dispose();
        _currentGameFinish?.Dispose();

        _currentGameLoad = null!;
        _currentGameFinish = null!;
        
        _routeManager = null!;
        _listener = null!;
        
        base.Dispose();
    }
}