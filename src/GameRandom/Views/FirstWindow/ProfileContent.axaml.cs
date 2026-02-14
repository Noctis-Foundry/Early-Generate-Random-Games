using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;
using Microsoft.EntityFrameworkCore;
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
    }
    
    public void AddListener(Action<string> _onChangeContent) => _changeContent = _onChangeContent;

    public void Open()
    {
        InitProfileAvatar();
        
        DataContext = new ProfileViewModel();
        
        if (DataContext is ProfileViewModel profileViewModel)
            Dispatcher.UIThread.InvokeAsync(() => profileViewModel.LoadTable());
    }

    public void Close(object? sender, RoutedEventArgs e)
    {
        _changeContent?.Invoke("Main");
        
        if (DataContext is ProfileViewModel profileViewModel)
            profileViewModel.UnloadTable();
        
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