using System;
using System.Collections.Generic;
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
    
    delegate void RefControlDelegate();
    
    private readonly Register<string, RefControlDelegate> _lazyRegister = new();
    private readonly Register<string, UserControl> _preloadRegister = new();
    private readonly Dictionary<string, int> test;
    private readonly Action<string> _selectorAction;

    private UserControl? _oldControl = null;
    
    public MainWindow()
    {
        InitializeComponent();
        Di.Container.ResolveFieldsFromClassInstance(this);
        RegisterUiService(this);
        
        var vm = new MainWindowViewModel(new WindowService(this));
        DataContext = vm;
        
        _selectorAction = Navigate;
        
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
        mainContent.AddListener(_selectorAction);

        var tableContent = new GameTable();
        tableContent.AddListener(_selectorAction);
        
        _preloadRegister.RegisterNewObject("Main", mainContent);
        _preloadRegister.RegisterNewObject("Table", tableContent);
        
        // _lazyRegister.RegisterNewObject("Roll", DelegateSwitchFactory<RollGame>(_selectorAction));
        // _lazyRegister.RegisterNewObject("Table", DelegateSwitchFactory<GameTable>(_selectorAction));
        // _lazyRegister.RegisterNewObject("Profile", DelegateSwitchFactory<ProfileContent>(_selectorAction));
    }

    private void Navigate(string nameControl)
    {
        ControlMain.Content = null;
        
        if (_preloadRegister.GetObjectFromRegister(nameControl, out var value))
        {
            ControlMain.Content = value;
            return;
        }

        if (_lazyRegister.GetObjectFromRegister(nameControl, out var @delegate))
        {
            @delegate?.Invoke();
        }
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

    // private RefControlDelegate DelegateSwitchFactory<TUserControl>(Action<string> switchAction) TODO upgrade lifetime for users control 
    //     where TUserControl : UserControl, IAddListener, new()
    // {
    //     RefControlDelegate del = delegate() //Sending ControlMain.Control
    //     {
    //         var newClass = new TUserControl();
    //         newClass.AddListener(switchAction);
    //         
    //         _oldControl = newClass;
    //         
    //         Console.WriteLine($"Delegate is work. Created class {typeof(TUserControl).Name}." +
    //                           $" new class name: {newClass.Name}");
    //         
    //         ControlMain.Content = newClass;
    //     };
    //     
    //     return del;
    // }
}