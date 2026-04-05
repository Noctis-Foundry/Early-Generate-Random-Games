using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Service;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.Src;
using GameRandom.Src.UserData;

namespace GameRandom.ViewModels.AdminConfirmSystem;

public class RegistrationCommandFactory
{
    private const int DatabaseTimeoutSec = 5;

    /// <summary>
    /// Creates a command to grant administrative rights to a user.
    /// </summary>
    /// <param name="userInfo">The user to be promoted.</param>
    /// <param name="actionSemaphore"></param>
    /// <param name="startTaskWaiter"></param>
    /// <param name="endTaskWaiter"></param>
    /// <returns>An <see cref="AsyncRelayCommand"/> that executes the promotion logic.</returns>
    public AsyncRelayCommand AddAdminCommand(Users userInfo, SemaphoreSlim actionSemaphore, Action startTaskWaiter, Action endTaskWaiter)
    {
        return new AsyncRelayCommand(async () =>
        {
            if (Di.ResolveInstance.TryGetInstance<ErrorService>() is not ErrorService errorService)
                throw new NullReferenceException("Failed to inject error service from DI");
            
            if (!await actionSemaphore.WaitAsync(0))
            {
                errorService.ShowWindow("Wait for the previous command to complete");
                return;
            }

            if (!User.GetInstance().IsTopLevelAdmin())
                return;
            
            startTaskWaiter?.Invoke();
            
            try
            {
                if (!User.GetInstance().IsTopLevelAdmin())
                    return;

                if (Di.ResolveInstance.TryGetInstance<DatabaseService>() is { } databaseService)
                {
                    await UpdateLobby(userInfo, databaseService);

                }
                else
                    throw new NullReferenceException("Failed resolve database service");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
            finally
            {
                actionSemaphore.Release();
                endTaskWaiter?.Invoke();
            }
        });
    }

    /// <summary>
    /// Creates a command to revoke administrative rights from a user.
    /// </summary>
    /// <param name="userInfo">The user whose rights are to be revoked.</param>
    /// <param name="actionSemaphore"></param>
    /// <param name="startTaskWaiter"></param>
    /// <param name="endTaskWaiter"></param>
    /// <returns>An <see cref="AsyncRelayCommand"/> that executes the revocation logic.</returns>
    public AsyncRelayCommand RemoveAdminCommand(Users userInfo, SemaphoreSlim actionSemaphore, Action startTaskWaiter, Action endTaskWaiter)
    {
        return new AsyncRelayCommand(async () =>
        {
            if (Di.ResolveInstance.TryGetInstance<ErrorService>() is not ErrorService errorService)
                throw new NullReferenceException("Failed to inject error service from DI");
            
            if (!User.GetInstance().IsTopLevelAdmin())
                return;

            if (!await actionSemaphore.WaitAsync(0))
            {
                errorService.ShowWindow("Wait for the previous command to complete");
                return;
            }

            startTaskWaiter?.Invoke();

            try
            {
                if (Di.ResolveInstance.TryGetInstance<DatabaseService>() is not { } databaseService)
                    throw new NullReferenceException("Failed resolve database service");

                await DeleteAdminRules(databaseService, userInfo);
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
            }
            finally
            {
                actionSemaphore.Release();
                endTaskWaiter?.Invoke();
            }
        });
    }

    private async Task UpdateLobby(Users userInfo, DatabaseService databaseService)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseTimeoutSec));

        var lobbies = await databaseService.GetLobbyById(userInfo.LobbyId, cts.Token);

        if (lobbies is null)
        {
            Logger.Error("Lobby is not founded");
            return;
        }

        lobbies.AdminsList.Add(new Admins
        {
            SteamId = userInfo.SteamId,
            LobbyId = userInfo.LobbyId,
            IsTopAdmin = false
        });

        var isUpdating = await databaseService.UpdateAsync(lobbies, cts.Token);

        if (!isUpdating)
            Logger.Error("Failed to add admin");
        else
            Logger.Info("Admin is added");
    }
    
    private async Task DeleteAdminRules(DatabaseService databaseService, Users userInfo)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseTimeoutSec));

        var isRemoving =
            await databaseService.DeleteItemWithPredicate<Admins>(e => e.SteamId == userInfo.SteamId,
                cts.Token);

        if (!isRemoving)
        {
            Logger.Error("Failed to remove admin");
        }
    }
}