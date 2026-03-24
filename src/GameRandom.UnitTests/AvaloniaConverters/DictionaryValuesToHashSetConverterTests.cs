using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using GameRandom.AvaloniaConverters;
using Xunit;

namespace GameRandom.UnitTests.AvaloniaConverters;

public class DictionaryValuesToHashSetConverterTests
{
    private readonly JsonSerializerOptions _options;

    public DictionaryValuesToHashSetConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new DictionaryValuesToHashSetConverter());
    }

    [Fact]
    public void Deserialize_Dictionary_ShouldReturnHashSetOfValues()
    {
        string json = "{\"1\": \"Action\", \"2\": \"RPG\", \"3\": \"Action\"}";
        var result = JsonSerializer.Deserialize<HashSet<string>>(json, _options);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("Action", result);
        Assert.Contains("RPG", result);
    }

    [Fact]
    public void Deserialize_EmptyDictionary_ShouldReturnEmptyHashSet()
    {
        string json = "{}";
        var result = JsonSerializer.Deserialize<HashSet<string>>(json, _options);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Deserialize_Null_ShouldReturnNull()
    {
        string json = "null";
        var result = JsonSerializer.Deserialize<HashSet<string>>(json, _options);

        // System.Text.Json returns null for "null" for reference types 
        // without calling the custom converter's Read method in many cases.
        Assert.Null(result);
    }
}
