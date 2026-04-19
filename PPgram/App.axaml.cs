using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using PPgram.Application;
using PPgram.Config;
using PPgram.Logging;
using PPgram.State;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PPgram;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("windows")]
public partial class App : Avalonia.Application
{
    private readonly CancellationTokenSource _cts = new();
    private AppLifecycle? _appLifecycle;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var config = AppConfig.LoadOrCreate();
        var background = new BackgroundState(config, _cts.Token);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            _appLifecycle = new AppLifecycle(desktop, _cts, background);
            _appLifecycle.Initialize();
        }

        _ = InitializeBackgroundAsync(background);

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitializeBackgroundAsync(BackgroundState background)
    {
        try
        {
            await background.InitializeAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            AppLog.Info("App", "Background initialization canceled.");
        }
        catch (Exception ex)
        {
            AppLog.Error("App", "Background initialization failed.", ex);
        }
    }
}
