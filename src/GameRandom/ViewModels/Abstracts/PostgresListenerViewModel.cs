using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GameRandom.Scr.Service;

namespace GameRandom.ViewModels;

public class PostgresListenerViewModel<TCollectionType> : ViewModelBase, IPostgresListener
{
    private ObservableCollection<TCollectionType> _collection;
    public ObservableCollection<TCollectionType> Collection
    {
        get => _collection;
        set => SetProperty(ref _collection, value);
    }
    
    public async Task UpdateAsync(PayloadStructure payloadStructure)
    {
        switch ((OperationsEnum)payloadStructure.OpCode)
        {
            case OperationsEnum.Add:
                await AddOpCode((TableEnum)payloadStructure.TableCode, payloadStructure.RowId, Collection);
                break;
        }
    }

    public Task UpdateOpCode(TableEnum tableCode, int rowId, object? targetCollection)
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteOpCode(TableEnum tableCode, int rowId, object? targetCollection)
    {
        throw new System.NotImplementedException();
    }

    public Task AddOpCode(TableEnum tableCode, int rowId, object? targetCollection)
    {
        throw new System.NotImplementedException();
    }

    public Task UpdateOpCode(TableEnum tableCode, int rowId = 0)
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteOpCode(TableEnum tableCode, int rowId = 0)
    {
        throw new System.NotImplementedException();
    }

    public Task AddOpCode(TableEnum tableCode, int rowId = 0)
    {
        throw new System.NotImplementedException();
    }
}