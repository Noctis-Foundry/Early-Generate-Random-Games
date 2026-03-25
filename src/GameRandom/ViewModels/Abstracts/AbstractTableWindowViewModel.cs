using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;

namespace GameRandom.ViewModels.AdminSystem;

public class AbstractTableWindowViewModel<TEntity> : ViewModelBase where TEntity : class
{
    [Inject] protected DatabaseService? _databaseService = null!;
    [Inject] protected ObservableConverter? _observableConverter = null!;
    [Inject] protected ErrorService? _errorService = null!;
    protected ObservableCollection<TEntity>? _tableData;

    public ObservableCollection<TEntity>? TableData
    {
        get => _tableData;
        set => SetProperty(ref _tableData, value);
    }

    private const int DatabaseTimeoutSec = 4;
    
    public AbstractTableWindowViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
    }

    public virtual async Task LoadData(Func<TEntity, bool>? predicate = null)
    {
        if (IsNotValidateInjectingData()) throw new NullReferenceException();

        List<TEntity>? tableList = new();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseTimeoutSec));
        
        tableList = predicate is null
            ? await _databaseService.GetTableListAsync<TEntity>(cts.Token)
            : await _databaseService.Where(predicate, cts.Token);

        if (tableList is null)
        {
            _errorService.ShowWindow(new ErrorStruct{ErrorMessage = $"Failed get table with type {typeof(TEntity)}", ErrorType = ErrorEnum.Error});
            return;
        }
            
        TableData = _observableConverter.ToObservableCollection(tableList);
        return;
    }

    protected bool IsNotValidateInjectingData()
    {
        return _databaseService is null && _observableConverter is null && _errorService is null;
    }

    public override void Dispose()
    {
        _databaseService = null;
        _observableConverter = null;
        _errorService = null;
        
        _tableData?.Clear();
        _tableData = null;
        
        TableData?.Clear();
        TableData = null;
        
        base.Dispose();
    }
}