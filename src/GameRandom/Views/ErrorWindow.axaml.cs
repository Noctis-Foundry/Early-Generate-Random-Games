using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GameRandom.Views;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();
    }

    public void ChangeTextOnModal(string text)
    {
        ErrorLabel.Content = text;
    }
}