using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.DataBaseContexts;

namespace GameRandom.ViewModels;

public class GameTableViewModel : ViewModelBase
{
    private ObservableCollection<GameProgress> _gameProgress;
    
    public ObservableCollection<GameProgress> GameProgress
    {
        get => _gameProgress;
        set => SetProperty(ref _gameProgress, value);
    }
}