using System;
using System.Linq;
using System.Timers;
using Avalonia.Controls;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels;

public class CurrentGameStatusViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService = null!;

    private Timer _timer;
    private UpdatingInfo _updatingInfo;

    public async Task LoadInfo(GameStatusInfo uiInfo)
    {
        Di.Container.ResolveField(out _databaseService);
        
        var userInfo = User.GetInstance().GetUserInfo();

        if (_databaseService is null) throw new NullReferenceException();

        var userGameInfo = await _databaseService.GetUserGameAsync(userInfo);

        if (userGameInfo is not null && userGameInfo.AppId != 0)
        {
            var gameInfo = userGameInfo.GameProgresses;

            _updatingInfo = new UpdatingInfo(gameInfo.EndTime, uiInfo.GetTimeSpentBlock());

            _timer = new Timer(1000);
            _timer.Elapsed += (sender, e) => _updatingInfo.CalculateTime();
        }
    }

    public void CloseCurrentGameWindow()
    {
        _databaseService = null;
        
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

    public void UpdateInfo(string name, DateTime beginTime, DateTime todayDate, DateTime endTime, Bitmap gameImage)
    {
        _nameBlock.Text = name;
        _beginTime.Text = beginTime.ToString("D");
        _endTime.Text = endTime.ToString("D");
        _todayDate.Text = todayDate.ToString("D");
        _gameImage.Source = gameImage;
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
        
        _timeSpent.Text = timeLeft.ToString($"{timeLeft.Days}d/{timeLeft.Hours}h/{timeLeft.Minutes}m/{timeLeft.Seconds}");
    }
}