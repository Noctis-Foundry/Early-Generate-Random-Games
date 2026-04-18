using System;
using Avalonia.Controls;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;

namespace GameRandom.ViewModels.MainWindowSystem.Interface;

public interface IControlNavigate
{
    IObservable<object> ControlContent { get; }
    void BindingNavigateSystem();
    void Navigate(ControlTypes controlType);

    void Dispose();
}