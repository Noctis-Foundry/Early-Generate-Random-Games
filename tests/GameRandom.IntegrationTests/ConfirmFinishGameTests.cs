using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;
using GameRandom.ViewModels;
using Xunit;

namespace GameRandom.IntegrationTests;

public class ConfirmFinishGameTests
{
    [Fact]
    public void ViewModel_Properties_Are_Initialized()
    {
        var viewModel = new ConfirmFinishGameViewModel();
        
        Assert.Null(viewModel.GameProgress);
        Assert.Null(viewModel.ImageBitmap);
    }

    [Fact]
    public void ViewModel_GameProgress_Can_Be_Set()
    {
        var viewModel = new ConfirmFinishGameViewModel();
        var gameProgress = new GameProgresses
        {
            AppId = 123,
            AppName = "Test Game",
            Comment = "Test Comment"
        };

        viewModel.GameProgress = gameProgress;

        Assert.NotNull(viewModel.GameProgress);
        Assert.Equal("Test Game", viewModel.GameProgress.AppName);
        Assert.Equal("Test Comment", viewModel.GameProgress.Comment);
    }

    [Fact]
    public async Task SaveEditAsync_Executes_Without_Exception()
    {
        var viewModel = new ConfirmFinishGameViewModel();
        
        await viewModel.SaveEditAsync();
        
        Assert.True(true);
    }

    [Fact]
    public void ViewModel_PropertyChanged_Fires_On_GameProgress_Change()
    {
        var viewModel = new ConfirmFinishGameViewModel();
        bool propertyChanged = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(ConfirmFinishGameViewModel.GameProgress))
                propertyChanged = true;
        };

        viewModel.GameProgress = new GameProgresses { AppName = "Test" };

        Assert.True(propertyChanged);
    }

    [Fact]
    public void GameProgress_Comment_Can_Be_Updated()
    {
        var viewModel = new ConfirmFinishGameViewModel();
        var gameProgress = new GameProgresses
        {
            AppId = 456,
            AppName = "Another Game",
            Comment = "Initial Comment"
        };

        viewModel.GameProgress = gameProgress;
        viewModel.GameProgress.Comment = "Updated Comment";

        Assert.Equal("Updated Comment", viewModel.GameProgress.Comment);
    }

    [Fact]
    public void Multiple_Property_Changes_Fire_Events()
    {
        var viewModel = new ConfirmFinishGameViewModel();
        int changeCount = 0;

        viewModel.PropertyChanged += (sender, args) => changeCount++;

        viewModel.GameProgress = new GameProgresses { AppName = "Game 1" };
        viewModel.GameProgress = new GameProgresses { AppName = "Game 2" };

        Assert.Equal(2, changeCount);
    }
}
