using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GameRandom.DataBaseContexts;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class AdminConfirmWindow : WindowAbstract
{
    public AdminConfirmWindow()
    {
        InitializeComponent();
        
        DataContext = new AdminConfirmViewModel();
    }

    public void LoadData(GameProgresses elementData)
    {
        if (DataContext is AdminConfirmViewModel vm)
            vm.UpdateElementData(elementData);
    }
}