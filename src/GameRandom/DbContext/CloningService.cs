using System;
using System.Text.Json;

namespace GameRandom.DataBaseContexts;

public class CloningService
{
    private static Lazy<CloningService> _lazy = new(new CloningService());
    
    public static CloningService Instance => _lazy.Value;

    public T Clone<T>(T source)
    {
        if (source == null!)
            return default!;
        
        var options = new JsonSerializerOptions()
        {
            WriteIndented = false,
            IncludeFields = false,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
        };
        
        var json = JsonSerializer.Serialize(source, options);
        return JsonSerializer.Deserialize<T>(json, options)!;
    }
}