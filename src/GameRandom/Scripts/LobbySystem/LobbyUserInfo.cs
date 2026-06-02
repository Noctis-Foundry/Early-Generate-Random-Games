using System.Collections;
using Avalonia.Media.Imaging;

namespace GameRandom.Src.LobbySystem;

public class LobbyUserInfo(ulong userId, string userName, Bitmap avatarMap)
{
    public ulong UserId { get; set; } = userId;
    public string UserName { get; set; } = userName;
    public Bitmap AvatarData { get; set; } = avatarMap;
}