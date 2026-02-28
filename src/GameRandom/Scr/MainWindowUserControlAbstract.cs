using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GameRandom.SteamSDK;

public abstract class MainWindowUserControlAbstract : UserControl, IDisposable
{
    protected Action<string>? _changeWindowAction;
    
    /// <summary>
    /// Registers navigation callback for content switching.
    /// </summary>
    public virtual void AddListener(Action<string> _onChangeContent) => _changeWindowAction = _onChangeContent;

    public abstract void Close(object? sender, RoutedEventArgs e);

    public virtual void Open()
    {
        
    }
    
    /// <summary>
    /// Cleans up resources and nullifies references.
    /// </summary>
    public virtual void Dispose()
    {
        _changeWindowAction = null;
    }
}