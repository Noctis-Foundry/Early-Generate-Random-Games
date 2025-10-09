using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;

namespace GameRandom.Views.LobbyModalWindow;

public partial class LobbyWindow : Window
{
    private const int MaxLenghtId = 18;
    
    public LobbyWindow()
    {
        InitializeComponent();
    }

    private void OnLobbyIdChanging(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (textBox.Text == null)
                return;
            
            var filtered = new string(textBox.Text.Where(char.IsDigit).ToArray());
            
            if (filtered.Length > 18)
                filtered = filtered.Substring(0, 18);
            
            if (filtered != textBox.Text)
                textBox.Text = filtered;
        }
    }
}