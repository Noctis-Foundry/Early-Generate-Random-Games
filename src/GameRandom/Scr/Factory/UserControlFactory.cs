using System;

namespace GameRandom.SteamSDK.Factory;

public class UserControlFactory
{
    public TType CreateUserControl<TType>(Action<string> onNavigate) where TType :  MainWindowUserControlAbstract, new()
    {
        var newClass = new TType();
        newClass.AddListener(onNavigate);

        return newClass;
    }
}