using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.Scr.DI;
using GameRandom.Src;
using GameRandom.ViewModels.AdminConfirmSystem;
using Xunit;

namespace GameRandom.UnitTests.ViewModels;

public class RollGameViewModelTests : IDisposable
{
    private class MockGenApp : IGenApp
    {
        public bool IsInitialized { get; set; } = true;
        public List<AppSavedContext> Games { get; set; } = new();

        public AppSavedContext? GetRandomGame(int year) => Games.FirstOrDefault(g => g.AppReleaseYear == year);

        public AppSavedContext? GetRandomGame()
        {
            if (Games.Count == 0) return null;
            return Games[0];
        }
    }

    public RollGameViewModelTests()
    {
        // Mock SteamService for VM constructor
        Di.Container.RegisterSingleInstance<SteamService>(new SteamService());
    }

    public void Dispose()
    {
        Di.Container.Unregister<SteamService>();
    }

    [Fact]
    public void Constructor_Should_Initialize()
    {
        // Act
        var vm = new RollGameViewModel(new MockGenApp());

        // Assert
        Assert.NotNull(vm.AppInfo);
        Assert.Empty(vm.AppInfo);
        Assert.False(vm.IsFilter);
    }

    [Fact]
    public void IsFilter_PropertyChange_Should_Notify()
    {
        // Arrange
        var vm = new RollGameViewModel(new MockGenApp());
        bool notified = false;
        vm.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(vm.IsFilter)) notified = true; };

        // Act
        vm.IsFilter = true;

        // Assert
        Assert.True(vm.IsFilter);
        Assert.True(notified);
    }

    [Fact]
    public async Task GenerateGames_Should_AddGames_When_NoFilter()
    {
        var mockGen = new MockGenApp();
        var vm = new RollGameViewModel(mockGen);
        mockGen.Games.Add(new AppSavedContext 
        { 
            AppId = 1, 
            AppName = "Test Game", 
            HeaderImage = "http://example.com/img.png" 
        });

        // Act & Assert
        // Logic for testing GenerateGames goes here
    }
}
