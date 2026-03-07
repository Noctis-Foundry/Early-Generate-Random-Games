using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.CoreApp;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels;

public class ChooseGameViewModel : ViewModelBase, IDisposable
{
    private AppInfo? _appInfo;

    public AppInfo? AppInfo
    {
        get => _appInfo;
        set => SetProperty(ref _appInfo, value);
    }
    
    public async Task<bool> ChooseGame()
    {
        Console.WriteLine("Choose Game");

        if (_appInfo is null)
        {
            throw new NullReferenceException($"_savedContext or _imageBytes is null. _saveContext {_appInfo == null}");
        }
        
        DateTime date = DateTime.UtcNow;
        DateTime endDate = date.AddDays(30);

        var gameInfo = new GameProgresses
        {
            AppHeaderImage = _appInfo.ImageBytes,
            AppId = _appInfo.AppData.AppId,
            AppName = _appInfo.AppData.AppName,
            BeginTime = date,
            Comment = "Default",
            EndTime = endDate,
            Grade = 0,
            IsFinished = false,
            FinishTime = endDate,
            PlayerId = SteamManager.GetSteamIdAsLong()
        };

        if (Di.Container.GetInstance<DatabaseService>() is DatabaseService service)
        {
            UserGame? userGame = await service.GetUserGameAsync(User.GetInstance().GetUserInfo());

            if (userGame is null)
                throw new NullReferenceException("User game is not initialize");

            if (userGame.AppId != 0)
            {
                if (Di.Container.GetInstance<ErrorService>() is ErrorService errorService)
                {
                    errorService.ShowErrorWindow("Failed to set new game. Finish your current game", ErrorEnum.Message);
                    return false;
                }
            }
            
            bool isAdded = await service.AddItemAsync(gameInfo);
            
            if (!isAdded)
                throw new Exception("Error add item to db");

            userGame.AppId = _appInfo.AppData.AppId;
            await service.UpdateAsync(userGame);
        }

        return true;
    }

    public void LoadGameInfo(AppSavedContext appSavedContext, byte[] imageBytes)
    {
        AppInfo = new AppInfo(appSavedContext, imageBytes);
    }

    public void ShowSteamStore()
    {
        var url = $"https://store.steampowered.com/app/{_appInfo?.AppData.AppId}";
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public void Dispose()
    {
        AppInfo = null;
        _appInfo = null;
    }
}