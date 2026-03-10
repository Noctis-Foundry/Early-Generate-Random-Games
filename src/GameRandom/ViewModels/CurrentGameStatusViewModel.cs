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
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels;

public class CurrentGameStatusViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService = null!;
    [Inject] private FinishedGameDialogService? _finishedGameDialogService = null!;
    [Inject] private ErrorService? _errorService = null!;
    
    private CancellationTokenSource _cts = new CancellationTokenSource();

    private TimeSpan _currentTime;
    public TimeSpan CurrentTime
    {
        get => _currentTime;
        set => SetProperty(ref _currentTime, value);
    }
    
    private DispatcherTimer? _timer;

    private EventHandler? _savedHandler;
    public UserGame? UserGame { get; private set; }
    
    private GameProgresses? _appInfo;
    public GameProgresses? AppInfo
    {
        get => _appInfo;
        set => SetProperty(ref _appInfo, value);
    }

    private Bitmap? _imageBitmap;

    public Bitmap? ImageBitmap
    {
        get => _imageBitmap;
        set => SetProperty(ref _imageBitmap, value);
    }

    public async Task LoadInfo()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        var userInfo = User.GetInstance().GetUserInfo();

        if (_databaseService is null) throw new NullReferenceException();

        var userGameInfo = await _databaseService.GetUserGameAsync(userInfo, _cts.Token);

        if (userGameInfo is not null && userGameInfo.AppId != 0)
        {
            UserGame = userGameInfo;
            
            var gameInfo = await _databaseService.GetFirstOrDefaultAsync<GameProgresses>
                (e => e.AppId == userGameInfo.AppId, _cts.Token);

            if (gameInfo is null)
            {
                Console.WriteLine($"Failed to get gameInfo with appID: {userGameInfo.AppId} from database");
                throw new NullReferenceException();
            }
            
            AppInfo = gameInfo;

            _savedHandler = (sender, args) => UpdateDateTimer();

            ImageBitmap = SteamService.Instance.GetImageSyncFromBytes(_appInfo.AppHeaderImage);
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += _savedHandler; 
            _timer.Start();
        }
    }

    public async Task FinishingGame()
    {
        if (_finishedGameDialogService is null || _errorService is null) throw new NullReferenceException();

        if (AppInfo is null)
        {
            _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Failed to finish game, your game is Empty"});
            return;
        }
        
        var isAdded = await _finishedGameDialogService.ShowWindowAsync(AppInfo);

        if (isAdded && UserGame is not null)
        {
            UserGame.AppId = 0;
            var isUpdate = await _databaseService.UpdateAsync(UserGame, _cts.Token);
            
            if (isUpdate) 
                ClearingContent();
        }
    }

    private void UpdateDateTimer()
    {
        if (_appInfo is not null) 
            CurrentTime = _appInfo.EndTime - DateTime.Now;
    }

    public void ClearingContent()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        
        if (_timer != null)
        {
            _timer.Stop();
            
            if (_savedHandler is not null) 
                _timer.Tick -= _savedHandler;
        }

        _savedHandler = null;
        _databaseService = null;
        UserGame = null;
        _appInfo = null;
    }
}