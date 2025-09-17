namespace GameRandom.CoreApp;

public class AppContext
{
    public int AppId { get; private set; }
    public string AppName { get; private set; }

    public AppContext(int appId, string appName)
    {
        AppId = appId;
        AppName = appName;
    }
}
public class AppFilterResult
{
    public AppData AppData { get; private set; }
    public bool IsValid { get; private set; }

    public AppFilterResult(AppData appData, bool isValid)
    {
        AppData = appData;
        IsValid = isValid;
    }
}

public struct AppFilterSend
{
    public string AppGenres { get; private set; }
    public int CountGenerate { get; private set; }
    
    public AppFilterSend(string appGenres, int  countGenerate)
    {
        AppGenres = appGenres;
        CountGenerate = countGenerate;
    }
}