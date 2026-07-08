namespace GameRandom.Scripts.RollGameSystem.GenerateGames;

public class AppInfo (AppSavedContext savedContext, byte[] imageBytes)
{
    public byte[] ImageBytes { get; } = imageBytes;
    public AppSavedContext AppData { get; } = savedContext;
}