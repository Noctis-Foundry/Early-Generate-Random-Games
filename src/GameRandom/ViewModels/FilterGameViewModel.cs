using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using GameRandom.Scr.Service;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel for managing game filtering options such as categories, genres, and years.
/// </summary>
public class FilterGameViewModel : ViewModelBase
{
    #region DataCollection

    /// <summary>
    /// Available game categories loaded from JSON.
    /// </summary>
    private ObservableCollection<string> _categories = new();
    public ObservableCollection<string> Categories => _categories;

    /// <summary>
    /// Available game genres loaded from JSON.
    /// </summary>
    private ObservableCollection<string> _genres = new();
    public ObservableCollection<string> Genres => _genres;
    
    /// <summary>
    /// Available release years for games.
    /// </summary>
    private List<int> _years = Enumerable.Range(2003, DateTime.Now.Year - 2003 + 1).ToList();
    public List<int> Years => _years;

    #endregion
    
    #region SelectedCollection

    private List<string> _selectedCategories = new();
    private List<string> _selectedGenres = new();
    private List<int> _selectedYears = new();

    /// <summary>
    /// List of categories selected by the user.
    /// </summary>
    public List<string> SelectedCategories
    {
        get => _selectedCategories;
        set
        {
            _selectedCategories = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// List of genres selected by the user.
    /// </summary>
    public List<string> SelectedGenres
    {
        get => _selectedGenres;
        set
        {
            _selectedGenres = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// List of release years selected by the user.
    /// </summary>
    public List<int> SelectedYears
    {
        get => _selectedYears;
        set
        {
            _selectedYears = value;
            OnPropertyChanged();
        }
    }

    #endregion
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterGameViewModel"/> class and loads data from JSON.
    /// </summary>
    public FilterGameViewModel()
    {
        LoadDataFromJson();
    }

    /// <summary>
    /// Loads categories and genres from local JSON asset files.
    /// </summary>
    private void LoadDataFromJson()
    {
        try
        {
            string categoriesPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Jsons", "categories.json");
            string genresPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Jsons", "genres.json");

            if (File.Exists(categoriesPath))
            {
                var categoriesList = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(categoriesPath));
                if (categoriesList != null)
                {
                    _categories = new ObservableCollection<string>(categoriesList);
                }
            }

            if (File.Exists(genresPath))
            {
                var genresList = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(genresPath));
                if (genresList != null)
                {
                    _genres = new ObservableCollection<string>(genresList);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error loading filter data: {ex.Message}");
            throw;
        }
    }
    

    /// <summary>
    /// Gets the current filter selection as a <see cref="FilteredData"/> object.
    /// </summary>
    /// <returns>An instance of <see cref="FilteredData"/> containing selected filters.</returns>
    public FilteredData GetFilters()
    {
        return new FilteredData(SelectedCategories, SelectedGenres, SelectedYears);
    }
    
    /// <summary>
    /// Clears selected filter collections and releases resources.
    /// </summary>
    public override void Dispose()
    {
        _selectedCategories.Clear();
        _selectedGenres.Clear();
        _selectedYears.Clear();
        
        SelectedCategories.Clear();
        SelectedGenres.Clear();
        SelectedYears.Clear();
        
        _categories.Clear();
        _genres.Clear();
        _years.Clear();
        
        Years.Clear();
        Genres.Clear();
        Categories.Clear();
        
        
        base.Dispose();
    }
}

/// <summary>
/// Data structure representing the selected game filter criteria.
/// </summary>
/// <param name="categories">Selected categories.</param>
/// <param name="genres">Selected genres.</param>
/// <param name="years">Selected years.</param>
public class FilteredData (List<string> categories, List<string> genres, List<int> years)
{
    /// <summary>
    /// Gets or sets the list of selected categories.
    /// </summary>
    public List<string> Categories { get; set; } = categories;

    /// <summary>
    /// Gets or sets the list of selected genres.
    /// </summary>
    public List<string> Genres { get; set; } = genres;

    /// <summary>
    /// Gets or sets the list of selected release years.
    /// </summary>
    public List<int> Years { get; set; } = years;
}
