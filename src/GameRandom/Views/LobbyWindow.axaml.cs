using System.ComponentModel;
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.Events;
using GameRandom.Scr.LobbySystem;
using GameRandom.Scr.Service;
using GameRandom.Scr.DI;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Events;
using GameRandom.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameRandom.Views.LobbyModalWindow;

public partial class LobbyWindow : Window
{
    private const int MaxLenghtId = 18;
    private readonly LobbyService _lobbyService;
    [Inject] private CreateLobbyService? _service;
    [Inject] private EventBus? _eventBus;
    
    public LobbyWindow()
    {
        Console.WriteLine("Initialize Create Lobby");
        
        InitializeComponent();
        
        var viewModel = new CreateLobbyViewModel();
        DataContext = viewModel;
        
        Di.Container.RegisterSingleInstance(viewModel);
        
        if (Di.Container.TryGetInstance<LobbyService>() is LobbyService system)
        {
            _lobbyService = system;
        }
        
        
    }
    
    
    private void OnLobbyIdChanging(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (textBox.Text == null)
                return;
            
            var filtered = new string(textBox.Text.Where(char.IsDigit).ToArray());
            
            if (filtered.Length > 18)
                filtered = filtered.Substring(0, 18);
            
            if (filtered != textBox.Text)
                textBox.Text = filtered;
        }
    }

    private void ConnectToLobby(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(IdBox.Text))
            return; //Добавить блок для отображения ошибок

        if (uint.TryParse(IdBox.Text, out var id))
        {
            _lobbyService.ConnectToLobby(id);
        }
    }
    
    private void CreateNewLobby(object? sender, RoutedEventArgs e)
    {
        if (_service == null)
            throw new Exception("Failed to create new lobby. Create lobby service is null");
        
        _service.CreateLobby();

        if (_eventBus == null)
        {
            Logger.Error("CreateLobby: EventBus is null");
            return;
        }
            
        _eventBus.Publish(new LobbyUpdate());
    }
}