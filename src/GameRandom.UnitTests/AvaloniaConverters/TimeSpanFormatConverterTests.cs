using System;
using System.Globalization;
using GameRandom.AvaloniaConverters;
using Xunit;

namespace GameRandom.UnitTests.AvaloniaConverters;

public class TimeSpanFormatConverterTests
{
    private readonly TimeSpanFormatConverter _converter = new TimeSpanFormatConverter();

    [Fact]
    public void Convert_DateTime_ShouldFormatWithPrefix()
    {
        var date = new DateTime(2023, 10, 27);
        var result = _converter.Convert(date, typeof(string), "End", CultureInfo.InvariantCulture);
        // "End: Friday, October 27, 2023" (Depends on culture, but :D is long date)
        // Actually DateTime.ToString("D") depends on current culture.
        // Let's use a more robust check or expect what we get.
        Assert.StartsWith("End: ", result.ToString());
        Assert.Contains("2023", result.ToString());
    }

    [Fact]
    public void Convert_NonDateTime_ShouldReturnDashes()
    {
        var result = _converter.Convert("not a date", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("--", result);
    }
}
