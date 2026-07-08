using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.DbContext;
using GameRandom.Scripts.Service;
using GameRandom.Views;

namespace GameRandom.Scripts.WindowServices;

public class AdminConfirmService : AbstractWindowService<AdminConfirmWindow>
{
    private Queue<FinishedGames>? _dialogQueue = new();
    public bool IsOpen { get; private set; } = false;

    public AdminConfirmService(Window owner) : base(owner)
    {
        ControlWindow.Closing += (_, _) => ClosingWindow();
    }
    
    public override void ShowWindow(object? data = null)
    {
        if (data is FinishedGames gameInfo)
        {
            ControlWindow = new();
            ControlWindow.LoadData(gameInfo);
        }
        
        if (data is null)
            Logger.Error("Admin confirm service get empty data");
        
        if (ControlWindow.IsActive)
            Logger.Error("Window is active, failed to open");
    }

    public override async Task ShowWindowAsync(object? data = null)
    {
        if (data is not List<FinishedGames> gamesInfo || gamesInfo.Count == 0) return;

        ControlWindow = new AdminConfirmWindow();
        _dialogQueue = new Queue<FinishedGames>(gamesInfo);
        ControlWindow.LoadData(_dialogQueue.Dequeue());
        
        await base.ShowWindowAsync(data);
    }

    public void AddNextDialog(FinishedGames game)
    {
        _dialogQueue?.Enqueue(game);
    }

    private void LoadNextDialog()
    {
        var nextGame = _dialogQueue!.Dequeue();
        ControlWindow.LoadData(nextGame);
    }

    private bool NextDialog()
    {
        if (_dialogQueue is null || _dialogQueue.Count == 0)
            return false;
        
        return true;
    }

    private void ClosingWindow()
    {
        if (NextDialog())
        {
            LoadNextDialog();
            return;
        }
        
        IsOpen = false;
    }
}