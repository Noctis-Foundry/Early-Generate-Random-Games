using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.Scr.WindowScr;
using GameRandom.SteamSDK.LobbySystem;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class MainWindow : Window
{
    [Inject] private LobbyService? _lobby;
    [Inject] private PostgresListener?  _postgres;
    
    private readonly Register<string, UserControl> _userControlRegister = new();
    private readonly Action<string> _changeContent;
    public MainWindow()
    {
        InitializeComponent();
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_lobby == null || _postgres == null)
            throw new InvalidOperationException("Failed connect _lobby || _postgres from DI container in MainWindow.cs");

        //Task.Run(async () => await TestingDeleteLobbyMembers());
        
        var vm = new MainWindowViewModel(new WindowService(this));
        DataContext = vm;
        
        _changeContent = Navigate;
        
        InitializeUserControlRegister();
        
        Navigate("Main");
        
        Closing += MainWindow_OnClosed;
        
        EventsConnecting();
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
        //Task.Run(async () => await TestingDeleteLobbyMembers());

        LobbyImages.Children.Clear();
        
        if (DataContext is MainWindowViewModel vm)
        {
            _postgres.Subscribe(TableEnum.LobbyContext,
                e => Dispatcher.UIThread.InvokeAsync(() => vm.UpdateLobby(LobbyImages, e)));
        }

        if (_lobby == null)
            throw new Exception("Lobby service not found");
        
        Dispatcher.UIThread.InvokeAsync(() => _lobby.StartApp());
    }
    private async Task TestingDeleteLobbyMembers()
    {
        var dbContext = new AppDbContext();
        
        foreach (var item in dbContext.LobbyContexts.ToList())
        {
            dbContext.LobbyContexts.Remove(item);
        }
        foreach (var item in dbContext.Lobbies.ToList())
        {
            dbContext.Lobbies.Remove(item);
        }
        
        await dbContext.SaveChangesAsync();
    }
    
}