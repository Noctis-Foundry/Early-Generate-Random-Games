using System.Collections.Generic;
using System.Text.Json.Serialization;
using GameRandom.AvaloniaConverters;

namespace GameRandom.CoreApp;

public class AppSavedContext
{
    public int AppId { get; set; }
    public int AppReleaseYear { get; set;}
    public string AppName { get; set;}
    public string AppDescription { get; set;}
    public string HeaderImage { get; set;}

    [JsonConverter(typeof(DictionaryValuesToHashSetConverter))]
    public HashSet<string> AppGenres { get; set; } = [];

    [JsonConverter(typeof(DictionaryValuesToHashSetConverter))]
    public HashSet<string> AppCategoris { get; set; } = [];
}