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

public class ErrorService : AbstractWindowService<ErrorWindow>
{
    private readonly Queue<ErrorStruct> _queue = new();

    public ErrorService(Window owner) : base(owner)
    {
        GlobalExceptionHandler();
    }

    private void GlobalExceptionHandler()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                SaveInvokeOnUI(() => ShowWindow(new ErrorStruct{ErrorMessage = exception.Message, ErrorType = ErrorEnum.Error}));
            }
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            SaveInvokeOnUI(() => ShowWindow(new ErrorStruct{ErrorMessage = args.Exception.Message, ErrorType = ErrorEnum.Error}));
            args.SetObserved();
        };
    }

    public override void ShowWindow(object? data = null)
    {
        if (data is not null && data is ErrorStruct errorStruct)
        {
            ShowErrorWindow(errorStruct);
        }
    }

    private void ShowErrorWindow(ErrorStruct errorStruct)
    {
        if (ControlWindow.IsActive)
        {
            _queue.Enqueue(errorStruct);
            return;
        }
        
        ControlWindow.ChangeTextOnModal(errorStruct.ErrorMessage, errorStruct.ErrorType);
        ControlWindow.Open();
    }

    protected override void ClosingWindow()
    {
        TryGoNext();
    }


    private void SaveInvokeOnUI(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }

    private void TryGoNext()
    {
        if (_queue.Count > 0 && !ControlWindow.IsActive)
        {
            ShowErrorWindow(_queue.Dequeue());
        }
    }
}

public struct ErrorStruct
{
    public ErrorEnum ErrorType;
    public string ErrorMessage;
}