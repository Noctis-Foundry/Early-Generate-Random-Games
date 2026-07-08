using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.DbContext;
using GameRandom.Scripts.UserData;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.AdminConfirmSystem.Interface;
using GameRandom.ViewModels.AdminRegistrationSystem.Interface;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.ViewModels.AdminRegistrationSystem;

public class RegistrationLoad : BaseModelService, IRegistrationLoad
{
    private List<AdminRegistrationData> _admins = new();
    private RegistrationCommandFactory _commandFactory = new();
    
    private Action _refStartTaskWaiter;
    private Action _refEndTaskWaiter;
    private SemaphoreSlim _refActionSemaphore;
    
    private const string AddAdmin = "Add admin";
    private const string RemoveAdmin = "Remove admin";
    
    public RegistrationLoad(Action refStartTaskWaiter, Action refEndTaskWaiter, SemaphoreSlim refActionSemaphore)
    {
        _refStartTaskWaiter = refStartTaskWaiter;
        _refEndTaskWaiter = refEndTaskWaiter;
        _refActionSemaphore = refActionSemaphore;
    }

    public async Task<List<AdminRegistrationData>?> LoadRegistrations()
    {
        _admins.Clear();
        
        var userInfo = User.GetInstance().GetUserInfo();

        if (userInfo.LobbyId <= 0)
            return null!;
        
        var currentLobby = await DatabaseService.GetLobbyById(userInfo.LobbyId);

        if (currentLobby == null)
            throw new Exception("Lobby is not found");

        foreach (var user in await NotAdminUsers(currentLobby))
        {
            var isAddCommand = _commandFactory.AddAdminCommand(user, _refActionSemaphore, _refStartTaskWaiter, _refEndTaskWaiter);
            
            _admins.Add(new AdminRegistrationData(user, AddAdmin, isAddCommand, false));
        }

        return _admins;
    }
    
    /// <summary>
    /// Filters members of the lobby who are not currently admins.
    /// </summary>
    /// <param name="lobbies">The lobby containing members.</param>
    /// <returns>A list of users who are not admins.</returns>
    private async Task<List<Users>> NotAdminUsers(Lobbies lobbies)
    {
        var users = new List<Users>();

        foreach (var lobbyMember in lobbies.LobbyData)
        {
            if (lobbyMember.UserId == User.GetInstance().GetUserId())
                continue;

            var user = await DatabaseService.GetUserByUlongId(lobbyMember.UserId);

            if (user is null)
                continue;

            if (lobbies.AdminsList.Exists(e => e.SteamId == lobbyMember.UserId))
            {
                AddNewAdminToList(lobbies, user);
                continue;
            }

            users.Add(user);
        }

        return users;
    }
    
    /// <summary>
    /// Adds a user to the admin list if they are recognized as an admin in the lobby data.
    /// </summary>
    /// <param name="lobbies">The lobby context.</param>
    /// <param name="user">The user to check and add.</param>
    private void AddNewAdminToList(Lobbies lobbies, Users user)
    {
        var admin = lobbies.AdminsList.Find(e => e.SteamId == user.SteamId);

        if (admin is null || admin.IsTopAdmin)
            return;

        var isRemoveCommand =
            _commandFactory.RemoveAdminCommand(user, _refActionSemaphore, _refStartTaskWaiter, _refEndTaskWaiter);
        
        _admins.Add(new AdminRegistrationData(user, RemoveAdmin,isRemoveCommand, true));
    }

    public override void Dispose()
    {
        _refActionSemaphore = null!;
        _refEndTaskWaiter = null!;
        _refStartTaskWaiter = null!;
        
        _admins.Clear();
        _admins = null!;
        
        _commandFactory = null!;
        
        base.Dispose();
    }
}