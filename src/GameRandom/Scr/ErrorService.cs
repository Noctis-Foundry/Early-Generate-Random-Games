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

    private bool isActiveWindow = false;

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

        _errorWindow.Closed += (sender, args) =>
        {
            ClosedWindow();
        };
    }

    public void ShowErrorWindow(string message, ErrorEnum errorType)
    {
        if (isActiveWindow)
        {
            _queue.Enqueue(new ErrorStruct { ErrorMessage = message, ErrorType = errorType });
            return;
        }

        isActiveWindow = true;
        _errorWindow.ChangeTextOnModal(message, errorType);
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await _errorWindow.ShowDialog(_ownerWindow);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                _errorWindow.Close();
            }
        });
    }

    private void ClosedWindow()
    {
        isActiveWindow = false;
        TryGoNext();
    }

    private void SaveInvokeOnUI(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }

    private void TryGoNext()
    {
        if (_queue.Count > 0 && !isActiveWindow)
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