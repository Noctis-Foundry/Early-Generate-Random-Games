using System;

namespace GameRandom.Scr.DI;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class InjectAttribute : Attribute
{
}