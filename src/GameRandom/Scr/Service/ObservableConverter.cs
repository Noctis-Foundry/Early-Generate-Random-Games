using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GameRandom.Scr.Service;

public class ObservableConverter
{
    public ObservableCollection<TData> ToObservableCollection<TData>(IEnumerable<TData> enumerable)
    {
        return new ObservableCollection<TData>(enumerable);
    }
}