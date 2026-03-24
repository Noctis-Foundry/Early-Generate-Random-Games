using System;
using System.Globalization;
using GameRandom.AvaloniaConverters;
using Xunit;

namespace GameRandom.UnitTests.AvaloniaConverters;

public class LongToStringConverterTests
{
    private readonly LongToStringConverter _converter = new LongToStringConverter();

    [Fact]
    public void Convert_LongValue_ShouldIncludePrefix()
    {
        var result = _converter.Convert(12345L, typeof(string), "Lobby", CultureInfo.InvariantCulture);
        Assert.Equal("Lobby: 12345", result);
    }

    [Fact]
    public void Convert_LongValue_NoPrefix_ShouldReturnFormattedValue()
    {
        var result = _converter.Convert(12345L, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(": 12345", result);
    }

    [Fact]
    public void Convert_NonLong_ShouldReturnEmptyLobbyID()
    {
        var result = _converter.Convert("not a long", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("Empty lobby ID", result);
    }
}
