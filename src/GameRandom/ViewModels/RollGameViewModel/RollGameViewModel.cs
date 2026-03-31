using System;
using GameRandom.Src;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameRandom.CoreApp;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel for the logic of rolling (generating) random games.
/// </summary>
public sealed class RollGameViewModel : ViewModelBase
{
    [Inject] private SteamService _steamService = null!;
    
    /// <summary>
    /// Interface for generating random applications.
    /// </summary>
    private IGenApp? _generateRandomApps;
    
    /// <summary>
    /// Maximum number of iterations to find suitable games.
    /// </summary>
    private const int IterationLimit = 150;
    
    /// <summary>
    /// Current number of iterations.
    /// </summary>
    private int _iterationCount;

    #region BindingProperty

    /// <summary>
    /// List of generated games with their information and images.
    /// </summary>
    private List<AppInfo> _appInfo = new();
    /// <summary>
    /// List of generated games for UI display.
    /// </summary>
    public List<AppInfo> AppInfo => _appInfo;
    
    /// <summary>
    /// Flag indicating whether filtering is used during generation.
    /// </summary>
    private bool _isFilter = false;

    /// <summary>
    /// Property for enabling/disabling filtering. Updates UI via SetProperty.
    /// </summary>
    public bool IsFilter
    {
        get => _isFilter;
        set => SetProperty(ref _isFilter, value);
    }

    #endregion
    
    
    /// <summary>
    /// Constructor. Initializes the random application generator if not in design mode.
    /// </summary>
    public RollGameViewModel(IGenApp generateRandomApps)
    {
        if (Design.IsDesignMode) return;

        _generateRandomApps = generateRandomApps;

        InitializeDiContainer();
        
    }

    protected override void InitializeDiContainer()
    {
        base.InitializeDiContainer();

        if (_steamService is null)
            throw new NullReferenceException();
    }

    /// <summary>
    /// Generates the specified number of games considering filters.
    /// </summary>
    /// <param name="countGames">Number of games to retrieve.</param>
    /// <param name="filteredGamesData">Filtering data.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    public async Task GenerateGames(int countGames, FilterOutputData? filteredGamesData, CancellationToken cancellationToken = default)
    {
        if (_generateRandomApps is null || !_generateRandomApps.IsInitialized)
            return;

        ClearItems();

        try
        {
            for (int i = 0; i < IterationLimit && _appInfo.Count < countGames; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var appInfo = await GenerateAppInfo(filteredGamesData);
                
                if (appInfo is not null)
                    _appInfo.Add(appInfo);
            }
        }
        catch (Exception e)
        {
            Logger.Error("Failed to generate games: " + e.Message);
        }
    }

    /// <summary>
    /// Generates information for a single random game and loads its image.
    /// </summary>
    /// <param name="filteredGamesData">Filtering data.</param>
    /// <returns>AppInfo object or null if the game failed filtering or an error occurred.</returns>
    private async Task<AppInfo?> GenerateAppInfo(FilterOutputData? filteredGamesData)
    {
        var gameInfo = _generateRandomApps?.GetRandomGame();

        if (gameInfo is null || _appInfo.Any(e => e.AppData.AppId == gameInfo.AppId))
            return null;

        if (IsFilter && filteredGamesData is not null)
            if (!FilterGame(gameInfo, filteredGamesData))
                return null;

        var imageBytes = await _steamService.GetImageBytes(gameInfo.HeaderImage);

        if (imageBytes == null)
            return null;
        
        return new AppInfo(gameInfo, imageBytes);
    }
    
    /// <summary>
    /// Checks if the game matches the specified filters (categories, genres, years).
    /// </summary>
    /// <param name="savedGame">Game data.</param>
    /// <param name="filter">Active filters.</param>
    /// <returns>True if the game matches the filters, otherwise False.</returns>
    private bool FilterGame(AppSavedContext savedGame, FilterOutputData filter)
    {
        if (filter.Categories.Count > 0 && !filter.Categories.Any(c => savedGame.AppCategories.Contains(c)))
            return false;

        if (filter.Genres.Count > 0 && !filter.Genres.Any(g => savedGame.AppGenres.Contains(g)))
            return false;

        if (filter.Years.Count > 0 && !filter.Years.Any(y => y == savedGame.AppReleaseYear))
            return false;

        return true;
    }

    /// <summary>
    /// Clears the list of games and resets the iteration counter.
    /// </summary>
    private void ClearItems()
    {
        _appInfo.Clear();
        _iterationCount = 0;
    }

    /// <summary>
    /// Disposes of ViewModel resources.
    /// </summary>
    public override void Dispose()
    {
        ClearItems();
        
        //TODO _generateRandomApps.Dispose()
        _generateRandomApps = null;
    }
}