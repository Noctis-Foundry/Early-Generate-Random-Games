using System;
using System.Collections.Generic;
using System.Reflection;
using GameRandom.Scr.DI;
using Xunit;

namespace GameRandom.UnitTests.Scr.DI;

public class DiContainerTests
{
    private class MockService { }
    private interface IMockInterface { }
    private class MockImplementation : IMockInterface { }

    private class InjectTarget
    {
        [Inject]
        private MockService? _service;
        
        public MockService? Service => _service;
    }

    [Fact]
    public void RegisterSingleInstance_Should_StoreInstance()
    {
        // Arrange
        var container = new DiContainer();
        var service = new MockService();

        // Act
        container.RegisterSingleInstance(service);

        // Assert
        var retrieved = container.GetInstance<MockService>();
        Assert.Same(service, retrieved);
    }

    [Fact]
    public void GetInstance_NotRegistered_Should_ThrowException()
    {
        // Arrange
        var container = new DiContainer();

        // Act & Assert
        Assert.Throws<Exception>(() => container.GetInstance<MockService>());
    }

    [Fact]
    public void TryGetInstance_NotRegistered_Should_ReturnNull()
    {
        // Arrange
        var container = new DiContainer();

        // Act
        var result = container.TryGetInstance<MockService>();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Constructor_Should_RegisterSelf()
    {
        // Act
        var container = new DiContainer();

        // Assert
        var retrieved = container.GetInstance<DiContainer>();
        Assert.Same(container, retrieved);
    }

    [Fact]
    public void ResolveFieldsFromClassInstance_Should_InjectDependencies()
    {
        // Arrange
        var container = new DiContainer();
        var service = new MockService();
        container.RegisterSingleInstance(service);
        
        var target = new InjectTarget();

        // Act
        container.ResolveFieldsFromClassInstance(target);

        // Assert
        Assert.Same(service, target.Service);
    }

    [Fact]
    public void ResolveField_Should_ReturnTrue_When_Registered()
    {
        // Arrange
        var container = new DiContainer();
        var service = new MockService();
        container.RegisterSingleInstance(service);

        // Act
        bool result = container.ResolveField<MockService>(out var retrieved);

        // Assert
        Assert.True(result);
        Assert.Same(service, retrieved);
    }

    [Fact]
    public void ResolveField_Should_ReturnFalse_When_NotRegistered()
    {
        // Arrange
        var container = new DiContainer();

        // Act
        bool result = container.ResolveField<MockService>(out var retrieved);

        // Assert
        Assert.False(result);
        Assert.Null(retrieved);
    }

    [Fact]
    public void ResolveFieldsFromClassInstance_Should_Ignore_Fields_Without_Inject_Attribute()
    {
        // Arrange
        var container = new DiContainer();
        var service = new MockService();
        container.RegisterSingleInstance(service);
        
        var target = new NonInjectTarget();

        // Act
        container.ResolveFieldsFromClassInstance(target);

        // Assert
        Assert.Null(target.Service);
    }

    private class NonInjectTarget
    {
        private MockService? _service;
        public MockService? Service => _service;
    }

    [Fact]
    public void GetInstance_ByType_Should_ReturnInstance()
    {
        // Arrange
        var container = new DiContainer();
        var service = new MockService();
        container.RegisterSingleInstance(service);

        container.ResolveField(out MockService? retrieved);

        // Assert
        Assert.Same(service, retrieved);
    }

    [Fact]
    public void TryGetInstance_ByType_Should_ReturnNull_When_NotRegistered()
    {
        // Arrange
        var container = new DiContainer();

        // Act
        var result = container.TryGetInstance<MockService>();

        // Assert
        Assert.Null(result);
    }
}
