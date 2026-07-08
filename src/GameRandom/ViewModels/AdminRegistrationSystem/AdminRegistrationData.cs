using CommunityToolkit.Mvvm.Input;
using GameRandom.DbContext;

namespace GameRandom.ViewModels.AdminRegistrationSystem;

/// <summary>
/// Represents data for a single user in the admin registration view.
/// </summary>
/// <param name="userInfo">Information about the user.</param>
/// <param name="buttonText">The text for the action button.</param>
/// <param name="buttonCommand">The command to execute when the button is clicked.</param>
/// <param name="isAdmin">Indicates whether the user is currently an admin.</param>
public class AdminRegistrationData(Users userInfo, string buttonText, AsyncRelayCommand buttonCommand, bool isAdmin)
{
    /// <summary>
    /// Gets information about the user.
    /// </summary>
    public Users UserInfo { get; private set; } = userInfo;

    /// <summary>
    /// Gets the text for the action button.
    /// </summary>
    public string ButtonText { get; private set; } = buttonText;

    /// <summary>
    /// Gets the command to execute when the button is clicked.
    /// </summary>
    public AsyncRelayCommand ButtonCommand { get; private set; } = buttonCommand;

    /// <summary>
    /// Gets a value indicating whether the user is currently an admin.
    /// </summary>
    public bool IsAdmin { get; private set; } = isAdmin;
}