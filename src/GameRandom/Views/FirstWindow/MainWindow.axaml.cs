using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.Factory;
using GameRandom.Src.LobbySystem;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.Views.LobbyModalWindow;

namespace GameRandom.Views;

/// <summary>
/// Primary application window managing navigation, lobby system, and UI state.
/// </summary>
public partial class MainWindow : Window
{
    [Inject] private readonly LobbyService _lobby = null!;
    [Inject] private readonly EventBus _eventBus = null!;
    [Inject] private readonly UserControlFactory _controlFactory = null!;
    [Inject] private readonly PostgresListener _postgresListener = null!;
    [Inject] private readonly MainWindowFactory _mainWindowFactory = null!;
    [Inject] private readonly SteamService? _steamService;

    /// <summary>
    /// Registry for user control factories mapped by navigation keys.
    /// </summary>
    private readonly Register<string, Func<UserControl>> _preloadRegister = new();
    
    /// <summary>
    /// Action delegate for navigating between user controls.
    /// </summary>
    private readonly Action<string> _changeUserControlAction;

    private readonly Rules _rules = new();
    private readonly LobbyWindow _lobbyWindow;
    private AdminPanel _adminUserControl;


    /// <summary>
    /// Initializes the main window and all subsystems.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        RegisterServiceWithMainWindowOwnerAndResolve(this);
        
        _eventBus.Subscribe<AdminRulesUpdating>(_ =>
        {
            EnableAdminPanel();
        });
        
        _lobbyWindow = new LobbyWindow();
        DataContext = new MainWindowViewModel();
        BindingCommand();
        
        EnableAdminPanel();

        _changeUserControlAction = Navigate;
        
        InitializeUserControlRegister();
        _changeUserControlAction.Invoke("Main");
        InitWindowEvents();
    }

    /// <summary>
    /// Registers navigation targets for user controls.
    /// </summary>
    private void
        InitializeUserControlRegister() //TODO Change IUserControl in MainWindowUserControlAbstract for Profile and GameTable
    {
        _preloadRegister.RegisterNewObject("Main",
            () => _controlFactory.CreateUserControl<MainWindowContent>(_changeUserControlAction));
        _preloadRegister.RegisterNewObject("Profile",
            () => _controlFactory.CreateUserControl<ProfileContent>(_changeUserControlAction));
        _preloadRegister.RegisterNewObject("Roll",
            () => _controlFactory.CreateUserControl<RollGame>(_changeUserControlAction));
        _preloadRegister.RegisterNewObject("Table",
            () => _controlFactory.CreateUserControl<GameTable>(_changeUserControlAction));
    }

    /// <summary>
    /// Initializes window event subscriptions.
    /// </summary>
    private void InitWindowEvents()
    {
        Closing += MainWindow_OnClosed;

        EventsConnecting();

        _eventBus.Subscribe<LobbyUpdate>(_ => { UpdateLobby((int)TableEnum.Lobby); });
    }

    /// <summary>
    /// Navigates to the specified user control.
    /// </summary>
    /// <param name="nameControl">Navigation key for the target control.</param>
    /// <exception cref="NullReferenceException">Thrown when control creation fails.</exception>
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

    /// <summary>
    /// Handles window closing event, shutting down Steam and exiting application.
    /// </summary>
    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        SteamManager.GetSteamManager().ShutdownSteam();
        Environment.Exit(0);
    }

    /// <summary>
    /// Subscribes to database and lobby events.
    /// </summary>
    /// <exception cref="Exception">Thrown when lobby service is not found.</exception>
    private void EventsConnecting()
    {
        LobbyImages.Children.Clear();

        _postgresListener.Subscribe(TableEnum.Lobby,
            e => UpdateLobby(e.TableCode));

        if (_lobby == null)
            throw new Exception("Lobby service not found");

        Dispatcher.UIThread.InvokeAsync(async () => { await _lobby.StartApp(); });
    }

    /// <summary>
    /// Registers services with DI container and resolves dependencies.
    /// </summary>
    /// <param name="mainWindow">The main window instance.</param>
    private void RegisterServiceWithMainWindowOwnerAndResolve(Window mainWindow)
    {
        Di.Container.RegisterSingleInstance(new ErrorService(mainWindow));
        Di.Container.RegisterSingleInstance(new ConfirmService(mainWindow));
        Di.Container.RegisterSingleInstance(new AdminConfirmService(mainWindow));
        Di.Container.RegisterSingleInstance(new FinishedGameDialogService(mainWindow));
        Di.Container.RegisterSingleInstance(new TaskWaiterWindow(mainWindow));

        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_steamService is null)
            throw new NullReferenceException();
    }

    /// <summary>
    /// Updates lobby data and avatar grid on UI thread.
    /// </summary>
    /// <param name="tableCode">Database table code for validation.</param>
    private void UpdateLobby(int tableCode)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        if (tableCode != (int)TableEnum.Lobby)
            return;
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await vm.UpdateLobby();
            await UpdateAvatarGrid();
        });
    }

    /// <summary>
    /// Updates the avatar grid with lobby member images.
    /// </summary>
    private async Task UpdateAvatarGrid()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var profileList = vm.UsersToLobby;
        LobbyImages.Children.Clear();

        LobbyImages.ColumnDefinitions.Clear();
        int imageCount = 0;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        foreach (var profile in profileList)
        {
            LobbyImages.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var image = _mainWindowFactory.CreateImageInGrid(LobbyImages, imageCount);

            image.Source = await _steamService.GetImage(profile.avatarUrl, cts.Token);
            imageCount++;
        }
    }

    /// <summary>
    /// Binds ViewModel commands to window actions.
    /// </summary>
    private void BindingCommand()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.BindingOpenLobbyCommand(() => _lobbyWindow.Show());
            vm.BindingRulesWindow(() => _rules.Show());
        }
    }

    private void EnableAdminPanel()
    {
        if (!User.GetInstance().IsAdmin())
        {
            AdminPanel.IsVisible = false;
            return;
        }
        
        _preloadRegister.RegisterNewObject("Admin", () => _controlFactory.CreateUserControl<AdminPanel>(_changeUserControlAction));
        AdminPanel.IsVisible = true;
        
        if (DataContext is MainWindowViewModel vm)
        {
            vm.BindingAdminPanel(() => Navigate("Admin"));
        }
    }
}