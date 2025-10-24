using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Events;
using GameRandom.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameRandom.Views;

public partial class CreateLobby : Window
{
    private LobbySystem _system;
    public CreateLobby()
    {
        InitializeComponent();

        DataContext = new CreateLobbyViewModel();

        if (Design.IsDesignMode)
            return;
        
        if (Di.Container.TryGetInstance<LobbySystem>() is LobbySystem system)
        {
            _system = system;
        }
        else
        {
            throw new Exception("Unable to find LobbySystem");
        }
    }

    private async void CreateNewLobby(object? sender, RoutedEventArgs e)
    {
        await using (var db = new AppDbContext())
        {
            var memberList = await db.LobbyContexts.ToListAsync();

            if (memberList.Count <= 0)
                await _system.CreateLobby();
            else
                await _system.CreateLobby(memberList);
        }

        if (Di.Container.TryGetInstance<EventBus>() is EventBus bus)
        {
            bus.Publish(new LobbyUpdate());
        }
    }
}