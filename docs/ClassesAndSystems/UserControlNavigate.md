# NavigateUserControls

## Overview

`NavigateUserControls` is a navigation service that manages switching between user controls in the main window. It implements `IControlNavigate` and exposes the current control as a reactive observable stream via `BehaviorSubject`.

## Location

`src/GameRandom/ViewModels/MainWindowSystem/Services/NavigateUserControls.cs`

## Implements

`IControlNavigate`

## Properties

| Property | Type | Description |
|---|---|---|
| `ControlContent` | `IObservable<object>` | Observable stream of the currently active user control |

## Fields

| Field | Type | Description |
|---|---|---|
| `_controlContent` | `BehaviorSubject<object>` | Backing subject emitting the current control |
| `_currentControl` | `object` | Reference to the currently displayed control |
| `_controlFactory` | `UserControlFactory` | DI-injected factory for creating user controls |
| `_preloadRegister` | `Register<ControlTypes, Func<UserControl>>` | Registry mapping `ControlTypes` keys to control factory functions |
| `_changeUserControlAction` | `Action<ControlTypes>` | Delegate passed to controls for triggering navigation |
| `_isInitializeDi` | `bool` | Guards against repeated DI resolution |

## Methods

### `BindingNavigateSystem()`
Initializes the navigation system. Resolves DI dependencies if not yet done, assigns the navigation delegate, and registers all control factories.

### `Navigate(ControlTypes controlType)`
Navigates to the specified control type:
1. Looks up the factory in `_preloadRegister`.
2. Invokes the factory to create the control.
3. Disposes the previous control if it implements `IDisposable`.
4. Pushes the new control to `_controlContent`.
5. Calls `Open()` on the new control if it extends `MainWindowUserControlAbstract`.

Throws `NullReferenceException` if the factory returns null.

### `InitializeUserControlRegister()` *(private)*
Registers factory functions for all navigable controls:

| Key | Control |
|---|---|
| `ControlTypes.MainWindow` | `MainWindowContent` |
| `ControlTypes.Profile` | `ProfileContent` |
| `ControlTypes.Roll` | `RollGame` |
| `ControlTypes.GameTable` | `GameTable` |
| `ControlTypes.Admin` | `AdminPanel` |

### `InitializeUserFactory()` *(private)*
Resolves `UserControlFactory` from the DI container via `Di.ResolveInstance.ResolveFiled`. Throws `NullReferenceException` if resolution fails.

## Constructor

Initializes `_controlContent` and `_currentControl` with a `LoadControl` instance as the default placeholder shown before navigation begins.

## Usage Example

```csharp
var navigator = new NavigateUserControls();
navigator.BindingNavigateSystem();

// Bind to UI
navigator.ControlContent.Subscribe(control => ContentArea.Content = control);

// Navigate
navigator.Navigate(ControlTypes.Roll);
```

## Notes

- DI injection of `_controlFactory` uses the `[Inject]` attribute and is resolved lazily on first `BindingNavigateSystem()` call.
- The `_changeUserControlAction` delegate is passed into each created control, enabling controls to trigger navigation themselves.
- Previous controls are disposed before switching to prevent resource leaks.
