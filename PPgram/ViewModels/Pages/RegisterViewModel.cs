using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPgram.State;

namespace PPgram.ViewModels.Pages;

public partial class RegisterViewModel(BackgroundState background) : ViewModelBase
{
    private readonly BackgroundState _background = background;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string? _usernameError;

    [ObservableProperty]
    private string? _passwordError;

    [ObservableProperty]
    private string? _confirmPasswordError;

    public bool HasUsernameError => !string.IsNullOrWhiteSpace(UsernameError);
    public bool HasPasswordError => !string.IsNullOrWhiteSpace(PasswordError);
    public bool HasConfirmPasswordError => !string.IsNullOrWhiteSpace(ConfirmPasswordError);

    public required Action ShowLogin { get; init; }

    [RelayCommand]
    private void OpenLogin()
    {
        ShowLogin();
    }

    [RelayCommand]
    private void Register()
    {
        UsernameError = string.IsNullOrWhiteSpace(Username)
            ? "Required"
            : null;

        PasswordError = string.IsNullOrWhiteSpace(Password)
            ? "Required"
            : null;

        ConfirmPasswordError = string.IsNullOrWhiteSpace(ConfirmPassword)
            ? "Required"
            : Password != ConfirmPassword
                ? "Must match"
                : null;

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            // Keep existing behavior scope focused on requested field-level errors.
            return;
        }
    }

    partial void OnUsernameErrorChanged(string? value)
    {
        OnPropertyChanged(nameof(HasUsernameError));
    }

    partial void OnUsernameChanged(string value)
    {
        UsernameError = null;
    }

    partial void OnPasswordErrorChanged(string? value)
    {
        OnPropertyChanged(nameof(HasPasswordError));
    }

    partial void OnPasswordChanged(string value)
    {
        PasswordError = null;
    }

    partial void OnConfirmPasswordErrorChanged(string? value)
    {
        OnPropertyChanged(nameof(HasConfirmPasswordError));
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        ConfirmPasswordError = null;
    }
}
