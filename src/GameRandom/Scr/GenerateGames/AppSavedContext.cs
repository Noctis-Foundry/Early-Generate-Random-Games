using System.Collections.Generic;

namespace GameRandom.CoreApp;

public class AppSavedContext
{
    public int AppId { get; set; }
    public int AppReleaseYear { get; set;}
    public string AppName { get; set;}
    public string AppDescription { get; set;}
    public string HeaderImage { get; set;}
    public Dictionary<int, string> AppGenres { get; set;}
    public Dictionary<int, string> AppCategoris { get; set;}

    public AppSavedContext(int appId, string appName, string appDescription, int appReleaseYear,
        Dictionary<int, string> appGenres, Dictionary<int, string> appCategoris,  string headerImage)
    {
        AppId = appId;
        AppName = appName;
        AppDescription = appDescription;
        AppReleaseYear = appReleaseYear;
        AppGenres = appGenres;
        AppCategoris = appCategoris;
        HeaderImage = headerImage;
    }
}