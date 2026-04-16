using System;
using Avalonia.Interactivity;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;

namespace GameRandom.Scripts.UserControls;

public interface IUserControl
{
    public void CloseUserControl(object? sender, RoutedEventArgs e);
    public void AddListener(Action<ControlTypes> changeUserControl);
    public void Dispose();
}