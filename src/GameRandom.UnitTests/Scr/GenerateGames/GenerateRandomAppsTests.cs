using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GameRandom.CoreApp;
using GameRandom.Scripts.RollGameSystem.GenerateGames;
using Xunit;

namespace GameRandom.UnitTests.Scr.GenerateGames;

public class GenerateRandomAppsTests : IDisposable
{
    private readonly string _tempFilePath;

    public GenerateRandomAppsTests()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }

    private void CreateTempFile(object apps)
    {
        var json = JsonSerializer.Serialize(apps);
        File.WriteAllText(_tempFilePath, json);
    }

    [Fact]
    public void Constructor_WithValidPath_ShouldInitialize()
    {
        // Arrange
        var apps = new[]
        {
            new { AppId = 1, AppName = "Game 1", AppReleaseYear = 2020, AppGenres = new Dictionary<int, string> { { 1, "Action" } }, AppCategories = new Dictionary<int, string> { { 1, "Single-player" } } }
        };
        CreateTempFile(apps);

        // Act
        var genApps = new GenerateRandomApps(_tempFilePath);

        // Assert
        Assert.True(genApps.IsInitialized);
    }

    [Fact]
    public void Constructor_WithInvalidPath_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var invalidPath = "non_existent_file.json";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => new GenerateRandomApps(invalidPath));
    }

    [Fact]
    public void GetRandomGame_WithYear_ShouldReturnMatchingGame()
    {
        // Arrange
        var apps = new[]
        {
            new { AppId = 1, AppName = "Game 1", AppReleaseYear = 2020, AppGenres = new Dictionary<int, string> { { 1, "Action" } }, AppCategories = new Dictionary<int, string> { { 1, "Single-player" } } },
            new { AppId = 2, AppName = "Game 2", AppReleaseYear = 2021, AppGenres = new Dictionary<int, string> { { 2, "RPG" } }, AppCategories = new Dictionary<int, string> { { 2, "Multi-player" } } },
            new { AppId = 3, AppName = "Game 3", AppReleaseYear = 2020, AppGenres = new Dictionary<int, string> { { 3, "Strategy" } }, AppCategories = new Dictionary<int, string> { { 3, "Co-op" } } }
        };
        CreateTempFile(apps);
        var genApps = new GenerateRandomApps(_tempFilePath);

        // Act
        var result = genApps.GetRandomGame(2020);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2020, result.AppReleaseYear);
        Assert.True(result.AppId == 1 || result.AppId == 3);
    }

    [Fact]
    public void GetRandomGame_WithYear_NoMatch_ShouldReturnNull()
    {
        // Arrange
        var apps = new[]
        {
            new { AppId = 1, AppName = "Game 1", AppReleaseYear = 2020, AppGenres = new Dictionary<int, string> { { 1, "Action" } }, AppCategories = new Dictionary<int, string> { { 1, "Single-player" } } }
        };
        CreateTempFile(apps);
        var genApps = new GenerateRandomApps(_tempFilePath);

        // Act
        var result = genApps.GetRandomGame(2021);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetRandomGame_NoYear_ShouldReturnAnyGame()
    {
        // Arrange
        var apps = new[]
        {
            new { AppId = 1, AppName = "Game 1", AppReleaseYear = 2020, AppGenres = new Dictionary<int, string> { { 1, "Action" } }, AppCategories = new Dictionary<int, string> { { 1, "Single-player" } } },
            new { AppId = 2, AppName = "Game 2", AppReleaseYear = 2021, AppGenres = new Dictionary<int, string> { { 2, "RPG" } }, AppCategories = new Dictionary<int, string> { { 2, "Multi-player" } } }
        };
        CreateTempFile(apps);
        var genApps = new GenerateRandomApps(_tempFilePath);

        // Act
        var result = genApps.GetRandomGame();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.AppId, new[] { 1, 2 });
    }
}
