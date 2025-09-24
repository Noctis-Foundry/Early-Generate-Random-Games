using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using GameRandom.CoreApp;
using GameRandom.Service;
using GameRandom.SteamSDK;
using Steamworks;
using Avalonia.Animation;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class MainWindow : Window
{
    private readonly Register<string, UserControl> _userControlRegister = new();
    private readonly Action<string> _changeContent;
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
        
        _changeContent = Navigate;
        
        InitializeUserControlRegister();
        
        Navigate("Main");
    }

    private void InitializeUserControlRegister()
    {
        var mainContent = new MainWindowContent();
        mainContent.AddListener(_changeContent);

        var rollContent = new RollGame();
        rollContent.AddListener(_changeContent);

        var profileContent = new ProfileContent();
        profileContent.AddListener(_changeContent);
        
        _userControlRegister.RegisterNewObject("Main", mainContent);
        _userControlRegister.RegisterNewObject("Roll", rollContent);
        _userControlRegister.RegisterNewObject("Profile", profileContent);
    }
    
    private void Navigate(string nameControl)
    {
        ControlMain.Content = _userControlRegister.GetObjectFromRegister(nameControl);
    }
}