using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;

namespace GameRandom.Views;

public partial class CreateLobby : Window
{
    [Inject] private EventBus? _eventBus;
    public CreateLobby()
    {
        Console.WriteLine("Initialize Create Lobby");
        InitializeComponent();
    }

    private void CreateNewLobby(object? sender, RoutedEventArgs e)
    {
        
    }
}