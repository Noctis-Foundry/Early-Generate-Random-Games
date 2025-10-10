using System;
using System.ComponentModel;

namespace GameRandom.Scr.DI;

public static class Di
{
    private static readonly DiContainer DiContainer = new DiContainer();
    public static DiContainer Container => DiContainer;
}