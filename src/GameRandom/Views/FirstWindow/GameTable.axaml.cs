using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserData;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class GameTable : UserControl, IDisposable
{
    [Inject] private DatabaseService _databaseService = null!;
    [Inject] private ObservableConverter _converter = null!;
    [Inject] private ErrorService _errorService = null!;
    
    private readonly CancellationTokenSource _cts = new();
    
    private Action<PayloadStructure>? _savedDelegate;
    
    private Action<string>? _onShowContent;

    public GameTable()
    {
        InitializeComponent();
        Di.Container.ResolveFieldsFromClassInstance(this);
        DataContext = new GameTableViewModel();
        
        if (Design.IsDesignMode)
            return;

        Task.Run(async () => await InitializeTable(), _cts.Token);

        _savedDelegate = e => 
        {
            Dispatcher.UIThread.InvokeAsync(() => SubscribeToUpdateTable(e.TableCode));
        };
        
        if (Di.Container.TryGetInstance<PostgresListener>() is PostgresListener listener)
        {
            listener.Subscribe(TableEnum.GameProgress, _savedDelegate);
        }
        
        Dispatcher.UIThread.InvokeAsync(() => SubscribeToUpdateTable((int)TableEnum.GameProgress));
    }

    private async Task InitializeTable()
    {
        try
        {
            var gameProgresses = await _databaseService.GetTableListAsync<GameProgresses>();
            await Dispatcher.UIThread.InvokeAsync(() => UpdateTable(gameProgresses));
        }
        catch (Exception e)
        {
            _errorService.ShowErrorWindow($"An error occured while loading GameTable + {e.Message}", ErrorEnum.Error);
        }
    }
    
    public void AddListener(Action<string> onChangeContent) => _onShowContent = onChangeContent;

    private void Close(object? sender, RoutedEventArgs e)
    {
        _onShowContent?.Invoke("Main");
        // Dispose();
    }

    private async Task SubscribeToUpdateTable(int tableCode)
    {
        if (tableCode != (int)TableEnum.GameProgress)
        {
            _errorService.ShowErrorWindow($"TableCode: {tableCode} not correct", ErrorEnum.Error);
            return;
        }

        var finallyList = new List<GameProgresses>();
        
        try
        {
            var userInfo = User.GetInstance().GetUserInfo();

            var gameList = await _databaseService.GetTableListAsync<GameProgresses>();

            if (gameList is null)
            {
                _errorService.ShowErrorWindow("Not founded games", ErrorEnum.Error);
                return;
            }

            if (userInfo.LobbyID <= 0)
                finallyList = gameList.Where(x => x.PlayerID == userInfo.SteamID).ToList();
            else
            {
                Lobbies? lobbies = await _databaseService.GetLobbyById(userInfo.LobbyID);

                if (lobbies is null)
                {
                    _errorService.ShowErrorWindow($"Failed find lobby with id {userInfo.LobbyID} in db", ErrorEnum.Error);
                    bool isUpdating = await User.GetInstance().UpdateLobbyId(-1);

                    if (!isUpdating)
                    {
                        _errorService.ShowErrorWindow("Failed updating lobby id for user in database", ErrorEnum.Error);
                        return;
                    }
                }
                
                //Testing for game table TODO delete this code block and change to get lobby game progresses
                finallyList = gameList.Where(x => x.PlayerID == userInfo.SteamID).ToList();
            }
            
            UpdateTable(finallyList);
        }
        catch (Exception e)
        {
            _errorService.ShowErrorWindow($"An error occured while updating GameTable + {e.Message}", ErrorEnum.Error);
            return;
        }
    }
    
    private void UpdateTable(List<GameProgresses> gameProgress)
    {
        if (DataContext is GameTableViewModel viewModel)
            viewModel.GameProgress = _converter.ToObservableCollection(gameProgress);
    }

    public void Dispose()
    {
        _onShowContent = null;

        if (Di.Container.TryGetInstance<PostgresListener>() is PostgresListener listener)
        {
            listener.Unsubscribe(TableEnum.GameProgress, _savedDelegate);
        }

        _savedDelegate = null;
        
        _databaseService = null!;
        _errorService = null!;
        _converter = null!;
        
        _cts.Cancel();
        _cts.Dispose();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}