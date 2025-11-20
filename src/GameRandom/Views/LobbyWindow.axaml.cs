using System;
using System.Linq;
using Avalonia.Controls;
using GameRandom.Scr.Events;
using GameRandom.Scr.DI;
using GameRandom.ViewModels;

namespace GameRandom.Views.LobbyModalWindow;

public partial class LobbyWindow : Window
{
    private const int MaxLenghtId = 18;
    [Inject] private EventBus? _eventBus;
    
    public LobbyWindow()
    {
        Console.WriteLine("Initialize Create Lobby");
        
        InitializeComponent();
        
        var viewModel = new CreateLobbyViewModel();
        DataContext = viewModel;
        
        Di.Container.RegisterSingleInstance(viewModel);
        Di.Container.ResolveFieldsFromClassInstance(this);
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