using System;
using Avalonia.Interactivity;

namespace GameRandom.SteamSDK;

public interface IUserControl
{
    void AddListener(Action<string> onChangeContent);
    void Open();
    void Close(object? sender, RoutedEventArgs e);
}