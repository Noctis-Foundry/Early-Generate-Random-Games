using System;
using GameRandom.Src;
using GameRandom.DependenceInjectSystem;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using GameRandom.CoreApp;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.Scr.Service;
using GameRandom.Scripts.RollGameSystem.GenerateGames;
using GameRandom.Src.RollGameSystem;
using GameRandom.Src.RollGameSystem.GenerateStrategy;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel for the logic of rolling (generating) random games.
/// </summary>
public sealed class RollGameViewModel : ViewModelBase
{
    [Inject] private SteamService _steamService = null!;
    [Inject] private ISteamWebService _steamWebService = null!;

    /// <summary>
    /// Interface for generating random applications.
    /// </summary>
    private IGenApp _generateRandomApps;

    private SemaphoreSlim _userLibSemaphore = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Maximum number of iterations to find suitable games.
    /// </summary>
    private const int IterationLimit = 50;

    /// <summary>
    /// Current number of iterations.
    /// </summary>
    private int _iterationCount;

    private List<int> LibraryIds = new();

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

    private bool _isUserLib = false;

    public bool IsUserLib
    {
        get => _isUserLib;
        set => SetProperty(ref _isUserLib, value);
    }

    #endregion

    /// <summary>
    /// Constructor. Initializes the random application generator if not in design mode.
    /// </summary>
    public RollGameViewModel()
    {
        if (Design.IsDesignMode) return;

        _generateRandomApps = new GenerateRandomApps();

        InitializeDiContainer();
    }

    protected override void InitializeDiContainer()
    {
        base.InitializeDiContainer();

        if (_steamService is null)
            throw new NullReferenceException();

        if (_steamWebService is null)
            throw new NullReferenceException(nameof(_steamWebService));
    }

    /// <summary>
    /// Generates the specified number of games considering filters.
    /// </summary>
    /// <param name="countGames">Number of games to retrieve.</param>
    /// <param name="filteredGamesData">Filtering data.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    public async Task GenerateGames(int countGames, FilterOutputData? filteredGamesData,
        CancellationToken cancellationToken = default)
    {
        await _generateRandomApps.StartGenerateApp();

        if (!_generateRandomApps.ListIsLoad())
            throw new NullReferenceException("list with games is null");

        ClearItems();

        try
        {
            for (int i = 0; i < IterationLimit && _appInfo.Count < countGames; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await GenerateAppInfo(filteredGamesData);

                if (result.code == GenerationStatusCode.Exit)
                    break;

                if (result.appInfo is not null)
                    _appInfo.Add(result.appInfo);
            }
        }
        catch (Exception e)
        {
            Logger.Error("Failed to generate games: " + e.Message);
        }
        finally
        {
            _generateRandomApps.EndGeneration();
        }
    }

    /// <summary>
    /// Generates information for a single random game and loads its image.
    /// </summary>
    /// <param name="filteredGamesData">Filtering data.</param>
    /// <returns>AppInfo object or null if the game failed filtering or an error occurred.</returns>
    private async Task<(GenerationStatusCode code, AppInfo? appInfo)> GenerateAppInfo(
        FilterOutputData? filteredGamesData)
    {
        GenerationTypes types = GetGenerationType();
        object? inputData = null;

        if (types == GenerationTypes.RandomFromLibrary)
            inputData = await IsLibrary();

        var result = await _generateRandomApps.GetRandomGame(types, inputData);

        if (result.StatusCode == GenerationStatusCode.Exit)
            return (result.StatusCode, null);

        var gameInfo = result.AppSavedContext;

        if (gameInfo is null || _appInfo.Any(e => e.AppData.AppId == gameInfo.AppId))
            return (result.StatusCode, null);

        if (IsFilter && filteredGamesData is not null)
            if (!FilterGame(gameInfo, filteredGamesData))
                return (result.StatusCode, null);

        var imageBytes = await _steamService.GetImageBytes(gameInfo.HeaderImage);

        if (imageBytes == null)
            return (result.StatusCode, null);

        return (result.StatusCode, new AppInfo(gameInfo, imageBytes));
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

        if (filter.Years.Count > 0 && filter.Years.All(y => y != savedGame.AppReleaseYear))
            return false;

        return true;
    }

    private async Task<List<int>?> IsLibrary()
    {
        if (!await _userLibSemaphore.WaitAsync(0))
        {
            Logger.Debug("Failed to choose user type");
            return null;
        }

        _isUserLib = true;

        StartTaskWaiter();

        try
        {
            var filename = OperatingSystem.IsWindows() ? "SteamVDFParser.exe" : "SteamVDFParser";

            var procInfo = new ProcessStartInfo
            {
                FileName = filename,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var process = new Process { StartInfo = procInfo };

            process.Start();

            Task<string> output = process.StandardOutput.ReadToEndAsync();
            Task<string> error = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(output, error, process.WaitForExitAsync());

            var json = JsonDocument.Parse(output.Result);

            var list = json.Deserialize<List<int>>();

            Logger.Info($"Steam info list count = {list.Count}");

            return list;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
        finally
        {
            _userLibSemaphore.Release();
        }
    }

    private GenerationTypes GetGenerationType()
    {
        return _isUserLib ? GenerationTypes.RandomFromLibrary : GenerationTypes.RandomIndex;
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

        _generateRandomApps?.Dispose();
        _generateRandomApps = null!;
    }
}