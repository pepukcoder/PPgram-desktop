using CommunityToolkit.Mvvm.ComponentModel;
using PPgram.State;
using PPgram.ViewModels.Pages;

namespace PPgram.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly BackgroundState _background;

    public MainWindowViewModel(BackgroundState background)
    {
        _background = background;
        ShowLogin();
    }

    [ObservableProperty]
    private object? _currentViewModel;

    [ObservableProperty]
    private object? _modalViewModel;

    [ObservableProperty]
    private object? _notificationsViewModel;

    public bool IsModalVisible => ModalViewModel is not null;

    public bool IsNotificationsVisible => NotificationsViewModel is not null;

    private void ShowLogin()
    {
        CurrentViewModel = new LoginViewModel(_background)
        {
            ShowRegister = ShowRegister,
        };
    }

    private void ShowRegister()
    {
        CurrentViewModel = new RegisterViewModel(_background)
        {
            ShowLogin = ShowLogin,
        };
    }

    partial void OnModalViewModelChanged(object? value)
    {
        OnPropertyChanged(nameof(IsModalVisible));
    }

    partial void OnNotificationsViewModelChanged(object? value)
    {
        OnPropertyChanged(nameof(IsNotificationsVisible));
    }
}
