using System.Xml.Schema;

namespace MessageBox.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string _windowTitle = "Message box";
    private string _windowMessage = "Exception error: not load steam. Please restart your steam or on him on your PC";
    
    public string WindowTitle
    {
        get => _windowTitle;
        set => SetProperty(ref _windowTitle, value);
    }

    public string WindowContent
    {
        get => _windowMessage;
        set => SetProperty(ref _windowMessage, value);
    }

    public void SetWindowData(string title, string message)
    {
        WindowTitle = title;
        WindowContent = message;
    }
}

