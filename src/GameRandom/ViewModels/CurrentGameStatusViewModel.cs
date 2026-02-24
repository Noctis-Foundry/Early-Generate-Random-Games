using System;
using System.Linq;
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
    
    private DispatcherTimer? _timer;
    private UpdatingInfo? _updatingInfo;
    private GameStatusInfo _gameStatusInfo;

    private EventHandler? _savedHandler;
    
    public UserGame? UserGame { get; private set; }

    public async Task LoadInfo(GameStatusInfo uiInfo)
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        _gameStatusInfo = uiInfo;
        
        var userInfo = User.GetInstance().GetUserInfo();

        if (_databaseService is null) throw new NullReferenceException();

        var userGameInfo = await _databaseService.GetUserGameAsync(userInfo);

        if (userGameInfo is not null && userGameInfo.AppId != 0)
        {
            UserGame = userGameInfo;
            
            var gameInfo = await _databaseService.GetFirstOrDefaultAsync<GameProgresses>
                (e => e.AppId == userGameInfo.AppId);

            if (gameInfo is null)
            {
                Console.WriteLine($"Failed to get gameInfo with appID: {userGameInfo.AppId} from database");
                throw new NullReferenceException();
            }
            
            _updatingInfo = new UpdatingInfo(gameInfo.EndTime, uiInfo.GetTimeSpentBlock());

            _updatingInfo.CalculateTime();
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _savedHandler = (sender, args) => _updatingInfo.CalculateTime(); 
            _timer.Tick += _savedHandler; 
            _timer.Start();
            
            uiInfo.UpdateInfo(gameInfo.AppName, gameInfo.BeginTime, DateTime.Now, gameInfo.EndTime, SteamService.Instance.GetImageSyncFromBytes(gameInfo.AppHeaderImage));
        }
    }

    public async Task FinishingGame()
    {
        if (UserGame is null || _databaseService == null) throw new NullReferenceException();

        var finishedGame =
            await _databaseService.GetFirstOrDefaultAsync<GameProgresses>(e =>
                e.AppId == UserGame.AppId && !e.IsFinished && e.PlayerId == UserGame.UserId);

        if (finishedGame is null)
            return;
        
        finishedGame.IsFinished = true;
        finishedGame.FinishTime = DateTime.UtcNow;
        
        UserGame.AppId = 0;
        var isUpdating = await _databaseService.UpdateAsync(UserGame);
        
        if (!isUpdating) throw new Exception("Failed to update user game");
        
        isUpdating = await _databaseService.UpdateAsync(finishedGame);
        if (!isUpdating) throw new Exception("Failed to update game progresses");
        
        _gameStatusInfo.GameIsFinished();
        _timer.Stop();
    }

    public void ClearingContent()
    {
        if (_timer != null)
        {
            _timer.Stop();
            
            if (_savedHandler is not null) 
                _timer.Tick -= _savedHandler;
        }

        _savedHandler = null;
        
        _gameStatusInfo.Dispose();
        _updatingInfo?.Dispose();

        _databaseService = null;
    }
}

public class GameStatusInfo(TextBlock nameBlock, TextBlock beginTime, TextBlock todayDate, TextBlock timeSpent, TextBlock endTime, Image gameImage) : IDisposable
{
    private TextBlock? _nameBlock = nameBlock;
    private TextBlock? _beginTime = beginTime;
    private TextBlock? _todayDate = todayDate;
    private TextBlock? _timeSpent = timeSpent;
    private TextBlock? _endTime = endTime;
    private Image? _gameImage = gameImage;
    
    private const string EmptyImage = "Assets/steamAwatarWithNight.jpg";

    public void UpdateInfo(string? name, DateTime beginTime, DateTime todayDate, DateTime endTime, Bitmap? gameImage)
    {
        if (!IsValidation()) return;
        
        _nameBlock.Text = name ?? "Default";
        _beginTime.Text = "Start time: " + beginTime.ToString("D");
        _endTime.Text =  "End time: " + endTime.ToString("D");
        _todayDate.Text = "Today: " + todayDate.ToString("D");
        _gameImage.Source = gameImage ?? AvaloniaService.CreateBitmapFromPath(EmptyImage);
    }

    public TextBlock GetTimeSpentBlock()
    {
        return _timeSpent;
    }

    private bool IsValidation()
    {
        return _nameBlock != null && _beginTime != null && _todayDate != null && _timeSpent != null && _endTime != null && _gameImage != null;
    }

    public void GameIsFinished()
    {
        if (IsValidation())
        {
            _nameBlock.Text = "App name:";
            _beginTime.Text = "Begin time: ";
            _todayDate.Text = "Today Date:";
            _timeSpent.Text = "Time spent:";
            _endTime.Text = "End Time:";
            _gameImage.Source = AvaloniaService.CreateBitmapFromPath(EmptyImage);
        }
    }
    
    public void Dispose()
    {
        if (IsValidation())
        {
            _nameBlock.Text = String.Empty;
            _beginTime.Text = String.Empty;
            _todayDate.Text = String.Empty;
            _timeSpent.Text = String.Empty;
            _endTime.Text = String.Empty;
            
            _gameImage.Source = AvaloniaService.CreateBitmapFromPath(EmptyImage);
        }
        
        _nameBlock = null;
        _beginTime = null;
        _todayDate = null;
        _timeSpent = null;
        _endTime = null;
        _gameImage = null;
    }
}

public class UpdatingInfo(DateTime endTime, TextBlock timeSpent) : IDisposable
{
    private DateTime _endTime = endTime;
    private TextBlock? _timeSpent = timeSpent;

    public void CalculateTime()
    {
        var timeNow = DateTime.Now;
        var timeLeft = _endTime - timeNow;

        if (_timeSpent == null)
            return;
        
        _timeSpent.Text = $"Left Time: {timeLeft.Days}d/{timeLeft.Hours}h/{timeLeft.Minutes}m/{timeLeft.Seconds}s";
    }

    public void Dispose()
    {
        _timeSpent = null;
    }
}