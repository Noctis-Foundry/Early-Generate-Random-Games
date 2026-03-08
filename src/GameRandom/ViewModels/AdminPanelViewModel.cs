using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;

namespace GameRandom.ViewModels;

public class AdminPanelViewModel : ViewModelBase, IDisposable
{
    [Inject] private readonly IDatabaseService? _databaseService = null!;
    [Inject] private readonly AdminConfirmService? _confirmEndGameService = null!;
    [Inject] private readonly PostgresListener? _postgresListener = null!;
    
    private List<AdminPanelElementData> _gameProgresses;
    private RelayCommand? _openWithQueue;

    public RelayCommand? OpenWithQueue
    {
        get => _openWithQueue;
        set => SetProperty(ref _openWithQueue, value);
    }
    public List<AdminPanelElementData> GameProgresses
    {
        get => _gameProgresses;
        set => SetProperty(ref _gameProgresses, value);
    }

    private bool _isInitialized = false;

    public AdminPanelViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService is null || _confirmEndGameService is null || _postgresListener is null) 
            throw new NullReferenceException("Failed inject instances");
        
        _postgresListener.Subscribe(TableEnum.GameProgress, new Action<PayloadStructure>(async p =>
        {
            await UpdateData(p);
        }));
    }

    public async Task LoadGameProgresses()
    {
        if (_isInitialized) return;
        
        var gameList = await _databaseService.GetTableListAsync<GameProgresses>();

        if (gameList is null)
        {
            Logger.Error("Failed to load game progresses from database");
            return;
        }

        var filteredList = gameList.Where(e => !e.IsFinished).ToList();

        if (filteredList.Count == 0)
            return;

        OpenWithQueue = new RelayCommand( async () =>
        {
            await _confirmEndGameService.OpenDialogWindowWithList(filteredList);
        });
        
        foreach (var game in filteredList)
        {
            RelayCommand openConfirmGameWindow = new RelayCommand( async () =>
            {
                await _confirmEndGameService.OpenDialogWindowAsync(game);
            });
            
            var adminPanelData = new AdminPanelElementData(game, openConfirmGameWindow);
            GameProgresses.Add(adminPanelData);
        }
    }

    public async Task UpdateData(PayloadStructure payloadStructure)
    {
        if (payloadStructure.TableCode != (int)TableEnum.EndGameTable || payloadStructure.OpCode == (int)OperationsEnum.Delete)
            return;
        
        //Взятие из таблицы и обновление данных листа
        //Апдейт данных в диалоговом окне если окно открыто
    }
    
    public void Dispose()
    {
        _gameProgresses.Clear();
        GameProgresses.Clear();
        _openWithQueue = null;
        OpenWithQueue = null;
    }
}

public class AdminPanelElementData(GameProgresses gameInfo, RelayCommand openCommand)
{
    public GameProgresses GameInfo { get; set; } = gameInfo;
    public RelayCommand OpenCommand { get; set; } = openCommand;
}