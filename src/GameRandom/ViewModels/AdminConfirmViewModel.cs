using GameRandom.DataBaseContexts;

namespace GameRandom.ViewModels;

public class AdminConfirmViewModel : ViewModelBase
{
    private GameProgresses _elementData;

    public  GameProgresses ElementData
    {
        get => _elementData;
        set => SetProperty(ref _elementData, value);
    }
    
    public void UpdateElementData(GameProgresses elementData) => ElementData = elementData;
}