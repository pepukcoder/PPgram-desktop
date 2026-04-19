using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using PPgram.State;
using PPgram.ViewModels;
using PPgram.Views;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace PPgram.Application;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("windows")]
public sealed class ShellController(IClassicDesktopStyleApplicationLifetime desktop, BackgroundState background) : IDisposable
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop = desktop;
    private readonly BackgroundState _background = background;
    private TrayIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private bool _isDisposing;

    public event Func<Task>? ExitRequested;

    public void Initialize()
    {
        _trayIcon = BuildTrayIcon();
        _mainWindow = BuildMainWindow();
        _desktop.MainWindow = _mainWindow;
        ShowMainWindow();
    }

    public void Dispose()
    {
        if (_isDisposing) return;
        _isDisposing = true;

        if (_mainWindow is not null)
        {
            _mainWindow.Closing -= OnMainWindowClosing;
            _mainWindow.Close();
            _mainWindow = null;
        }

        _trayIcon?.Dispose();
        _trayIcon = null;
    }

    private MainWindow BuildMainWindow()
    {
        var window = new MainWindow
        {
            DataContext = new MainWindowViewModel(_background),
        };
        window.Closing += OnMainWindowClosing;
        return window;
    }

    private TrayIcon BuildTrayIcon()
    {
        var menu = new NativeMenu();

        var openItem = new NativeMenuItem("Open");
        openItem.Click += (_, _) => ShowMainWindow();
        menu.Items.Add(openItem);

        menu.Items.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += async (_, _) => await RequestExitAsync();
        menu.Items.Add(exitItem);

        return new TrayIcon
        {
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://PPgram/Assets/icon.ico"))),
            ToolTipText = "PPgram",
            Menu = menu,
            IsVisible = true,
        };
    }

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_isDisposing) return;
        if (_mainWindow is not null && _mainWindow.IsVisible) _mainWindow.Hide();
        e.Cancel = true;
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            _mainWindow = BuildMainWindow();
            _desktop.MainWindow = _mainWindow;
        }

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private async Task RequestExitAsync()
    {
        var handler = ExitRequested;
        if (handler is null) return;
        await handler();
    }
}
