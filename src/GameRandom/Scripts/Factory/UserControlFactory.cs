using System;
using Avalonia.Controls;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scripts.UserControls;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.Src.Factory;

public class UserControlFactory : DependenceBase
{
    public TType CreateUserControl<TType>(Action<ControlTypes> onNavigate) where TType : UserControl, IUserControl, new()
    {
        var newClass = new TType();
        newClass.AddListener(onNavigate);

        return newClass;
    }
}