using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels;

public class AdminPanelViewModel : ViewModelBase, IDisposable
{
    [Inject] private readonly DatabaseService? _databaseService = null!;
    [Inject] private readonly AdminConfirmService? _confirmEndGameService = null!;
    [Inject] private readonly PostgresListener? _postgresListener = null!;
    [Inject] private readonly EventBus? _eventBus = null!;
    
    private CancellationTokenSource _cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    
    private bool _isCanShow = false;

    public bool IsCanShow
    {
        get => _isCanShow;
        set => SetProperty(ref _isCanShow, value);
    }

    private RelayCommand? _openWithQueue;

    public RelayCommand? OpenWithQueue
    {
        get => _openWithQueue;
        set => SetProperty(ref _openWithQueue, value);
    }

    private ObservableCollection<AdminPanelElementData> _gameList;
    
    public ObservableCollection<AdminPanelElementData> GameList
    {
        get => _gameList;
        set => SetProperty(ref _gameList, value);
    }

    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    private Action<PayloadStructure>? _loadAction;
    private Action<AdminRulesUpdating>? _updateRules;

    public AdminPanelViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService is null || _confirmEndGameService is null || _postgresListener is null
            || _eventBus is null) 
            throw new NullReferenceException("Failed inject instances to admin panel view model");

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
        
        _postgresListener.Subscribe(TableEnum.EndGameTable, p =>
        {
           _loadAction.Invoke(p);
        });
        
        _eventBus.Subscribe<AdminRulesUpdating>(_updateRules);
    }
    public async Task LoadGameProgresses()
    {
        if (!await _semaphoreSlim.WaitAsync(0))
        {
            Logger.Warning("Threading is not empty");
            return;
        }

        try
        {
            var gameList = await _databaseService.GetFinishedGames(_cts.Token);

            if (gameList is null)
            {
                Logger.Error("Failed to load game progresses from database");
                return;
            }

            OpenWithQueue = new RelayCommand(async () => { await _confirmEndGameService.ShowWindowAsync(gameList); });

            GameList = new ObservableCollection<AdminPanelElementData>();

            foreach (var game in gameList)
            {
                if (game.IsImprove)
                    continue;

                var localGame = game;
                AsyncRelayCommand openConfirmGameWindow = new AsyncRelayCommand(async () =>
                {
                    Logger.Debug($"Opening confirm window for game with id");

                    if (Di.Container.GetInstance<AdminConfirmService>() is AdminConfirmService dialogService)
                    {
                        dialogService.ShowWindow(localGame);
                    }
                });

                if (game.GameProgresses is null || game.GameProgresses.PlayerId == 0)
                    throw new NullReferenceException("Failed to get data from database");

                var user = await _databaseService.GetUserByUlongId(game.GameProgresses.PlayerId, _cts.Token);

                if (user is null) continue;

                var adminPanelData = new AdminPanelElementData(game, openConfirmGameWindow, user.Nickname);

                GameList.Add(adminPanelData);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            throw;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private void CheckIsAdminRules()
    {
        if (!User.GetInstance().IsTopLevelAdmin)
        {
            IsCanShow = false;
            return;
        }

        IsCanShow = true;
    }
    
    public override void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        
        _openWithQueue = null;
        OpenWithQueue = null;
        
        if (_updateRules is not null) 
            _eventBus?.Unsubscribe<AdminRulesUpdating>(_updateRules);
        
        if (_loadAction is not null) 
            _postgresListener?.Unsubscribe(TableEnum.EndGameTable, _loadAction);

        _updateRules = null;
        _loadAction = null;
    }
}

public class AdminPanelElementData(FinishedGames gameInfo, AsyncRelayCommand  openCommand, string nickname)
{
    public FinishedGames GameInfo { get; private set; } = gameInfo;
    public AsyncRelayCommand  OpenCommand { get; private set; } = openCommand;
    public string NickName { get; private set; } = nickname;
    
    public void SetGameInfo(FinishedGames info) => GameInfo = info;
}