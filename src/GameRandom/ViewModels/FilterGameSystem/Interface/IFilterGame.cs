namespace GameRandom.ViewModels.FilterGameSystem.Interface;

public interface IFilterGame
{
    public FilterLoadData LoadDataFromJson();
    public void Dispose();
}
