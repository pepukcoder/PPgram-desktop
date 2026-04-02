using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using PPgram.Logging;
using PPgram.State;
using PPgram.ViewModels;
using PPgram.Views;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PPgram;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("windows")]
public partial class App : Application
{
    private readonly BackgroundState _background = new();
    private IClassicDesktopStyleApplicationLifetime? _desktop;
    private TrayIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private bool _isExiting;
    private bool _exitCleanupStarted;
    private int _backgroundDisposeStarted;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        AppLog.Info("App", "Framework initialization completed; starting background state.");
        _background.StartAsync().GetAwaiter().GetResult();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.Exit += OnDesktopExit;

            _trayIcon = BuildTrayIcon();
            _mainWindow = BuildMainWindow();
            desktop.MainWindow = _mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        AppLog.Info("App", "Desktop exit event.");
        _trayIcon?.Dispose();
        DisposeBackgroundOnce();
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
        exitItem.Click += (_, _) => ExitApplication();
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
        if (_isExiting)
        {
            return;
        }

        e.Cancel = true;
        _mainWindow?.Hide();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            _mainWindow = BuildMainWindow();
            _desktop?.MainWindow = _mainWindow;
        }

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        if (_exitCleanupStarted)
        {
            return;
        }
        _exitCleanupStarted = true;
        _isExiting = true;
        _background.RequestCancellation();
        AppLog.Info("App", "Exit requested.");

        try
        {
            _trayIcon?.IsVisible = false;
            _mainWindow?.Close();
            _desktop?.Shutdown();
        }
        catch (Exception ex)
        {
            AppLog.Error("App", "Error during exit sequence.", ex);
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await _background.StopAsync(TimeSpan.FromSeconds(2));
            }
            catch
            {
                AppLog.Error("App", "Background stop failed during exit.");
            }
            finally
            {
                DisposeBackgroundOnce();
            }
        });

        Environment.ExitCode = 0;
    }

    private void DisposeBackgroundOnce()
    {
        if (Interlocked.Exchange(ref _backgroundDisposeStarted, 1) == 1)
        {
            return;
        }

        try
        {
            _background.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            AppLog.Error("App", "Background dispose failed.", ex);
        }
    }
}
