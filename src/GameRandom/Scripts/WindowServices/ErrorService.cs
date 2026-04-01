using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src.Enums;
using GameRandom.Views;

namespace GameRandom.Src;

public class ErrorService : AbstractWindowService<ErrorWindow>
{
    private readonly Queue<ErrorStruct> _queue = new();

    public ErrorService(Window owner) : base(owner)
    {
        ControlWindow.Closing += (sender, args) => ClosingWindow();
    }

    public override void ShowWindow(object? data = null)
    {
        if (data is ErrorStruct errorStruct)
            ShowErrorWindow(errorStruct);
    }

    public void ShowWindow(string message, ErrorEnum errorEnum = ErrorEnum.Error)
    {
        if (string.IsNullOrEmpty(message))
            return;
        
        ShowErrorWindow(new ErrorStruct{ErrorMessage = message, ErrorType = errorEnum});
    }

    private void ShowErrorWindow(ErrorStruct errorStruct)
    {
        if (ControlWindow.IsActive)
        {
            _queue.Enqueue(errorStruct);
            return;
        }
        
        ControlWindow.ChangeTextOnModal(errorStruct.ErrorMessage, errorStruct.ErrorType);
        ControlWindow.Show();
    }

    protected void ClosingWindow()
    {
        TryGoNext();
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