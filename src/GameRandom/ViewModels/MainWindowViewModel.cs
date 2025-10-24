using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.WindowScr;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.Views;
using GameRandom.Views.LobbyModalWindow;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IWindowService _windowService;
    public ICommand OpenLobbyCommand { get; }
    public ICommand CreateLobbyCommand { get; }

    public MainWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenLobbyCommand = new RelayCommand(OpenLobby);
        CreateLobbyCommand = new RelayCommand(OpenCreateLobbyWindow);
    }

    public async Task UpdateLobby(Grid lobbyGrid)
    {
        lobbyGrid.Children.Clear();
        
        IError? error = Di.Container.TryGetInstance<IError>() as ErrorService;

        List<LobbyContext> lobbyList;
        
        await using (var db = new AppDbContext())
        {
            lobbyList = db.LobbyContexts.ToListAsync().Result;

            if (lobbyList.Count <= 0 || lobbyList == null)
            {
                return;
            }
        }
        
        var factory = Di.Container.GetInstance<MainWindowFactory>() as MainWindowFactory;
        
        if (factory == null)
            throw new Exception("No main window factory found");
        
        var imageList = factory.CreateImageInGrid(lobbyList.Count, lobbyGrid);
        
        Console.WriteLine($"imageList count {imageList?.Count}");
        
        if (imageList == null || imageList.Count <= 0)
        {
            if (error != null)
            {
                error.ShowErrorWindow("Failed to update lobbies. No loaded member images");
            }
                
            return;
        }
        
        for (int i = 0; i < lobbyList.Count; i++)
        {
            CSteamID memberId = new CSteamID(lobbyList[i].MemberID);
            int imageUrl = SteamFriends.GetLargeFriendAvatar(memberId);
                
            Bitmap bitmap = AvaloniaService.CreateSteamImage(imageUrl);

            imageList[i].Source = bitmap;   
        }
    }
    
    public async void OpenLobby()
    {
        await _windowService.ShowDialogAsync<LobbyWindow>();
    }

    public async void OpenCreateLobbyWindow()
    {
        await _windowService.ShowDialogAsync<CreateLobby>();
    }

    public async void ShowRules()
    {
        await _windowService.ShowDialogAsync<LobbyWindow>();
    }
    
    public void ShowError()
    {
        IError? error = Di.Container.TryGetInstance<IError>() as ErrorService;

        if (error != null)
        {
            error.ShowErrorWindow("Open error modal");
        }
        else
        {
            throw new Exception("Not fount error modal");
        }
    }
}