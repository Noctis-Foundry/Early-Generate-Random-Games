using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.LobbySystem;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Events;
using GameRandom.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameRandom.Views;

public partial class CreateLobby : Window
{
    [Inject] private CreateLobbyService? _service;
    [Inject] private EventBus? _eventBus;
    public CreateLobby()
    {
        Console.WriteLine("Initialize Create Lobby");
        
        InitializeComponent();

        var viewModel = new CreateLobbyViewModel();
        DataContext = viewModel;
        
        Di.Container.RegisterSingleInstance(viewModel);
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