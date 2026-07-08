using System;
using Avalonia.Controls;
using GameRandom.DISystem;
using GameRandom.Scripts.UserControls;
using GameRandom.ViewModels.MainWindowSystem.Enums;

namespace GameRandom.Scripts.Factory;

public class UserControlFactory : DependenceBase
{
    public TType CreateUserControl<TType>(Action<ControlTypes> onNavigate) where TType : UserControl, IUserControl, new()
    {
        var newClass = new TType();
        newClass.AddListener(onNavigate);

        return newClass;
    }
}