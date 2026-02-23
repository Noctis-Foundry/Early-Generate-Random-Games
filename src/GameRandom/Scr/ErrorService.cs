using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.Enums;
using GameRandom.Views;

namespace GameRandom.SteamSDK;

public class ErrorService : Register, IError
{
    private Window _ownerWindow;
    private ErrorWindow _errorWindow;

    private readonly Queue<ErrorStruct> _queue = new();

    public ErrorService()
    {
        GlobalExceptionHandler();
    }

    private void GlobalExceptionHandler()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                SaveInvokeOnUI(() => ShowErrorWindow(exception.Message, ErrorEnum.Error));
            }
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            SaveInvokeOnUI(() => ShowErrorWindow(args.Exception.Message, ErrorEnum.Error));
            args.SetObserved();
        };
    }

    public override void Init<T1>(T1 arg1)
    {
        if (arg1 is Window owner)
            _ownerWindow = owner;

        _errorWindow = new ErrorWindow();

        _errorWindow.Closing += (sender, args) =>
        {
            CloseErrorWindow();
        };
    }

    public void ShowErrorWindow(string message, ErrorEnum errorType)
    {
        if (_errorWindow.IsActiveWindow)
        {
            _queue.Enqueue(new ErrorStruct { ErrorMessage = message, ErrorType = errorType });
            return;
        }
        
        _errorWindow.ChangeTextOnModal(message, errorType);
        _errorWindow.IsActiveWindow = true;
        _errorWindow.Show(_ownerWindow);
    }

    private void CloseErrorWindow()
    {
        if (!_errorWindow.IsActiveWindow)
        {
            TryGoNext();
        }
    }


    private void SaveInvokeOnUI(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }

    private void TryGoNext()
    {
        if (_queue.Count > 0 && !_errorWindow.IsActiveWindow)
        {
            var error = _queue.Dequeue();
            ShowErrorWindow(error.ErrorMessage, error.ErrorType);
        }
    }
}

public interface IError
{
    public void ShowErrorWindow(string message, ErrorEnum errorType);
}

public struct ErrorStruct
{
    public ErrorEnum ErrorType;
    public string ErrorMessage;
}