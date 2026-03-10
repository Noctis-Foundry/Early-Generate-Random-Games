using System.Collections.Generic;
using Avalonia.Controls;
using GameRandom.DataBaseContexts;
using GameRandom.Views;

namespace GameRandom.SteamSDK;
using System.Threading.Tasks;

public class AdminConfirmService(Window owner) : AbstractWindowService<AdminConfirmWindow>(owner)
{
    private Queue<FinishedGames>? _dialogQueue = new();
    public bool IsOpen { get; private set; } = false;

    public override void ShowWindow(object? data = null)
    {
        if (data is FinishedGames gameInfo && !ControlWindow.IsActive)
        {
            ControlWindow = new();
            ControlWindow.LoadData(gameInfo);
        }
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

    protected override void ClosingWindow()
    {
        if (NextDialog())
        {
            LoadNextDialog();
            return;
        }
        
        IsOpen = false;
        base.ClosingWindow();
    }
}