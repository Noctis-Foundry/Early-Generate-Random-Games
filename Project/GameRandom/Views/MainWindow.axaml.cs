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

namespace GameRandom.Views;

public partial class MainWindow : Window
{
    private Action<bool> _changeOnRollContent;
    
    public MainWindow()
    {
        InitializeComponent();
        _changeOnRollContent += ChangeOnRollContent;
        InitializeContents();
    }

    private void InitializeContents()
    {
        var mainWind = new MainWindowContent();
        mainWind.InitializeMainContent(_changeOnRollContent);
        MainWindowContent.Content = mainWind;
        
        var rollContent = new RollGame();
        rollContent.InitializeRollGameContent(_changeOnRollContent);
        RollContent.Content = rollContent;
        RollContent.IsVisible = false;
    }

    private void ChangeOnRollContent(bool isActiveRollContent)
    {
        if (isActiveRollContent)
        {
            MainWindowContent.IsVisible = false;
            RollContent.IsVisible = true;
        }
        else
        {
            MainWindowContent.IsVisible = true;
            RollContent.IsVisible = false;
        }
    }
}