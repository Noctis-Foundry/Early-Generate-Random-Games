using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;
using Steamworks;

namespace GameRandom.Views;

public partial class ProfileContent : UserControl, IDisposable, IUserControl
{
    private Action<string>? _changeContent;

    public ProfileContent()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
        DataContext = new ProfileViewModel();
    }
    
    public void AddListener(Action<string> _onChangeContent) => _changeContent = _onChangeContent;

    public void Open()
    {
        InitProfileAvatar();

        if (DataContext is ProfileViewModel profileViewModel)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            { 
                await profileViewModel.LoadTable();
                ProfileText.Text = profileViewModel.GameProgresses.Count.ToString(); //TODO Delete
            });
        }
    }

    public void Close(object? sender, RoutedEventArgs e)
    {
        _changeContent?.Invoke("Main");
        
        if (DataContext is ProfileViewModel profileViewModel)
            profileViewModel.UnloadTable();
        
        ProfileName.Text = "Profile"; //TODO Delete
        
        Dispose();
    }

    private void InitProfileAvatar()
    {
        CSteamID steamId = SteamManager.GetSteamManager().GetSteamId();
        
        string accName = SteamFriends.GetPersonaName();
        
        int imageId = SteamFriends.GetLargeFriendAvatar(steamId);
        
        var bitmap = AvaloniaService.CreateSteamImage(imageId);
        
        ProfileImage.Source = bitmap;
        ProfileName.Text = accName;
    }

    public void Dispose()
    {
        _changeContent = null;
        ProfileImage.Source = null;
        ProfileName.Text = string.Empty;
        DataContext = null;
    }
}