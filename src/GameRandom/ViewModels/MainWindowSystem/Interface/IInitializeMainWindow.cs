using System.Threading.Tasks;

namespace GameRandom.Views.MainWindowSystem;

public interface IInitializeMainWindow
{
    public void InitializeUi();
    public void SetLoadControl();
    public void EndLoadingData();
}