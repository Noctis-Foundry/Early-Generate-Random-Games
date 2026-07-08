using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GameRandom.Scripts.RollGameSystem.GenerateGames;
using GameRandom.Scripts.RollGameSystem.GenerateStrategy;
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
    public async Task Constructor_WithValidPath_ShouldInitialize()
    {
        // Arrange
        var apps = new[]
        {
            new { AppId = 1, AppName = "Game 1", AppReleaseYear = 2020, AppGenres = new Dictionary<int, string> { { 1, "Action" } }, AppCategories = new Dictionary<int, string> { { 1, "Single-player" } } }
        };
        CreateTempFile(apps);

        // Act
        var genApps = new GenerateRandomApps(_tempFilePath);
        await genApps.StartGenerateApp();

        // Assert
        Assert.True(genApps.ListIsLoad());
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
    public async Task GetRandomGame_ShouldReturnAnyGame()
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
        var result = await genApps.GetRandomGame(GenerationTypes.RandomIndex);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.AppSavedContext.AppId, new[] { 1, 2 });
    }
}
