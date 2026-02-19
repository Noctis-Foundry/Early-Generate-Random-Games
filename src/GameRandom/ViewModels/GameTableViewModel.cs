using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.DataBaseContexts;

namespace GameRandom.ViewModels;

public class GameTableViewModel : ViewModelBase
{
    private ObservableCollection<GameProgresses> _gameProgress;
    
    public ObservableCollection<GameProgresses> GameProgress
    {
        get => _gameProgress;
        set => SetProperty(ref _gameProgress, value);
    }
}