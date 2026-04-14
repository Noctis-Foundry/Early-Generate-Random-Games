using System;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;

namespace GameRandom.Src.Factory;

public class UserControlFactory : DependenceBase
{
    public TType CreateUserControl<TType>(Action<ControlTypes> onNavigate) where TType :  MainWindowUserControlAbstract, new()
    {
        var newClass = new TType();
        newClass.AddListener(onNavigate);

        return newClass;
    }
}