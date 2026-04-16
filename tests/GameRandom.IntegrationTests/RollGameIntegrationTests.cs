using System;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scripts.RollGameSystem.GenerateGames;
using GameRandom.Src;
using GameRandom.ViewModels.AdminConfirmSystem;
using Xunit;

namespace GameRandom.IntegrationTests;

public class RollGameIntegrationTests : IDisposable
{
    private readonly RollGameViewModel _viewModel;
    private readonly IGenApp _genApp;

    public RollGameIntegrationTests()
    {
        // Real app generator loading JSON
        _genApp = new GenerateRandomApps();
        
        Di.BindingInstance.BindSingleton(typeof(SteamService), new SteamService());
        
        _viewModel = new RollGameViewModel();
    }

    [Fact]
    public async Task GenerateGames_Should_LoadGames_From_Json_File()
    {
        // Act
        // Generate 5 games without filters
        await _viewModel.GenerateGames(5, null);

        // Assert
        Assert.NotEmpty(_viewModel.AppInfo);
        // Sometimes it might find fewer if iterations limit reached or network failed,
        // but it should find at least something from temp_apps.json
        Assert.True(_viewModel.AppInfo.Count > 0);
        
        var firstGame = _viewModel.AppInfo.First();
        Assert.NotNull(firstGame.AppData.AppName);
        Assert.NotNull(firstGame.ImageBytes);
        Assert.True(firstGame.ImageBytes.Length > 0);
    }

    [Fact]
    public async Task GenerateGames_With_Filter_Should_Only_Return_Matching_Games()
    {
        // Arrange
        _viewModel.IsFilter = true;
        // Let's assume we have at least one game from 2020 or later in temp_apps.json
        // Or we can pick a year that we know exists in the file.
        var filter = new FilterOutputData(new(), new(), new() { 2020, 2021, 2022, 2023, 2024 });

        // Act
        await _viewModel.GenerateGames(3, filter);

        // Assert
        foreach (var app in _viewModel.AppInfo)
        {
            Assert.Contains(app.AppData.AppReleaseYear, filter.Years);
        }
    }

    [Fact]
    public async Task GenerateGames_With_Genres_Filter_Should_Onlyy_Return_Matching_Games()
    {
        _viewModel.IsFilter = true;
        var filter = new FilterOutputData(new(), new() {"Action", "RPG"}, new());

        await _viewModel.GenerateGames(3, filter);

        foreach (var app in _viewModel.AppInfo)
        {
            Assert.Contains(app.AppData.AppGenres, g => filter.Genres.Contains(g));
        }
    }
    
    [Fact]
    public async Task GenerateGames_With_Categories_Filter_Should_Onlyy_Return_Matching_Games()
    {
        _viewModel.IsFilter = true;
        var filter = new FilterOutputData(new() { "Single-player" }, new(), new());

        await _viewModel.GenerateGames(3, filter);

        foreach (var app in _viewModel.AppInfo)
        {
            Assert.Contains(app.AppData.AppCategories, c => filter.Categories.Contains(c));
        }
    }
    
    [Fact]
    public async Task GenerateGames_With_All_Filter_Should_Onlyy_Return_Matching_Games()
    {
        _viewModel.IsFilter = true;
        var filter = new FilterOutputData(new() { "Single-player" }, new() {"Action", "RPG"}, new() { 2020, 2021, 2022, 2023, 2024 });

        await _viewModel.GenerateGames(3, filter);

        foreach (var app in _viewModel.AppInfo)
        {
            Assert.Contains(app.AppData.AppGenres, g => filter.Genres.Contains(g));
            Assert.Contains(app.AppData.AppCategories, c => filter.Categories.Contains(c));
            
            Assert.Contains(app.AppData.AppReleaseYear, filter.Years);
        }
    }

    public void Dispose()
    {
        Di.DiClearing.UnsubscribeInstance(typeof(SteamService));
        _viewModel.Dispose();
    }
}
