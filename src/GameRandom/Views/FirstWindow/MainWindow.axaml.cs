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
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.LobbySystem;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class MainWindow : Window
{
    [Inject] private readonly LobbyService _lobby = null!;
    [Inject] private readonly DiFactory _diFactory = null!;
    [Inject] private readonly PostgresListener  _postgres = null!;
    [Inject] private readonly EventBus _eventBus = null!;
    
    private readonly Register<string, UserControl> _userControlRegister = new();
    private readonly Action<string> _changeContent;
    public MainWindow()
    {
        InitializeComponent();
        Di.Container.ResolveFieldsFromClassInstance(this);
        RegisterUiService(this);
        
        var vm = new MainWindowViewModel(new WindowService(this));
        DataContext = vm;
        
        _changeContent = Navigate;
        
        InitializeUserControlRegister();
        
        Navigate("Main");
        
        Closing += MainWindow_OnClosed;
        
        EventsConnecting();
        
        _eventBus.Subscribe<LobbyUpdate>(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(() => vm.UpdateLobby(LobbyImages, (int)TableEnum.LobbyContext));
        });
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
        
        _userControlRegister.RegisterNewObject("Main", mainContent);
        _userControlRegister.RegisterNewObject("Roll", rollContent);
        _userControlRegister.RegisterNewObject("Profile", profileContent);
        _userControlRegister.RegisterNewObject("Table", tableContent);
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
        //Task.Run(async () => await TestingDeleteLobbyMembers()); //Deleted all position on Lobby and LobbyUserContext tables

        LobbyImages.Children.Clear();
        
        if (DataContext is MainWindowViewModel vm)
        {
            _postgres.Subscribe(TableEnum.LobbyContext,
                e => Dispatcher.UIThread.InvokeAsync(async () => await vm.UpdateLobby(LobbyImages, e.TableCode)));
        }

        if (_lobby == null)
            throw new Exception("Lobby service not found");
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await _lobby.StartApp();
        });
    }
    
    private void RegisterUiService(Window window)
    {
        if (window is MainWindow mainWindow)
            _diFactory.Create(new ErrorService(), mainWindow);
        else
            throw new Exception("Window not found");
    }
    
    private async Task TestingDeleteLobbyMembers()
    {
        var dbContext = new AppDbContext();
        
        foreach (var item in dbContext.LobbyUserContext.ToList())
        {
            dbContext.LobbyUserContext.Remove(item);
        }
        foreach (var item in dbContext.Lobbies.ToList())
        {
            dbContext.Lobbies.Remove(item);
        }
        
        await dbContext.SaveChangesAsync();
    }
    
}