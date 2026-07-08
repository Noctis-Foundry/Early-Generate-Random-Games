using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.DISystem;
using GameRandom.Providers;
using GameRandom.Scripts;
using GameRandom.Scripts.Factory;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.HandleSystem.Interfaces;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.LobbySystem;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.SteamSDK;
using GameRandom.ViewModels.MainWindowSystem;
using GameRandom.ViewModels.MainWindowSystem.Enums;
using GameRandom.ViewModels.MainWindowSystem.Interface;

namespace GameRandom.Views.MainWindowSystem;

/// <summary>
/// Primary application window managing navigation, lobby system, and UI state.
/// </summary>
public partial class MainWindow : WindowBase<MainWindowViewModel>, IInitializeMainWindow
{
    [Inject] private readonly LobbyService _lobby = null!;
    [Inject] private readonly EventBus _eventBus = null!;
    [Inject] private readonly IRouteManager _routeManager = null!;
    [Inject] private readonly MainWindowFactory _mainWindowFactory = null!;
    [Inject] private readonly SteamService _steamService = null!;
    

    private RulesWindow _rulesWindowWindow = new();
    private LobbyWindow _lobbyWindow;

    private Func<PayloadStructure, Task> _onLobbyUpdateHandler;
    private Action<LobbyEvent> _onEventBusLobbyHandler;

    /// <summary>
    /// Initializes the main window and all subsystems.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    #region InitializeRegion

    public void InitializeUi()
    {
        var windowProvider = new WindowProvider(this);
        windowProvider.BindingInstance();
        
        InitializeViewModel();
        InitializeDiContainer();
        
        DataContext = new MainWindowViewModel();
    }

    public void SetLoadControl()
    {
        ControlMain.Content = new LoadControl();
        TopContainer.IsVisible = false;
    }

    public void EndLoadingData()
    {
        TopContainer.IsVisible = true;
        
        BindingCommands();
        InitWindowEvents();
        
        var vm = GetViewModel();
        vm.UserControlNavigate.BindingNavigateSystem();
        vm.UserControlNavigate.Navigate(ControlTypes.MainWindow);
    }

    #endregion

    #region InitializeDependence

    
    protected sealed override void InitializeDiContainer()
    {
        base.InitializeDiContainer();

        if (_lobby is null)
            throw new NullReferenceException(nameof(_lobby));

        if (_eventBus is null)
            throw new NullReferenceException(nameof(_eventBus));

        if (_routeManager is null)
            throw new NullReferenceException(nameof(_routeManager));

        if (_mainWindowFactory is null)
            throw new NullReferenceException(nameof(_mainWindowFactory));

        if (_steamService is null)
            throw new NullReferenceException(nameof(_steamService));
    }

    #endregion
    
    /// <summary>
    /// Initializes window event subscriptions.
    /// </summary>
    private void InitWindowEvents()
    {
        LobbyImages.Children.Clear();
        Closing += MainWindow_OnClosed;

        _onLobbyUpdateHandler = async (p) =>
        {
            if (p.TableCode != (int)TableEnum.Lobby)
                return;

            var viewModel = GetViewModel();

            if (viewModel == null)
                throw new NullReferenceException(nameof(viewModel));

            await viewModel.LobbyUpdate.UpdateLobby();
            await UpdateAvatarGrid();
        };
        _onEventBusLobbyHandler = (e) =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await _onLobbyUpdateHandler?.Invoke(new PayloadStructure
                {
                    TableCode = (int)TableEnum.Lobby,
                })!;
            });
        };

        _eventBus.Subscribe(_onEventBusLobbyHandler);
        _routeManager.GetRouteService(TableEnum.Lobby).Subscribe(RouteStage.View, _onLobbyUpdateHandler);
        
        InitializeLobbySystem();
    }

    /// <summary>
    /// Handles window closing event, shutting down Steam and exiting application.
    /// </summary>
    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        SteamManager.GetSteamManager().ShutdownSteam();
        Dispose();
        Environment.Exit(0);
    }

    /// <summary>
    /// Subscribes to database and lobby events.
    /// </summary>
    /// <exception cref="Exception">Thrown when lobby service is not found.</exception>
    private void InitializeLobbySystem()
    {
        if (_lobby == null)
            throw new Exception("Lobby service not found");

        Dispatcher.UIThread.InvokeAsync(async () => { await _lobby.StartApp(); });
    }

    /// <summary>
    /// Updates the avatar grid with lobby member images.
    /// </summary>
    private async Task UpdateAvatarGrid()
    {
        Logger.Debug("Start updating avatar grid");
        
        if (DataContext is not MainWindowViewModel vm) return;

        var profileList = vm.LobbyUpdate.UserContext;
        LobbyImages.Children.Clear();

        LobbyImages.ColumnDefinitions.Clear();
        int imageCount = 0;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        foreach (var profile in profileList)
        {
            Logger.Debug($"Update avatar grid with image: {profile.avatarUrl}");
            
            LobbyImages.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var image = _mainWindowFactory.CreateImageInGrid(LobbyImages, imageCount);

            image.Source = await _steamService.GetImage(profile.avatarUrl, cts.Token);
            imageCount++;
        }
    }

    private void BindingCommands()
    {
        _lobbyWindow = new LobbyWindow();
        _rulesWindowWindow = new RulesWindow();

        void OpenLobby() => _lobbyWindow.Show();
        void OpenRules() => _rulesWindowWindow.Show();

        if (DataContext is MainWindowViewModel vm)
        {
            vm.InitializeCommands(OpenLobby, OpenRules);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        
        _eventBus.Unsubscribe(_onEventBusLobbyHandler);
        _routeManager.GetRouteService(TableEnum.Lobby).Unsubscribe(RouteStage.View, _onLobbyUpdateHandler);
    }
}