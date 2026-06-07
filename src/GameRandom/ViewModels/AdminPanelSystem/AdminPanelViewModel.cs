using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Events;
using GameRandom.Scripts.HandleSystem;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.HandleSystem.RoutSystem;
using GameRandom.Src;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.AdminSystem.Interface;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.ViewModels.AdminPanelSystem;

/// <summary>
/// ViewModel for the admin panel, responsible for managing finished game processes and admin rules.
/// </summary>
public sealed class AdminPanelViewModel : ViewModelBase
{
    [Inject] private IRouteManager _routeManager = null!;
    [Inject] private EventBus _eventBus = null!;

    private IAdminLoad _adminLoadService = new AdminPanelLoadService();
    private Action<AdminRulesUpdate> _adminRulesUpdate;

    #region BindingProperty

    private bool _isCanShow = false;

    /// <summary>
    /// Gets or sets a value indicating whether the admin panel can be shown to the current user.
    /// </summary>
    public bool IsCanShow
    {
        get => _isCanShow;
        set => SetProperty(ref _isCanShow, value);
    }

    private AsyncRelayCommand? _openWithQueue;

    /// <summary>
    /// Command to open the confirmation window for the game queue.
    /// </summary>
    public AsyncRelayCommand? OpenWithQueue
    {
        get => _openWithQueue;
        set => SetProperty(ref _openWithQueue, value);
    }

    private ObservableCollection<AdminPanelElementData> _gameList;

    /// <summary>
    /// Collection of game data displayed in the admin panel.
    /// </summary>
    public ObservableCollection<AdminPanelElementData> GameList
    {
        get => _gameList;
        set => SetProperty(ref _gameList, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminPanelViewModel"/> class.
    /// Resolves dependencies and initializes listeners.
    /// </summary>
    public AdminPanelViewModel()
    {
        InitializeDiContainer();
        InitializeListeners();
        InitializeSemaphoreSlim();

        GameList = new ObservableCollection<AdminPanelElementData>();
    }

    protected override void InitializeDiContainer()
    {
        base.InitializeDiContainer();

        if (_routeManager == null)
            throw new ArgumentNullException(nameof(_routeManager));
    }

    /// <summary>
    /// Checks if the current user has admin rules and updates <see cref="IsCanShow"/>.
    /// </summary>
    private void CheckIsAdminRules()
    {
        if (!User.GetInstance().IsTopLevelAdmin())
        {
            IsCanShow = false;
            return;
        }

        IsCanShow = true;
    }

    public async Task<bool> LoadGameProgresses()
    {
        if (!await SemaphoreSlim.WaitAsync(0))
        {
            ErrorService.ShowWindow("Failed to load game progress. Load progress is not ending");
            return false;
        }

        StartTaskWaiter();

        var result = await TaskRunner.RunWithFinallyActionT<List<AdminPanelElementData>>(async () =>
            await _adminLoadService.LoadElementsData(), CloseTaskWaiterWithSemaphore);

        if (result is { Success: false, Value: null })
            return false;

        GameList = new ObservableCollection<AdminPanelElementData>(result.Value!);

        OpenWithQueue = new AsyncRelayCommand(async () =>
        {
            if (Di.ResolveInstance.TryGetInstance<AdminConfirmService>() is not { } adminConfirmService)
                throw new NullReferenceException(nameof(adminConfirmService));

            await adminConfirmService.ShowWindowAsync(GameList);
        });

        return true;
    }

    /// <summary>
    /// Initializes listeners for database updates and internal events.
    /// </summary>
    private void InitializeListeners()
    {
        _routeManager.GetRouteService(TableEnum.EndGameTable).Subscribe(RouteStage.View, LoadGameProgresses);
        _adminRulesUpdate = _ => CheckIsAdminRules();
    }

    /// <summary>
    /// Releases resources used by the <see cref="AdminPanelViewModel"/>.
    /// </summary>
    public override void Dispose()
    {
        _adminLoadService.Dispose();
        _adminLoadService = null!;

        _openWithQueue = null;
        OpenWithQueue = null;

        _eventBus?.Unsubscribe(_adminRulesUpdate);

        _adminRulesUpdate = null!;

        base.Dispose();
    }
}