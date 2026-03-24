using System.Collections.Generic;
using GameRandom.Scr.Service;
using Xunit;

namespace GameRandom.UnitTests.Services;

public class ObservableConverterTests
{
    private readonly ObservableConverter _converter = new();

    [Fact]
    public void ToObservableCollection_Should_ConvertCorrectly()
    {
        // Arrange
        var list = new List<string> { "item1", "item2" };

        // Act
        var result = _converter.ToObservableCollection(list);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("item1", result[0]);
        Assert.Equal("item2", result[1]);
    }

    [Fact]
    public void ToObservableCollection_EmptyList_Should_ReturnEmptyCollection()
    {
        // Arrange
        var list = new List<int>();

        // Act
        var result = _converter.ToObservableCollection(list);

        // Assert
        Assert.Empty(result);
    }
}
