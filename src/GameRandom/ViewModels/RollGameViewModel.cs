using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.SteamSDK;

namespace GameRandom.ViewModels;

public class RollGameViewModel : ViewModelBase
{
    public async Task ChooseGame(AppSavedContext savedContext)
    {
        Console.WriteLine("Choose Game");
        
        var eventBus = Di.Container.GetInstance<EventBus>() as EventBus;
        var instance = SteamManager.GetSteamManager();
        DateTime date = DateTime.UtcNow;
        DateTime endDate = date.AddDays(30);
        
        await using (var db = new AppDbContext())
        {
            GameProgress gameProgress = new GameProgress
            {
                ClientId = instance.GetSteamId().m_SteamID,
                DataBegin = $"{date:yy-MM-dd}",
                DataEnd = $"{endDate:yy-MM-dd}",
                GameName = savedContext.AppName,
                Comment = "Empty",
                IsFinished = false,
            };
            
            db.GameTables.Add(gameProgress);
            await db.SaveChangesAsync();

            Process.Start(new ProcessStartInfo
            {
                FileName = AppUrl(savedContext.AppId),
                UseShellExecute = true
            });
        }
    }
    
    private string AppUrl(int appId)
    {
        return $"https://store.steampowered.com/app/{appId}/?I=english";
    }
}