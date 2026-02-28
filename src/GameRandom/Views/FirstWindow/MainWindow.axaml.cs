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
using GameRandom.SteamSDK.Factory;
using GameRandom.SteamSDK.LobbySystem;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class MainWindow : Window
{
    [Inject] private readonly LobbyService _lobby = null!;
    [Inject] private readonly DiFactory _diFactory = null!;
    [Inject] private readonly PostgresListener  _postgres = null!;
    [Inject] private readonly EventBus _eventBus = null!;
    [Inject] private readonly UserControlFactory _controlFactory = null!;
    
    private readonly Register<string, Func<UserControl>> _preloadRegister = new();
    private readonly Action<string> _selectorAction;
    
    public MainWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        RegisterServiceWithMainWindowOwnerAndResolve(this);
        
        var vm = new MainWindowViewModel(new WindowService(this));
        DataContext = vm;
        
        _selectorAction = Navigate;
        
        InitializeUserControlRegister();
        _selectorAction.Invoke("Main");
        InitWindowEvents(vm);
    }
    
    private void InitializeUserControlRegister() //TODO Change IUserControl in MainWindowUserControlAbstract for Profile and GameTable
    {
        _preloadRegister.RegisterNewObject("Main", () => _controlFactory.CreateUserControl<MainWindowContent>(_selectorAction));
        // _preloadRegister.RegisterNewObject("Profile",() => _controlFactory.CreateUserControl<ProfileContent>(_selectorAction));
        _preloadRegister.RegisterNewObject("Roll", () =>  _controlFactory.CreateUserControl<RollGame>(_selectorAction));
        // _preloadRegister.RegisterNewObject("Table", () =>  _controlFactory.CreateUserControl<GameTable>(_selectorAction));
    }

    private void InitWindowEvents(MainWindowViewModel vm)
    {
        Closing += MainWindow_OnClosed;
        
        EventsConnecting();
        
        _eventBus.Subscribe<LobbyUpdate>(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(() => vm.UpdateLobby(LobbyImages, (int)TableEnum.Lobby));
        });
    }

    private void Navigate(string nameControl)
    {
        if (_preloadRegister.GetObjectFromRegister(nameControl, out var func))
        {
            var content = func?.Invoke();

            if (content is null)
                throw new NullReferenceException($"Failed navigate to {nameControl}");

            if (content is MainWindowUserControlAbstract value)
            {
                ControlMain.Content = value;
                value.Open();
            }
        }
    }
    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        SteamManager.GetSteamManager().ShutdownSteam();
        Environment.Exit(0);
    }
    private void EventsConnecting()
    {
        LobbyImages.Children.Clear();
        
        if (DataContext is MainWindowViewModel vm)
        {
            _postgres.Subscribe(TableEnum.Lobby,
                e => Dispatcher.UIThread.InvokeAsync(async () => await vm.UpdateLobby(LobbyImages, e.TableCode)));
        }

        if (_lobby == null)
            throw new Exception("Lobby service not found");
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await _lobby.StartApp();
        });
    }
    
    private void RegisterServiceWithMainWindowOwnerAndResolve(Window mainWindow)
    {
        Di.Container.RegisterSingleInstance(new ErrorService(mainWindow));
        Di.Container.RegisterSingleInstance(new ConfirmService(mainWindow));
        
        Di.Container.ResolveFieldsFromClassInstance(this);
    }
}