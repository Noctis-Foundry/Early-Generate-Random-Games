using System;
using GameRandom.DependenceInjectSystem;
using System.Collections.Generic;
using GameRandom.DependenceInjectSystem;
using System.Collections.ObjectModel;
using GameRandom.DependenceInjectSystem;
using System.IO;
using GameRandom.DependenceInjectSystem;
using System.Linq;
using GameRandom.DependenceInjectSystem;
using System.Text.Json;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scr.Service;
using GameRandom.DependenceInjectSystem;
using GameRandom.ViewModels.FilterGameSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.ViewModels.FilterGameSystem.Interface;
using GameRandom.DependenceInjectSystem;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel for managing game filtering options such as categories, genres, and years.
/// </summary>
public class FilterGameViewModel : ViewModelBase
{
    [Inject] private ObservableConverter _observableConverter = null!;
    
    #region DataCollection

    /// <summary>
    /// Available game categories loaded from JSON.
    /// </summary>
    private ObservableCollection<string> _categories = new();
    public ObservableCollection<string> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    /// <summary>
    /// Available game genres loaded from JSON.
    /// </summary>
    private ObservableCollection<string> _genres = new();
    public ObservableCollection<string> Genres
    {
        get => _genres;
        set => SetProperty(ref _genres, value);
    }

    /// <summary>
    /// Available release years for games.
    /// </summary>
    private List<int> _years = Enumerable.Range(2003, DateTime.Now.Year - 2003 + 1).ToList();
    public List<int> Years
    {
        get => _years;
        set => SetProperty(ref _years, value);
    }

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
        set => SetProperty(ref _selectedCategories, value);
    }

    /// <summary>
    /// List of genres selected by the user.
    /// </summary>
    public List<string> SelectedGenres
    {
        get => _selectedGenres;
        set => SetProperty(ref _selectedGenres, value);
    }

    /// <summary>
    /// List of release years selected by the user.
    /// </summary>
    public List<int> SelectedYears
    {
        get => _selectedYears;
        set => SetProperty(ref _selectedYears, value);
    }

    #endregion

    private IFilterGame _filterGame = new LoadFilterData();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterGameViewModel"/> class and loads data from JSON.
    /// </summary>
    public FilterGameViewModel()
    {
        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (_observableConverter == null)
            throw new NullReferenceException(nameof(_observableConverter));
        
        LoadDataFromJson();
    }

    private void LoadDataFromJson()
    {
        var filterData = _filterGame.LoadDataFromJson();
        
        Genres = _observableConverter.ToObservableCollection(filterData.GenresList);
        Categories = _observableConverter.ToObservableCollection(filterData.CategoriesList);
    }

    /// <summary>
    /// Gets the current filter selection as a <see cref="FilterOutputData"/> object.
    /// </summary>
    /// <returns>An instance of <see cref="FilterOutputData"/> containing selected filters.</returns>
    public FilterOutputData GetFilters()
    {
        Logger.Debug($"Selected categories count {SelectedCategories.Count}");
        Logger.Debug($"Selected genres count {SelectedGenres.Count}");
        Logger.Debug($"Selected years count {SelectedYears.Count}");
        
        return new FilterOutputData(SelectedCategories, SelectedGenres, SelectedYears);
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

        _observableConverter = null!;
        
        base.Dispose();
    }
}

/// <summary>
/// Data structure representing the selected game filter criteria.
/// </summary>
/// <param name="categories">Selected categories.</param>
/// <param name="genres">Selected genres.</param>
/// <param name="years">Selected years.</param>
public class FilterOutputData (List<string> categories, List<string> genres, List<int> years)
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
