using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRandom.AvaloniaConverters;

public class DictionaryValuesToHashSetConverter : JsonConverter<HashSet<string>>
{
    public override HashSet<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<int, String>>(ref reader, options);
        return dict?.Values.ToHashSet() ?? new HashSet<string>();
    }

    public override void Write(Utf8JsonWriter writer, HashSet<string> value, JsonSerializerOptions options)
    {
        
    }
}