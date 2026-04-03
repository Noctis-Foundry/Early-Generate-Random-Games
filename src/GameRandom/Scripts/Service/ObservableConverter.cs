using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;

namespace GameRandom.Scr.Service;

public class ObservableConverter : DependenceBase
{
    public ObservableCollection<TData> ToObservableCollection<TData>(IEnumerable<TData> enumerable)
    {
        return new ObservableCollection<TData>(enumerable);
    }
}