using System;
using System.Globalization;
using GameRandom.AvaloniaConverters;
using Xunit;

namespace GameRandom.UnitTests.AvaloniaConverters;

public class BoolConverterTests
{
    private readonly BoolConverter _converter = new BoolConverter();

    [Fact]
    public void Convert_True_ShouldReturnCompleted()
    {
        var result = _converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("Completed", result);
    }

    [Fact]
    public void Convert_False_ShouldReturnInProgress()
    {
        var result = _converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("In Progress", result);
    }

    [Fact]
    public void Convert_NonBool_ShouldReturnUnknown()
    {
        var result = _converter.Convert("not a bool", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void ConvertBack_CompletedString_ShouldReturnTrue()
    {
        var result = _converter.ConvertBack("Completed", typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.True((bool)result);
    }

    [Fact]
    public void ConvertBack_InProgressString_ShouldReturnFalse()
    {
        var result = _converter.ConvertBack("In Progress", typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.False((bool)result);
    }

    [Fact]
    public void ConvertBack_NonString_ShouldReturnFalse()
    {
        var result = _converter.ConvertBack(123, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.False((bool)result);
    }
}
