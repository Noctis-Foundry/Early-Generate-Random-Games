using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;

namespace GameRandom.ViewModels.CurrentGameSystem;

public class CurrentGameLoadData(GameProgresses gameInfo, Bitmap? imageBitmap, UserGame userGame)
{
    public GameProgresses GameInfo { get; private set; } = gameInfo;
    public Bitmap? ImageBitmap { get; private set; } = imageBitmap;
    public UserGame UserGame { get; private set; } = userGame;
}