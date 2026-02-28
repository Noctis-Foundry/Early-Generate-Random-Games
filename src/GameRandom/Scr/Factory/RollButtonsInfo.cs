using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace GameRandom.SteamSDK.Factory;

public class RollButtonsInfo(Button button, Image image, RelayCommand command) : IDisposable
{
    Button? AppButton = button;
    Image? AppImage = image;
    RelayCommand? AppCommand = command;

    public void Dispose()
    {
        AppButton = null;
        AppImage = null;
        AppCommand = null;
    }
}