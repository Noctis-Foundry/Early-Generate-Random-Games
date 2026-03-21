using System;
using GameRandom.SteamSDK;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameRandom.CoreApp;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace GameRandom.ViewModels.AdminSystem;

public class RollGameViewModel : ViewModelBase, IDisposable
{
    private List<AppInfo> _appInfo = new();
    public List<AppInfo> AppInfo => _appInfo;
    
    private IGenApp? _generateRandomApps;
    
    private const int IterationLimit = 500;
    
    private int _iterationCount;

    private bool _isFilter = false;

    public bool IsFilter
    {
        get => _isFilter;
        set => SetProperty(ref _isFilter, value);
    }

    public RollGameViewModel()
    {
        if (Design.IsDesignMode) return;
        
        _generateRandomApps = new GenerateRandomApps();
    }

    public async Task GenerateGames(int countGames, FilteredData? filteredGamesData, CancellationToken cancellationToken = default)
    {
        if (!IsValidationGenerateData())
            return;

        ClearItems();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            while (_iterationCount < IterationLimit && _appInfo.Count < countGames)
            {
                _iterationCount++;

                var gameInfo = _generateRandomApps?.GetRandomGame();

                if (gameInfo is null || _appInfo.Any(e => e.AppData.AppId == gameInfo.AppId))
                    continue;

                if (IsFilter && filteredGamesData is not null)
                    if (!FilterGame(gameInfo, filteredGamesData))
                        continue;

                var imageBytes = await SteamService.Instance.GetImageBytes(gameInfo.HeaderImage);

                if (imageBytes == null)
                    continue;

                _appInfo.Add(new AppInfo(gameInfo, imageBytes));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    /// <summary>
    /// Confirms with user before stopping ongoing game roll operation.
    /// </summary>
    /// <returns>True if operation can proceed, false if canceled.</returns>

    private bool FilterGame(AppSavedContext savedGame, FilteredData filter)
    {
        if (filter.Categories.Count > 0 && !filter.Categories.Any(c => savedGame.AppCategories.Contains(c)))
            return false;

        if (filter.Genres.Count > 0 && !filter.Genres.Any(g => savedGame.AppGenres.Contains(g)))
            return false;

        if (filter.Years.Count > 0 && !filter.Years.Any(y => y == savedGame.AppReleaseYear))
            return false;

        return true;
    }

    private bool IsValidationGenerateData()
    {
        return _generateRandomApps.IsInitialized;
    }

    private void ClearItems()
    {
        _appInfo.Clear();
        _iterationCount = 0;
    }

    public void Dispose()
    {
        _generateRandomApps = null;
        ClearItems();
    }
}