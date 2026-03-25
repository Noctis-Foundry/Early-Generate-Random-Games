using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.UserData;

namespace GameRandom.ViewModels.AdminSystem;

/// <summary>
/// ViewModel for the admin panel, responsible for managing finished game processes and admin rules.
/// </summary>
public class AdminPanelViewModel : ViewModelBase
{
    [Inject] private readonly DatabaseService? _databaseService = null!;
    [Inject] private readonly AdminConfirmService? _confirmEndGameService = null!;
    [Inject] private readonly PostgresListener? _postgresListener = null!;
    [Inject] private readonly EventBus? _eventBus = null!;
    
    private CancellationTokenSource _cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    
    private bool _isCanShow = false;

    /// <summary>
    /// Gets or sets a value indicating whether the admin panel can be shown to the current user.
    /// </summary>
    public bool IsCanShow
    {
        get => _isCanShow;
        set => SetProperty(ref _isCanShow, value);
    }

    private RelayCommand? _openWithQueue;

    /// <summary>
    /// Command to open the confirmation window for the game queue.
    /// </summary>
    public RelayCommand? OpenWithQueue
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

    /// <summary>
    /// Semaphore to ensure thread-safe operations on game progress loading.
    /// </summary>
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    private Action<PayloadStructure> _loadAction;
    private Action<AdminRulesUpdating> _updateRules;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminPanelViewModel"/> class.
    /// Resolves dependencies and initializes listeners.
    /// </summary>
    public AdminPanelViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        EnsureServicesInjected();

        InitializeListeners();
    }

    /// <summary>
    /// Asynchronously loads game progresses from the database and updates the <see cref="GameList"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadGameProgresses()
    {
        if (!await _semaphoreSlim.WaitAsync(0))
        {
            Logger.Warning("Threading is not empty");
            return;
        }

        try
        {
            var gameList = await GetFinishedGame();
            
            if (gameList is null) return;

            OpenWithQueue = new RelayCommand(async () => { await _confirmEndGameService.ShowWindowAsync(gameList); });

            GameList = new ObservableCollection<AdminPanelElementData>();

            foreach (var game in gameList)
            {
                if (!IterationRequired(game))
                    continue;
                
                var user = await _databaseService.GetUserByUlongId(game.GameProgresses.PlayerId, _cts.Token);

                if (user is null) continue;
                
                if (CreateAdminData(user, game) is { } adminPanelData) 
                    GameList.Add(adminPanelData);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// Retrieves the list of finished games from the database.
    /// </summary>
    /// <returns>A task that returns a list of finished games, or null if loading fails.</returns>
    private async Task<List<FinishedGames>?> GetFinishedGame()
    {
        var gameList = await _databaseService.GetFinishedGames(_cts.Token);

        if (gameList is null)
        {
            Logger.Error("Failed to load game progresses from database");
            return null;
        }
        
        return gameList;
    }

    /// <summary>
    /// Determines if a game iteration is required based on its status.
    /// </summary>
    /// <param name="game">The finished game to check.</param>
    /// <returns>True if the game should be processed; otherwise, false.</returns>
    private bool IterationRequired(FinishedGames game)
    {
        if (game.IsImprove)
            return false;

        if (game.GameProgresses is null || game.GameProgresses.PlayerId == 0)
            return false;

        return true;
    }
    
    /// <summary>
    /// Ensures that all injected services are properly initialized.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if a required service is null.</exception>
    private void EnsureServicesInjected()
    {
        if (_databaseService is null)
            throw new NullReferenceException(nameof(_databaseService) + " is not injected");

        if (_confirmEndGameService is null)
            throw new NullReferenceException(nameof(_confirmEndGameService) + " is not injected");

        if (_postgresListener is null)
            throw new NullReferenceException(nameof(_postgresListener) + " is not injected");

        if (_eventBus is null)
            throw new NullReferenceException(nameof(_eventBus) + " is not injected");
    }

    /// <summary>
    /// Creates admin data for a specific game and user.
    /// </summary>
    /// <param name="user">The user associated with the game.</param>
    /// <param name="game">The finished game object.</param>
    /// <returns>A new <see cref="AdminPanelElementData"/> object, or null if data is invalid.</returns>
    private AdminPanelElementData? CreateAdminData(Users user, FinishedGames game)
    {
        AsyncRelayCommand openConfirmGameWindow = new AsyncRelayCommand(async () =>
        {
            Logger.Debug($"Opening confirm window for game with id");

            if (Di.Container.GetInstance<AdminConfirmService>() is AdminConfirmService dialogService)
            {
                dialogService.ShowWindow(game);
            }
        });

        if (string.IsNullOrEmpty(user.Nickname))
            return null;
                
        return new AdminPanelElementData(game, openConfirmGameWindow, user.Nickname);
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

    /// <summary>
    /// Initializes listeners for database updates and internal events.
    /// </summary>
    private void InitializeListeners()
    {
        _loadAction += p =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (p.TableCode != (int)TableEnum.EndGameTable)
                    return;

                await LoadGameProgresses();
            });
        };
        
        _updateRules += _ => CheckIsAdminRules();

        _postgresListener?.Subscribe(TableEnum.EndGameTable, _loadAction);
        
        _eventBus?.Subscribe<AdminRulesUpdating>(_updateRules);
    }
    
    /// <summary>
    /// Releases resources used by the <see cref="AdminPanelViewModel"/>.
    /// </summary>
    public override void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        
        _openWithQueue = null;
        OpenWithQueue = null;
        
        _eventBus?.Unsubscribe<AdminRulesUpdating>(_updateRules);
        _postgresListener?.Unsubscribe(TableEnum.EndGameTable, _loadAction);

        _updateRules = null!;
        _loadAction = null!;
        
        base.Dispose();
    }
}

/// <summary>
/// Data structure representing a single element in the admin panel's game list.
/// </summary>
/// <param name="gameInfo">Information about the finished game.</param>
/// <param name="openCommand">Command to open the confirmation window.</param>
/// <param name="nickname">The player's nickname.</param>
public class AdminPanelElementData(FinishedGames gameInfo, AsyncRelayCommand  openCommand, string nickname)
{
    /// <summary>
    /// Gets information about the finished game.
    /// </summary>
    public FinishedGames GameInfo { get; private set; } = gameInfo;

    /// <summary>
    /// Gets the command to open the game confirmation window.
    /// </summary>
    public AsyncRelayCommand  OpenCommand { get; private set; } = openCommand;

    /// <summary>
    /// Gets the player's nickname.
    /// </summary>
    public string NickName { get; private set; } = nickname;
}