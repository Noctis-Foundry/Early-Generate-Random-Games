using GameRandom.SteamSDK;
using Xunit;

namespace GameRandom.UnitTests.Services;

public class SteamServiceTests
{
    [Fact]
    public void AppSteamPage_Should_ReturnCorrectUrl()
    {
        // Arrange
        var service = new SteamService();
        int appId = 440; // Team Fortress 2

        // Act
        var result = service.AppSteamPage(appId);

        // Assert
        Assert.Equal("https://store.steampowered.com/app/440/?I=english", result);
    }
}
