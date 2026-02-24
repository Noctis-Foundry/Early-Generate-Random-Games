using Avalonia.Controls;

namespace GameRandom.SteamSDK;

public abstract class WindowAbstract : Window
{
    public bool IsActive { get; private set; } = false;

    public virtual void Open(Window? parent = null)
    {
        if (IsActive) return;
        
        IsActive = true;

        if (parent is not null)
        {
            Show(parent);
            return;
        }
        
        Show();
    }
    
    public virtual void CloseWindow()
    {
        Hide();
        IsActive = false;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Hide();
        IsActive = false;
        e.Cancel = true;
    }
}