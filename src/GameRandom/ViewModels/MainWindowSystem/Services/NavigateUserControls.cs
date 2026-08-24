using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls;
using GameRandom.DISystem;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts.Factory;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.UserControls;
using GameRandom.ViewModels.MainWindowSystem.Enums;
using GameRandom.ViewModels.MainWindowSystem.Interface;
using GameRandom.Views;
using GameTable = GameRandom.Views.GameTable;
using LoadControl = GameRandom.Views.MainWindowSystem.LoadControl;
using ProfileContent = GameRandom.Views.ProfileContent;
using RollGame = GameRandom.Views.RollGame;

namespace GameRandom.ViewModels.MainWindowSystem.Services;

public class NavigateUserControls : IControlNavigate, IDisposable
{
    
    private readonly BehaviorSubject<object> _controlContent;
    public IObservable<object> ControlContent => _controlContent.AsObservable();

    [Inject] private UserControlFactory _controlFactory = null!;
    /// <summary>
    /// Registry for user control factories mapped by navigation keys.
    /// </summary>
    private readonly Register<ControlTypes, Func<UserControl>> _preloadRegister = new();

    private object _currentControl;
    
    /// <summary>
    /// Action delegate for navigating between user controls.
    /// </summary>
    private Action<ControlTypes> _changeUserControlAction;

    private bool _isInitializeDi = false;

    public NavigateUserControls()
    {
        var loadControl = new LoadControl();
        _controlContent = new BehaviorSubject<object>(loadControl);
        _currentControl = loadControl;
    }
    
    public void BindingNavigateSystem()
    {
        if (!_isInitializeDi)
            InitializeUserFactory();

        _changeUserControlAction = Navigate;
        
        InitializeUserControlRegister();
    }
    
    /// <summary>
    /// Registers navigation targets for user controls.
    /// </summary>
    private void
        InitializeUserControlRegister() //TODO Change IUserControl in MainWindowUserControlAbstract for Profile and GameTable
    {
        _preloadRegister.RegisterNewObject(ControlTypes.MainWindow,
            () => _controlFactory.CreateUserControl<MainWindowContent>(_changeUserControlAction));
        _preloadRegister.RegisterNewObject(ControlTypes.Profile,
            () => _controlFactory.CreateUserControl<ProfileContent>(_changeUserControlAction));
        _preloadRegister.RegisterNewObject(ControlTypes.Roll,
            () => _controlFactory.CreateUserControl<RollGame>(_changeUserControlAction));
        _preloadRegister.RegisterNewObject(ControlTypes.GameTable,
            () => _controlFactory.CreateUserControl<GameTable>(_changeUserControlAction));
        _preloadRegister.RegisterNewObject(ControlTypes.Admin,
            () => _controlFactory.CreateUserControl<AdminPanel>(_changeUserControlAction));
    }

    private void InitializeUserFactory()
    {
        Di.ResolveInstance.ResolveFiled(out _controlFactory);
        
        if (_controlFactory is null)
            throw new NullReferenceException(nameof(_controlFactory));

        _isInitializeDi = true;
    }

    /// <summary>
    /// Navigates to the specified user control.
    /// </summary>
    /// <param name="controlType"></param>
    /// <exception cref="NullReferenceException">Thrown when control creation fails.</exception>
    public void Navigate(ControlTypes controlType)
    {
        if (_preloadRegister.GetObjectFromRegister(controlType, out var func))
        {
            var content = func?.Invoke();

            if (content is null)
                throw new NullReferenceException($"Failed navigate to {controlType}");

            if (content is not IUserControl value)
                return;

            if (_currentControl is IDisposable disposable)
                disposable.Dispose();

            _controlContent.OnNext(content);
            _currentControl = content;
        }
    }

    public void Dispose()
    {
        _controlContent.Dispose();
        _preloadRegister.Dispose();
        _currentControl = null!;
        _changeUserControlAction = null!;
    }
}