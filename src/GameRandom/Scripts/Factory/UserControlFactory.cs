using System;
using GameRandom.Scr.DI;

namespace GameRandom.Src.Factory;

public class UserControlFactory : DependenceBase
{
    public TType CreateUserControl<TType>(Action<string> onNavigate) where TType :  MainWindowUserControlAbstract, new()
    {
        var newClass = new TType();
        newClass.AddListener(onNavigate);

        return newClass;
    }
}