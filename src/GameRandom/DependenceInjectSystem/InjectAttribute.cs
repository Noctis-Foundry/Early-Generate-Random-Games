using System;

namespace GameRandom.DependenceInjectSystem;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class InjectAttribute : Attribute
{
}