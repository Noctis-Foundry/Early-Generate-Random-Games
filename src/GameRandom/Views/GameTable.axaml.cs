using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GameRandom.Views;

public partial class GameTable : UserControl
{
    private Action<string> _onShowContent;
    public ObservableCollection<GameRecord> Games { get; set; }

    public GameTable()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
        Games = new ObservableCollection<GameRecord>();
        GamesItemsControl.ItemsSource = Games;
        
        LoadSampleData(); 
    }

    public void AddListener(Action<string> onChangeContent) => _onShowContent = onChangeContent;

    private void Close(object? sender, RoutedEventArgs e)
    {
        _onShowContent?.Invoke("Main");
    }

    private void CreateLobby_Click(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Create Lobby clicked!");
        
        var newGame = new GameRecord 
        { 
            Player = "New Player", 
            Game = "New Game", 
            Status = "Waiting", 
            StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
            EndDate = "-"
        };
        Games.Add(newGame);
    }

    private void LoadSampleData()
    {
        // Тестовые данные для демонстрации
        var sampleGames = new List<GameRecord>
        {
            new GameRecord { Player = "Player1", Game = "Chess", Status = "Active", 
                           StartDate = "2024-01-15", EndDate = "2024-01-15" },
            new GameRecord { Player = "Player2", Game = "Checkers", Status = "Finished", 
                           StartDate = "2024-01-14", EndDate = "2024-01-14" },
            new GameRecord { Player = "Player3", Game = "Backgammon", Status = "Waiting", 
                           StartDate = "2024-01-16", EndDate = "-" },
            new GameRecord { Player = "Player4", Game = "Chess", Status = "Active", 
                           StartDate = "2024-01-15", EndDate = "-" }
        };

        foreach (var game in sampleGames)
        {
            Games.Add(game);
        }
    }
}

// Модель данных для записей в таблице
public class GameRecord
{
    public string Player { get; set; } = string.Empty;
    public string Game { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
}