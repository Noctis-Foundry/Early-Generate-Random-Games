using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels;

public class RollGameViewModel : ViewModelBase
{
    public async Task ChooseGame(AppSavedContext savedContext, byte[] imageBytes)
    {
        Console.WriteLine("Choose Game");
        
        DateTime date = DateTime.UtcNow;
        DateTime endDate = date.AddDays(30);

        var gameInfo = new GameProgresses
        {
            AppHeaderImage = imageBytes,
            AppId = savedContext.AppId,
            AppName = savedContext.AppName,
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
                    return;
                }
            }
            
            bool isAdded = await service.AddItemAsync(gameInfo);
            
            if (!isAdded)
                throw new Exception("Error add item to db");

            userGame.AppId = savedContext.AppId;
            await service.UpdateAsync(userGame);
        }
    }
}