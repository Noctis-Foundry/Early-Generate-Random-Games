using System;
using Avalonia.Controls;
using GameRandom.Scr.DI;
using GameRandom.Views;

namespace GameRandom.SteamSDK;

public class ErrorService : Register, IError
{
    private readonly ErrorWindow _errorWindow;
    private Window? _owner;
    
    private const string ErrorTitle = "Error ";
    
    public ErrorService()
    {
        _errorWindow = new ErrorWindow();
    }

    public override void Init<T1>(T1 arg1)
    {
        if (arg1 is Window mainWindow)
        {
            _owner = mainWindow;
        }
    }

    public void ShowErrorWindow(string message)
    {
        if (_owner == null)
            throw new Exception("_owner not set");
        
        _errorWindow.ShowDialog(_owner);
        _errorWindow.ChangeTextOnModal(string.Concat(ErrorTitle, message));
    }
}

public interface IError
{
    public void ShowErrorWindow(string message);
}