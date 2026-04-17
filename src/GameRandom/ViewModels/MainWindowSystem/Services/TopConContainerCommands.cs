using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Timers;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameRandom.Scr.Service;
using GameRandom.ViewModels.MainWindowSystem.Interface;
using ReactiveUI;

namespace GameRandom.ViewModels.MainWindowSystem.Services;

public partial class TopConContainerCommands : ObservableObject, ITopConCommandBinding
{
    private static readonly RelayCommand EmptyCommand = new(() => Logger.Error("Empty command"));

    private ICommand _rulesCommand;
    private ICommand _lobbyCommand;
    private ICommand _adminCommand;

    public ICommand RulesCommand
    {
        get => _lobbyCommand;
        private set => SetProperty(ref _rulesCommand, value); 
    }
    public ICommand LobbyCommand {
        get => _lobbyCommand;
        private set => SetProperty(ref _lobbyCommand, value); 
    }
    public ICommand AdminCommand{
        get => _lobbyCommand;
        private set => SetProperty(ref _adminCommand, value); 
    }
    
    public void BindingRules(Action func)
    {
        RulesCommand = new RelayCommand(func);
    }

    public void BindingLobby(Action func)
    {
        LobbyCommand = new RelayCommand(func);
    }

    public void BindingAdmin(Action func)
    {
        AdminCommand = new RelayCommand(func);
    }

    public void Dispose()
    {
        _adminCommand = null!;
        _rulesCommand = null!;
        _lobbyCommand = null!;

        AdminCommand = null!;
        RulesCommand = null!;
        LobbyCommand = null!;
    }
}