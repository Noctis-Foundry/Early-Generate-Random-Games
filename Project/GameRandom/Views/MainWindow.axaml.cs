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
        
        _changeContent = Navigate;
        
        _userControlRegister.RegisterNewObject("Roll", new RollGame(_changeContent));
        _userControlRegister.RegisterNewObject("Profile", new ProfileContent(_changeContent));
        _userControlRegister.RegisterNewObject("Main", new MainWindowContent(_changeContent));

        if (DataContext is MainWindowViewModel vm)
        {
            vm.CurrentContent = _userControlRegister.GetObjectFromRegister("Main");
        }
    }
    
    private void Navigate(string nameControl)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.CurrentContent = _userControlRegister.GetObjectFromRegister(nameControl);
        }
    }
}