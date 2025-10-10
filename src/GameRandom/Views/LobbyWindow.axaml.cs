using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GameRandom.SteamSDK;

namespace GameRandom.Views.LobbyModalWindow;

public partial class LobbyWindow : Window
{
    private const int MaxLenghtId = 18;
    private readonly LobbySystem _lobbySystem;
    
    public LobbyWindow()
    {
        InitializeComponent();
        _lobbySystem = new LobbySystem();
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

    private void ConnectToLobby(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(IdBox.Text))
            return; //Добавить блок для отображения ошибок

        if (uint.TryParse(IdBox.Text, out var id))
        {
            _lobbySystem.ConnectToLobby(id);
        }
    }
}