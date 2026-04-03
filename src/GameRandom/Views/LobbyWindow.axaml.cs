using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.Events;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.LobbySystem;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Views.LobbyModalWindow;

public sealed partial class LobbyWindow : WindowBase<LobbyWindowViewModel>
{
    private const int MaxLenghtId = 18;

    public LobbyWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        InitializeViewModel();
        InitializeDiContainer();
        InitializeProcessingHandler();
    }

    private void Connect(object? sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        
        if (!string.IsNullOrEmpty(IdBox.Text)) 
            vm.ConnectToLobby(IdBox.Text);
    }

    private void Create(object? sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        vm.CreateNewLobby();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing)
            return;

        SetInactive();
        Hide();
        
        e.Cancel = true;
    }
}