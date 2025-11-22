using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.LobbySystem;
using GameRandom.ViewModels;

namespace GameRandom.Views.LobbyModalWindow;

public partial class LobbyWindow : Window
{
    private const int MaxLenghtId = 18;
    [Inject] private EventBus? _eventBus;
    [Inject] private LobbyService? _lobbyService;
    
    public LobbyWindow()
    {
        Console.WriteLine("Initialize Create Lobby");
        
        InitializeComponent();
        
        var viewModel = new CreateLobbyViewModel();
        DataContext = viewModel;
        
        Di.Container.RegisterSingleInstance(viewModel);
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        _eventBus = _eventBus ?? throw new InvalidOperationException("EventBus is null");
        _lobbyService = _lobbyService ?? throw new InvalidOperationException("LobbyService is null");
    }
    
    private void OnLobbyIdChanging(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (textBox.Text == null)
                return;
            
            var filtered = new string(textBox.Text.Where(char.IsDigit).ToArray());
            
           // if (filtered.Length > 19)
             //   filtered = filtered.Substring(0, 19);
            
            if (filtered != textBox.Text)
                textBox.Text = filtered;
        }
    }
    private async void Connect(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CreateLobbyViewModel viewModel)
        {
            if (long.TryParse(IdBox.Text, out var lobbyId))
            {
                await _lobbyService.ConnectToLobby(lobbyId);
            }
            else
                Logger.Error("Failed connect to the lobby");
        }
    }
    private async void Create(object? sender, RoutedEventArgs e)
    {
        await _lobbyService.CreateLobby();
    }
}