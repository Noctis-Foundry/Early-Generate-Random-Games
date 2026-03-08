using System.Collections.Generic;
using Avalonia.Controls;
using GameRandom.DataBaseContexts;
using GameRandom.Views;

namespace GameRandom.SteamSDK;
using System.Threading.Tasks;

public class AdminConfirmService
{
    private Window _ownerWindow;
    private AdminConfirmWindow _adminConfirmWindow = new AdminConfirmWindow();

    private Queue<GameProgresses>? _dialogQueue = new Queue<GameProgresses>();
    
    public AdminConfirmService(Window owner)
    {
        _ownerWindow = owner;

        _adminConfirmWindow.Closing += (sender, args) =>
        {
            OnClosing();
        };
    }
    
    public async Task OpenDialogWindowAsync(GameProgresses gameInfo)
    {
        await ShowWindow(gameInfo);
    }

    public async Task OpenDialogWindowWithList(List<GameProgresses> list)
    {
        if (list.Count <= 0) return;
        
        _dialogQueue = new Queue<GameProgresses>(list);
        
        var firstGame = _dialogQueue.Dequeue();
        
        await ShowWindow(firstGame);
    }

    private void OnClosing()
    {
        if (NextDialog())
        {
            LoadNextDialog();
        }
    }

    private void LoadNextDialog()
    {
        var nextGame = _dialogQueue!.Dequeue();
        _adminConfirmWindow.LoadData(nextGame);
    }

    private bool NextDialog()
    {
        if (_dialogQueue is null || _dialogQueue.Count == 0)
            return false;
        
        return true;
    }
    
    private async Task ShowWindow(GameProgresses gameInfo)
    {
        await _adminConfirmWindow.ShowDialog(_ownerWindow);
        _adminConfirmWindow.LoadData(gameInfo);
    }
}