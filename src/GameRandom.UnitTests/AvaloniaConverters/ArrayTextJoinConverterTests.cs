using System;
using System.Collections.Generic;
using System.Globalization;
using GameRandom.AvaloniaConverters;
using Xunit;

namespace GameRandom.UnitTests.AvaloniaConverters;

public class ArrayTextJoinConverterTests
{
    private readonly ArrayTextJoinConverter _converter = new ArrayTextJoinConverter();

    [Fact]
    public void Convert_HashSet_ShouldJoinWithPrefix()
    {
        var hashSet = new HashSet<string> { "Action", "RPG" };
        var result = _converter.Convert(hashSet, typeof(string), "Genres", CultureInfo.InvariantCulture);
        Assert.Equal("Genres: Action, RPG", result);
    }

    [Fact]
    public void Convert_EmptyHashSet_ShouldReturnOnlyPrefix()
    {
        var hashSet = new HashSet<string>();
        var result = _converter.Convert(hashSet, typeof(string), "Genres", CultureInfo.InvariantCulture);
        Assert.Equal("Genres: ", result);
    }

    [Fact]
    public void Convert_NonHashSet_ShouldReturnDashes()
    {
        var result = _converter.Convert("not a hashset", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("--", result);
    }
}
