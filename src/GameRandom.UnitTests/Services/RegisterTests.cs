using System;
using GameRandom.Service;
using Xunit;

namespace GameRandom.UnitTests.Services;

public class RegisterTests
{
    [Fact]
    public void RegisterNewObject_Should_AddObject()
    {
        // Arrange
        var register = new Register<string, string>();

        // Act
        register.RegisterNewObject("key1", "value1");

        // Assert
        bool found = register.GetObjectFromRegister("key1", out var value);
        Assert.True(found);
        Assert.Equal("value1", value);
    }

    [Fact]
    public void GetObjectFromRegister_NonExistentKey_Should_ReturnFalse()
    {
        // Arrange
        var register = new Register<string, string>();

        // Act
        bool found = register.GetObjectFromRegister("missing", out var value);

        // Assert
        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void RegisterNewObject_NullKey_Should_ThrowArgumentNullException()
    {
        // Arrange
        var register = new Register<string, string>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => register.RegisterNewObject(null!, "value"));
    }

    [Fact]
    public void RegisterNewObject_NullValue_Should_ThrowArgumentNullException()
    {
        // Arrange
        var register = new Register<string, string>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => register.RegisterNewObject("key", null!));
    }

    [Fact]
    public void GetObjectFromRegister_NullKey_Should_ThrowArgumentNullException()
    {
        // Arrange
        var register = new Register<string, string>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => register.GetObjectFromRegister(null!, out _));
    }
}
