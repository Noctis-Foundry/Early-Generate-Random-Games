using System.Collections.ObjectModel;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using System.Collections.Generic;
using Avalonia.Threading;
using GameRandom.SteamSDK.Enums;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GameRandom.ViewModels;

public class StatisticGameTableViewModel : AbstractTableWindowViewModel<GameProgresses>
{
    /// <summary>
    /// Need for clear all reference after closing user content. call from main class with dispose method
    /// </summary>
    public void UnloadTable()
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        
        TableData?.Clear();
        _tableData?.Clear();
        
        _databaseService = null;
        _errorService = null;
        _observableConverter = null;
    }
}