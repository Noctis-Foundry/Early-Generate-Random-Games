using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.Scr.WindowScr;
using GameRandom.SteamSDK.Events;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class MainWindow : Window
{
    private readonly Register<string, UserControl> _userControlRegister = new();
    private readonly Action<string> _changeContent;
    public MainWindow()
    {
        InitializeComponent();
        
        var vm = new MainWindowViewModel(new WindowService(this));
        DataContext = vm;
        
        _changeContent = Navigate;
        
        InitializeUserControlRegister();
        Navigate("Main");
        
        InitializeLobby();
        
        Closing += MainWindow_OnClosed;
    }

    private void InitializeUserControlRegister()
    {
        var mainContent = new MainWindowContent();
        mainContent.AddListener(_changeContent);

        var rollContent = new RollGame();
        rollContent.AddListener(_changeContent);

        var profileContent = new ProfileContent();
        profileContent.AddListener(_changeContent);
        
        var tableContent = new GameTable();
        tableContent.AddListener(_changeContent);
        
        var rulesContent = new Rules();
        rulesContent.AddListener(_changeContent);
        
        _userControlRegister.RegisterNewObject("Main", mainContent);
        _userControlRegister.RegisterNewObject("Roll", rollContent);
        _userControlRegister.RegisterNewObject("Profile", profileContent);
        _userControlRegister.RegisterNewObject("Table", tableContent);
        _userControlRegister.RegisterNewObject("Rules", rulesContent);
    }
    private void InitializeLobby()
    {
        Task.Run(async () =>
        {
            await using var db = new AppDbContext();
            await TestingDeleteLobbyMembers(db);
        });
        
        EventsConnecting();
    }
    private void Navigate(string nameControl)
    {
        ControlMain.Content = _userControlRegister.GetObjectFromRegister(nameControl);
    }
    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        SteamManager.GetSteamManager().ShutdownSteam();
    }
    private void EventsConnecting()
    {
        var eventBus = Di.Container.TryGetInstance<EventBus>() as EventBus;
        
        if (eventBus == null)
            throw new Exception("EventBus not found");
        
        if (DataContext is MainWindowViewModel vm)
        {
            eventBus.Subscribe<LobbyUpdate>(e =>
            {
                Dispatcher.UIThread.InvokeAsync(() => vm.UpdateLobby(LobbyImages));
            });
            
            eventBus.Publish(new LobbyUpdate());
        }
    }
    private async Task TestingDeleteLobbyMembers(AppDbContext dbContext)
    {
        foreach (var item in dbContext.LobbyContexts.ToList())
        {
            dbContext.LobbyContexts.Remove(item);
        }
        
        await dbContext.SaveChangesAsync();
    }
}