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
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;

namespace GameRandom.ViewModels;

public class AdminPanelViewModel : ViewModelBase, IDisposable
{
    [Inject] private readonly DatabaseService? _databaseService = null!;
    [Inject] private readonly AdminConfirmService? _confirmEndGameService = null!;
    [Inject] private readonly PostgresListener? _postgresListener = null!;
    
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

    private Dictionary<int, AdminPanelElementData> _gameProgresses = new();

    private ObservableCollection<AdminPanelElementData> _gameList;
    
    public ObservableCollection<AdminPanelElementData> GameList
    {
        get => _gameList;
        set => SetProperty(ref _gameList, value);
    }

    private bool _isInitialized = false;

    public AdminPanelViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService is null || _confirmEndGameService is null || _postgresListener is null) 
            throw new NullReferenceException("Failed inject instances");
        
        _postgresListener.Subscribe(TableEnum.EndGameTable, p =>
        {
            Dispatcher.UIThread.InvokeAsync(async () => await UpdateData(p));
        });
    }
    public async Task LoadGameProgresses()
    {
        if (_isInitialized) return;
        
        var gameList = await _databaseService.GetFinishedGames(_cts.Token);

        if (gameList is null)
        {
            Logger.Error("Failed to load game progresses from database");
            return;
        }
        
        OpenWithQueue = new RelayCommand( async () =>
        {
            await _confirmEndGameService.ShowWindowAsync(gameList);
        });
        
        GameList = new ObservableCollection<AdminPanelElementData>();
        
        foreach (var game in gameList)
        {
            if (game.IsImprove)
                continue;
            
            var localGame = game;
            AsyncRelayCommand openConfirmGameWindow = new AsyncRelayCommand ( async () =>
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

            if (_gameProgresses.TryAdd(game.GameProgressId, adminPanelData))
            {
                GameList.Add(adminPanelData);
            }
        }

        _isInitialized = true;
    }
    public async Task UpdateData(PayloadStructure payloadStructure)
    {
        if (payloadStructure.TableCode != (int)TableEnum.EndGameTable || payloadStructure.OpCode == (int)OperationsEnum.Delete || _databaseService is null)
            return;
        
        using var ct = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        var finishGame = await _databaseService.GetFinishedGamesFromId(payloadStructure.RowId, ct.Token);

        if (finishGame is null || finishGame.GameProgresses is null) return;

        if (_gameProgresses.TryGetValue(finishGame.GameProgressId, out var data))
        {
            data.SetGameInfo(finishGame);
            int index = GameList.IndexOf(GameList.First(x =>
                x.GameInfo.GameProgressId == data.GameInfo.GameProgressId));
            
            GameList[index] = data;
        }
        
        AsyncRelayCommand  relayCommand = new AsyncRelayCommand(async () =>
        {
            Logger.Debug($"Opening confirm window for game with id");
            await _confirmEndGameService.ShowWindowAsync(finishGame);
        });
        
        var user = await _databaseService.GetUserByUlongId(finishGame.GameProgresses.PlayerId, ct.Token);

        if (user is null) throw new Exception($"Failed to find user with {finishGame.GameProgresses.PlayerId}");
        
        var progress = new AdminPanelElementData(finishGame, relayCommand, user.Nickname);
        
        if (_gameProgresses.TryAdd(finishGame.GameProgressId, progress))
        {
            GameList.Add(progress);
            
            if (_confirmEndGameService is not null && _confirmEndGameService.IsOpen)
            {
                _confirmEndGameService.AddNextDialog(finishGame);
            }
        }
    }
    
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        
        _gameProgresses.Clear();
        _openWithQueue = null;
        OpenWithQueue = null;
        _isInitialized = false;
    }
}

public class AdminPanelElementData(FinishedGames gameInfo, AsyncRelayCommand  openCommand, string nickname)
{
    public FinishedGames GameInfo { get; private set; } = gameInfo;
    public AsyncRelayCommand  OpenCommand { get; private set; } = openCommand;
    public string NickName { get; private set; } = nickname;
    
    public void SetGameInfo(FinishedGames info) => GameInfo = info;
}