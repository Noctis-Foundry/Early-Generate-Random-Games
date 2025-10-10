using System;
using Avalonia.Controls;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.Scr.WindowScr;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class MainWindow : Window
{
    private readonly Register<string, UserControl> _userControlRegister = new();
    private readonly Action<string> _changeContent;
    public MainWindow()
    {
        InitializeComponent();
        
        var vm = new MainWindowViewModel(new WindowService(this));
        DataContext = vm;
        
        _changeContent = Navigate;
        
        InitializeUserControlRegister();
        Navigate("Main");
        
        Closing += MainWindow_OnClosed;
    }

    private void InitializeUserControlRegister()
    {
        var mainContent = new MainWindowContent();
        mainContent.AddListener(_changeContent);

        var rollContent = new RollGame();
        rollContent.AddListener(_changeContent);

        var profileContent = new ProfileContent();
        profileContent.AddListener(_changeContent);
        
        var tableContent = new GameTable();
        tableContent.AddListener(_changeContent);
        
        var rulesContent = new Rules();
        rulesContent.AddListener(_changeContent);
        
        _userControlRegister.RegisterNewObject("Main", mainContent);
        _userControlRegister.RegisterNewObject("Roll", rollContent);
        _userControlRegister.RegisterNewObject("Profile", profileContent);
        _userControlRegister.RegisterNewObject("Table", tableContent);
        _userControlRegister.RegisterNewObject("Rules", rulesContent);
    }
    
    private void Navigate(string nameControl)
    {
        ControlMain.Content = _userControlRegister.GetObjectFromRegister(nameControl);
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        if (SteamManager.Instance != null) 
            SteamManager.Instance.ShutdownSteam();
    }
}