using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GameRandom.ViewModels;

public class FilterGameViewModel : ViewModelBase, IDisposable
{
    private ObservableCollection<string> categories = new()
    {
        // Режимы игры
        "Single-player",
        "Multi-player",
        "Co-op",
        "Online Co-op",
        "Local Co-op",
        "Local Multi-Player",
        "MMO",
        "PvP",
        "Online PvP",
        "Shared/Split Screen PvP",
        "Shared/Split Screen Co-op",
        "Shared/Split Screen",
        "Cross-Platform Multiplayer",

        // Steam фичи
        "Steam Achievements",
        "Steam Cloud",
        "Steam Trading Cards",
        "Steam Workshop",
        "Steam Leaderboards",
        "In-App Purchases",
        "Partial Controller Support",
        "Full Controller Support",
        "Remote Play on Phone",
        "Remote Play on Tablet",
        "Remote Play on TV",
        "Remote Play Together",
        "Steam Turn Notifications",
        "HDR available",
        "VR Supported",
        "VR Only",

        // Дополнительно
        "Captions available",
        "Commentary available",
        "Stats",
        "Includes level editor",
        "Includes Source SDK",
        "Family Sharing",
    };

    public ObservableCollection<string> Categories => categories;

    private ObservableCollection<string> genres = new()
    {
        "Action",
        "Adventure",
        "Casual",
        "Free to Play",
        "Indie",
        "Massively Multiplayer",
        "Racing",
        "RPG",
        "Simulation",
        "Sports",
        "Strategy",
        "Early Access"
    };

    public ObservableCollection<string> Genres => genres;
    
    private List<int> years = Enumerable.Range(2003, DateTime.Now.Year - 2003 + 1).ToList();

    public List<int> Years => years;
    
    private List<string> _selectedCategories = new();
    private List<string> _selectedGenres = new();
    private List<int> _selectedYears = new();

    public List<string> SelectedCategories
    {
        get => _selectedCategories;
        set
        {
            _selectedCategories = value;
            OnPropertyChanged();
        }
    }
    public List<string> SelectedGenres
    {
        get => _selectedGenres;
        set
        {
            _selectedGenres = value;
            OnPropertyChanged();
        }
    }
    public List<int> SelectedYears
    {
        get => _selectedYears;
        set
        {
            _selectedYears = value;
            OnPropertyChanged();
        }
    }

    public FilteredData GetCategory()
    {
        return new FilteredData(SelectedCategories, SelectedGenres, SelectedYears);
    }
    
    public void Dispose()
    {
        SelectedCategories.Clear();
        SelectedGenres.Clear();
        SelectedYears.Clear();
    }
}

public class FilteredData (List<string> categories, List<string> genres, List<int> years)
{
    public List<string> Categories { get; set; } = categories;
    public List<string> Genres { get; set; } = genres;
    public List<int> Years { get; set; } = years;
}
