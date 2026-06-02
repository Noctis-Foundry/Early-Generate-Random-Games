using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.DependenceInjectSystem;
using GameRandom.Events;
using GameRandom.Providers;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Scripts.HandleSystem;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Factory;
using GameRandom.Src.LobbySystem;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;
using GameRandom.ViewModels.MainWindowSystem;
using GameRandom.Views.LobbyModalWindow;

namespace GameRandom.Views.MainWindowSystem;

/// <summary>
/// Primary application window managing navigation, lobby system, and UI state.
/// </summary>
public partial class MainWindow : WindowBase<MainWindowViewModel>, IInitializeMainWindow
{
    [Inject] private readonly LobbyService _lobby = null!;
    [Inject] private readonly EventBus _eventBus = null!;
    [Inject] private readonly PostgresListener _postgresListener = null!;
    [Inject] private readonly MainWindowFactory _mainWindowFactory = null!;
    [Inject] private readonly SteamService _steamService = null!;
    

    private RulesWindow _rulesWindowWindow = new();
    private LobbyWindow _lobbyWindow;

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

        if (_postgresListener is null)
            throw new NullReferenceException(nameof(_postgresListener));

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
        Closing += MainWindow_OnClosed;

        _eventBus.Subscribe<LobbyUpdate>(_ => { UpdateLobby((int)TableEnum.Lobby); });
        
        EventsConnecting();
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
    /// Updates lobby data and avatar grid on UI thread.
    /// </summary>
    /// <param name="tableCode">Database table code for validation.</param>
    private void UpdateLobby(int tableCode) //TODO create class with saving current lobby data and update lobby with event from event bus
    {
        if (DataContext is not MainWindowViewModel vm) return;

        if (tableCode != (int)TableEnum.Lobby)
            return;
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await vm.LobbyUpdate.UpdateLobby();
                await UpdateAvatarGrid();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        });
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
}