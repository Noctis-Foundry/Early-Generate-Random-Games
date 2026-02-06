using System;

namespace GameRandom.SteamSDK;

public interface IAddListener
{
    void AddListener(Action<string> onChangeContent);
}