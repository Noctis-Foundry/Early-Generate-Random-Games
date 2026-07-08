using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac.Core;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using GameRandom.Scripts.UserData;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.BaseClasses;
using GameRandom.ViewModels.MainWindowSystem.Enums;
using GameRandom.ViewModels.MainWindowSystem.Interface;
using GameRandom.ViewModels.MainWindowSystem.Services;
using GameRandom.Views.MainWindowSystem;

namespace GameRandom.ViewModels.MainWindowSystem;

/// <summary>
/// ViewModel for the main application window. Manages lobby and challenge rules.
/// </summary>
public sealed class MainWindowViewModel : ViewModelBase
{
    public IControlNavigate UserControlNavigate { get; private set; }
    public IAdminLock AdminLock { get; private set; }
    public ILobbyUpdate LobbyUpdate { get; private set; }= new MainWindowUpdateLobby();

    #region BindingArea

    private ICommand _rulesOpenCommand;
    private ICommand _adminOpenCommand;
    private ICommand _lobbyOpenCommand;

    public ICommand RulesOpenCommand {
        get => _rulesOpenCommand;
        set => SetProperty(ref _rulesOpenCommand, value);
    }
    public ICommand AdminOpenCommand {
        get => _adminOpenCommand;
        set => SetProperty(ref _adminOpenCommand, value);
    }
    public ICommand LobbyOpenCommand {
        get => _lobbyOpenCommand;
        set => SetProperty(ref _lobbyOpenCommand, value);
    }

    #endregion
    
    /// <summary>
    /// Initializes a new instance of MainWindowViewModel.
    /// </summary>
    public MainWindowViewModel()
    {
        InitializeDiContainer();
        InitializeSemaphoreSlim();

        UserControlNavigate = new NavigateUserControls();
        AdminLock = new AdminLockService();

        AdminLock.Initialize();
    }

    public void InitializeCommands(Action openLobby, Action openRules)
    {
        RulesOpenCommand = new RelayCommand(openRules);
        AdminOpenCommand = new RelayCommand(OpenAdminPanel);
        LobbyOpenCommand = new RelayCommand(openLobby);
    }

    private void OpenAdminPanel()
    {
        if (!User.GetInstance().IsAdmin())
            return;
        
        UserControlNavigate.Navigate(ControlTypes.Admin);
    }
    
    public override void Dispose()
    {
        LobbyUpdate.Dispose();
        LobbyUpdate = null!;
        
        UserControlNavigate.Dispose();
        AdminLock.Dispose();

        RulesOpenCommand = null!;
        AdminOpenCommand = null!;
        LobbyOpenCommand = null!;
        
        base.Dispose();
    }
}