using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPgram.State;

namespace PPgram.ViewModels.Pages;

public partial class LoginViewModel(BackgroundState background) : ViewModelBase
{
    private readonly BackgroundState _background = background;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _usernameError;

    [ObservableProperty]
    private string? _passwordError;

    public bool HasUsernameError => !string.IsNullOrWhiteSpace(UsernameError);
    public bool HasPasswordError => !string.IsNullOrWhiteSpace(PasswordError);

    public required Action ShowRegister { get; init; }

    [RelayCommand]
    private void OpenRegister()
    {
        ShowRegister();
    }

    [RelayCommand]
    private void Login()
    {
        UsernameError = string.IsNullOrWhiteSpace(Username)
            ? "Requred"
            : null;

        PasswordError = string.IsNullOrWhiteSpace(Password)
            ? "Required"
            : null;
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
}
