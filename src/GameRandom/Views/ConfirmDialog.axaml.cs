using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GameRandom.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public async Task<bool> ShowConfirmDialog(string title, Window owner)
    {
        TextBlock.Text = title;
        return await ShowDialog<bool>(owner);
    }


    private void YesClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void NoClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}