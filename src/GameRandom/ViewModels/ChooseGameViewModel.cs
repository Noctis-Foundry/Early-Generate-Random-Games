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
    private ChooseGameUiInfo? _uiInfo;
    private AppSavedContext? _savedContext;
    private byte[]? _imageBytes;
    
    public async Task<bool> ChooseGame()
    {
        Console.WriteLine("Choose Game");

        if (_savedContext is null || _imageBytes is null)
        {
            throw new NullReferenceException($"_savedContext or _imageBytes is null. _imageBytes {_imageBytes == null}, _saveContext {_savedContext == null}");
        }
        
        DateTime date = DateTime.UtcNow;
        DateTime endDate = date.AddDays(30);

        var gameInfo = new GameProgresses
        {
            AppHeaderImage = _imageBytes,
            AppId = _savedContext.AppId,
            AppName = _savedContext.AppName,
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

            userGame.AppId = _savedContext.AppId;
            await service.UpdateAsync(userGame);
        }

        return true;
    }

    public void LoadGameInfo(AppSavedContext appSavedContext, ChooseGameUiInfo uiInfo, byte[] imageBytes)
    {
        _uiInfo = uiInfo;
        _savedContext = appSavedContext;
        _imageBytes = imageBytes;
        
        uiInfo.AppName.Text = appSavedContext.AppName;
        uiInfo.Genres.Text = string.Join(", ", appSavedContext.AppGenres);
        uiInfo.DateRelease.Text = appSavedContext.AppReleaseYear.ToString();
        uiInfo.GameHeaderImage.Source = SteamService.Instance.GetImageSyncFromBytes(_imageBytes);
    }

    public void ShowSteamStore()
    {
        var url = $"https://store.steampowered.com/app/{_savedContext?.AppId}";
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public void Dispose()
    {
        _uiInfo = null;
    }
}

public class ChooseGameUiInfo(
    TextBlock appName,
    TextBlock genres,
    TextBlock dateRelease,
    TextBlock rating,
    TextBlock developers,
    Image gameHeaderImage)
{
    public TextBlock AppName { get; private set; } = appName;
    public TextBlock Genres { get; private set; } = genres;
    public TextBlock DateRelease { get; private set; } = dateRelease;
    public Image GameHeaderImage { get; private set; } = gameHeaderImage;
}