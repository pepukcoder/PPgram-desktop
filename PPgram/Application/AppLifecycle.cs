using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PPgram.Logging;
using PPgram.State;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PPgram.Application;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("windows")]
public sealed class AppLifecycle : IAsyncDisposable
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly CancellationTokenSource _appCts;
    private readonly BackgroundState _background;
    private readonly ShellController _shell;
    private readonly Lock _shutdownLock = new();
    private Task? _shutdownTask;
    private bool _shutdownRequestedByLifecycle;

    public AppLifecycle(
        IClassicDesktopStyleApplicationLifetime desktop,
        CancellationTokenSource appCts,
        BackgroundState background)
    {
        _desktop = desktop;
        _appCts = appCts;
        _background = background;
        _shell = new ShellController(desktop, background);
        _shell.ExitRequested += EnsureShutdownAsync;
    }

    public void Initialize()
    {
        _desktop.ShutdownRequested += OnShutdownRequested;
        _desktop.Exit += OnDesktopExit;
        _shell.Initialize();
    }

    public async Task EnsureShutdownAsync()
    {
        Task shutdownTask;
        using (_shutdownLock.EnterScope())
        {
            _shutdownTask ??= ShutdownCoreAsync();
            shutdownTask = _shutdownTask;
        }

        await shutdownTask;
    }

    public async ValueTask DisposeAsync()
    {
        await EnsureShutdownAsync();
    }

    private async Task ShutdownCoreAsync()
    {
        AppLog.Info("App", "Exiting");

        try
        {
            _appCts.Cancel();
            await _background.DisposeAsync();
            _shell.Dispose();

            _desktop.ShutdownRequested -= OnShutdownRequested;
            _desktop.Exit -= OnDesktopExit;

            if (!_shutdownRequestedByLifecycle)
            {
                _shutdownRequestedByLifecycle = true;
                _desktop.Shutdown();
            }
        }
        catch (Exception ex)
        {
            AppLog.Error("App", "Error during exit sequence.", ex);
            throw;
        }
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        if (_shutdownRequestedByLifecycle) return;
        e.Cancel = true;
        _ = EnsureShutdownAsync();
    }

    private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        EnsureShutdownAsync().GetAwaiter().GetResult();
    }
}
