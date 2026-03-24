using System;
using GameRandom.Scr.DI;
using Xunit;

namespace GameRandom.UnitTests.Scr.DI;

public class DiFactoryTests
{
    private class MockRegister : Register
    {
        public string? Arg1 { get; private set; }
        public int Arg2 { get; private set; }
        public bool InitCalled { get; private set; }

        public override void Init<T1>(T1 arg1)
        {
            Arg1 = arg1?.ToString();
            InitCalled = true;
        }

        public override void Init<T1, T2>(T1 arg1, T2 arg2)
        {
            Arg1 = arg1?.ToString();
            if (arg2 is int i) Arg2 = i;
            InitCalled = true;
        }
    }

    private interface IMockRegister { }
    private class MockRegisterWithInterface : MockRegister, IMockRegister { }

    [Fact]
    public void Create_OneArg_Should_InitAndRegister()
    {
        // Arrange
        var factory = new DiFactory();
        var instance = new MockRegister();
        string testArg = "test";

        // Act
        factory.Create(instance, testArg);

        // Assert
        Assert.True(instance.InitCalled);
        Assert.Equal(testArg, instance.Arg1);
        var retrieved = Di.Container.GetInstance<MockRegister>();
        Assert.Same(instance, retrieved);
    }

    [Fact]
    public void Create_WithInterface_Should_InitAndRegisterAsInterface()
    {
        // Arrange
        var factory = new DiFactory();
        var instance = new MockRegisterWithInterface();
        string testArg = "test";

        // Act
        factory.Create<IMockRegister, MockRegisterWithInterface, string>(instance, testArg);

        // Assert
        Assert.True(instance.InitCalled);
        var retrieved = Di.Container.GetInstance<IMockRegister>();
        Assert.Same(instance, retrieved);
    }

    [Fact]
    public void Create_TwoArgs_Should_InitAndRegister()
    {
        // Arrange
        var factory = new DiFactory();
        var instance = new MockRegister();
        string testArg1 = "test";
        int testArg2 = 42;

        // Act
        factory.Create(instance, testArg1, testArg2);

        // Assert
        Assert.True(instance.InitCalled);
        Assert.Equal(testArg1, instance.Arg1);
        Assert.Equal(testArg2, instance.Arg2);
        var retrieved = Di.Container.GetInstance<MockRegister>();
        Assert.Same(instance, retrieved);
    }
}
