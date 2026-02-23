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

    private DispatcherTimer _timer;
    private UpdatingInfo _updatingInfo;

    public async Task LoadInfo(GameStatusInfo uiInfo)
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        var userInfo = User.GetInstance().GetUserInfo();

        if (_databaseService is null) throw new NullReferenceException();

        var userGameInfo = await _databaseService.GetUserGameAsync(userInfo);

        if (userGameInfo is not null && userGameInfo.AppId != 0)
        {
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
            _timer.Tick += (sender, args) => _updatingInfo.CalculateTime(); 
            _timer.Start();
            
            uiInfo.UpdateInfo(gameInfo.AppName, gameInfo.BeginTime, DateTime.Now, gameInfo.EndTime, SteamService.Instance.GetImageSyncFromBytes(gameInfo.AppHeaderImage));
        }
    }

    public void CloseCurrentGameWindow()
    {
        _databaseService = null;
        
        _timer.Stop();
    }
}

public class GameStatusInfo(TextBlock nameBlock, TextBlock beginTime, TextBlock todayDate, TextBlock timeSpent, TextBlock endTime, Image gameImage)
{
    private readonly TextBlock _nameBlock = nameBlock;
    private readonly TextBlock _beginTime = beginTime;
    private readonly TextBlock _todayDate = todayDate;
    private readonly TextBlock _timeSpent = timeSpent;
    private readonly TextBlock _endTime = endTime;
    private readonly Image _gameImage = gameImage;

    public void UpdateInfo(string? name, DateTime beginTime, DateTime todayDate, DateTime endTime, Bitmap? gameImage)
    {
        _nameBlock.Text = name ?? "Default";
        _beginTime.Text = "Start time: " + beginTime.ToString("D");
        _endTime.Text =  "End time: " + endTime.ToString("D");
        _todayDate.Text = "Today: " + todayDate.ToString("D");
        
        _gameImage.Source = gameImage ?? new Bitmap("Assets/steamAwatarWithNight.jpg");
    }

    public TextBlock GetTimeSpentBlock()
    {
        return _timeSpent;
    }
}

public class UpdatingInfo(DateTime endTime, TextBlock timeSpent)
{
    private DateTime _endTime = endTime;
    private TextBlock _timeSpent = timeSpent;

    public void CalculateTime()
    {
        var timeNow = DateTime.Now;
        var timeLeft = _endTime - timeNow;
        
        _timeSpent.Text = $"Left Time: {timeLeft.Days}d/{timeLeft.Hours}h/{timeLeft.Minutes}m/{timeLeft.Seconds}s";
    }
}