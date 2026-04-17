using System;
using System.Windows.Input;

namespace GameRandom.ViewModels.MainWindowSystem.Interface;

public interface ITopConCommandBinding
{
    public ICommand RulesCommand { get; }
    public ICommand LobbyCommand { get; }
    public ICommand AdminCommand { get; }
    
    public void BindingRules(Action func);
    public void BindingLobby(Action func);
    public void BindingAdmin(Action func);

    public void Dispose();
}

