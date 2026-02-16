using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;

namespace GameRandom.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _database = null!;
    [Inject] private ErrorService? _errorService = null!;
    [Inject] private ObservableConverter? _converter = null!;
    
    private ObservableCollection<ProfileTableData> _gameProgresses;

    public ObservableCollection<ProfileTableData>? GameProgresses
    {
        get => _gameProgresses;
        set => SetProperty(ref _gameProgresses, value);
    }

    /// <summary>
    /// Method starting operations and threads for load table with player games to profile table
    /// </summary>
    public async Task LoadTable()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        ulong userId = SteamManager.GetSteamManager().GetSteamId().m_SteamID;
        
        var list = await _database.GetTableListAsync<GameProgress>();

        if (list == null || list.Count == 0)
        {
            _errorService.ShowErrorWindow("Failed donwoload data from database. Game progress table is empty", ErrorEnum.Message);
            return;
        }
        
        var playerTable = ToPlayerTableData(list, userId);

        if (playerTable == null)
        {
            _errorService.ShowErrorWindow($"Failed rebind data to player table data. Games for your client id: {userId} is not founded",
                ErrorEnum.Message);
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            GameProgresses = _converter.ToObservableCollection(playerTable);
        });
    }
    
    /// <summary>
    /// Sorting GameProgress with client ID and converting to ProfileTableData
    /// </summary>
    /// <param name="gameProgress">List with all games</param>
    /// <param name="userId">Steam client id</param>
    /// <returns></returns>
    private List<ProfileTableData>? ToPlayerTableData(List<GameProgress> gameProgress, ulong userId)
    {
        var list = gameProgress.Where(e => e.ClientId == userId).ToList();

        if (list.Count == 0)
            return null;

        List<ProfileTableData> playerTable = new List<ProfileTableData>();

        for (int i = 0; i < list.Count; i++)
        {
            playerTable.Add(new ProfileTableData(list[i].GameName, list[i].DataBegin, list[i].DataEnd));
        }
        
        return playerTable;
    }
    
    /// <summary>
    /// Need for clear all reference after closing user content. call from main class with dispose method
    /// </summary>
    public void UnloadTable()
    {
        if (GameProgresses != null)
        {
            GameProgresses.Clear();
            GameProgresses = null;
        }
        
        _database = null;
        _errorService = null;
    }
}

public class ProfileTableData
{
    public string GameName { get; set; }
    public string DataBegin { get; set; }
    public string DataEnd { get; set; }

    public ProfileTableData(string gameName, string dataBegin, string dataEnd)
    {
        GameName = gameName;
        DataBegin = dataBegin;
        DataEnd = dataEnd;
    }
}