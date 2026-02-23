using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;

namespace GameRandom.ViewModels;

public class AbstractTableWindowViewModel<TEntity> : ViewModelBase where TEntity : class
{
    [Inject] protected DatabaseService? _databaseService = null!;
    [Inject] protected ObservableConverter? _observableConverter = null!;
    [Inject] protected ErrorService? _errorService = null!;
    protected ObservableCollection<TEntity> _tableData;

    public ObservableCollection<TEntity> TableData
    {
        get => _tableData;
        set => SetProperty(ref _tableData, value);
    }

    public AbstractTableWindowViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
    }

    public virtual async Task LoadData(Func<TEntity, bool>? predicate = null)
    {
        if (IsNotValidateInjectingData()) throw new NullReferenceException();

        List<TEntity>? tableList = new();

        tableList = predicate is null
            ? await _databaseService.GetTableListAsync<TEntity>()
            : await _databaseService.Where(predicate);

        if (tableList is null)
        {
            _errorService.ShowErrorWindow($"Failed get table with type {typeof(TEntity)}", ErrorEnum.Error);
            return;
        }
            
        TableData = _observableConverter.ToObservableCollection(tableList);
        return;
    }

    protected bool IsNotValidateInjectingData()
    {
        return _databaseService is null && _observableConverter is null && _errorService is null;
    }
}