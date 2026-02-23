using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class CurrentGame : Window
{
    [Inject]
    private DatabaseService _databaseService;
    
    [Inject]
    private ErrorService _errorService;
    
    public CurrentGame()
    {
        InitializeComponent();
    }

    public void Open()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        Show();
    }

    public void CloseWindow()
    {
        // Очистка данных
        _databaseService = null;
        _errorService = null;
    }
    
    private void PopulateControls()
    {
        // Пример заполнения контролов по x:Name:
        // var myButton = this.FindControl<Button>("MyButton");
        // var myTextBlock = this.FindControl<TextBlock>("GameTitle");
        // myTextBlock.Text = "Название игры";
    }
}