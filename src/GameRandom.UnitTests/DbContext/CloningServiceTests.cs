using System;
using GameRandom.DataBaseContexts;
using Xunit;

namespace GameRandom.UnitTests.DbContext;

public class CloningServiceTests
{
    private readonly CloningService _service = CloningService.Instance;

    [Fact]
    public void Clone_SimpleObject_ShouldReturnNewInstanceWithSameValues()
    {
        var source = new Users
        {
            Id = 1,
            SteamId = 12345,
            Nickname = "Test",
            LobbyId = 678,
            AvatarURL = 9
        };

        var clone = _service.Clone(source);

        Assert.NotSame(source, clone);
        Assert.Equal(source.Id, clone.Id);
        Assert.Equal(source.SteamId, clone.SteamId);
        Assert.Equal(source.Nickname, clone.Nickname);
        Assert.Equal(source.LobbyId, clone.LobbyId);
        Assert.Equal(source.AvatarURL, clone.AvatarURL);
    }

    [Fact]
    public void Clone_Null_ShouldReturnDefault()
    {
        Users source = null!;
        var clone = _service.Clone(source);
        Assert.Null(clone);
    }
}
