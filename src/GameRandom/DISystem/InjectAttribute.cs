using System;

namespace GameRandom.DISystem;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class InjectAttribute : Attribute
{
}