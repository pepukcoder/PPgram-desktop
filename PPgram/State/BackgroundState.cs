using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using PPgram.Config;
using PPgram.Logging;
using PPgram.Network;

namespace PPgram.State;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("windows")]
public sealed class BackgroundState(AppConfig config, CancellationToken appCt) : IAsyncDisposable
{
    private readonly Lock _lock = new();
    private readonly CancellationToken _appCt = appCt;
    private bool _initialized;
    private bool _disposed;

    public PPClient Client { get; } = new PPClient(new PPTransport(
            config.Host,
            config.Port,
            config.Alpn,
            config.ServerName,
            config.InsecureSkipCertificateValidation));

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        using (_lock.EnterScope())
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_initialized)
            {
                AppLog.Info("BackgroundState", "Initialize skipped: already initialized");
                return;
            }
        }

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_appCt, ct);
        await Client.ConnectAsync(linkedCts.Token);

        using (_lock.EnterScope())
        {
            if (_disposed) return;
            _initialized = true;
        }

        AppLog.Info("BackgroundState", "Initialized");
    }

    public async ValueTask DisposeAsync()
    {
        using (_lock.EnterScope())
        {
            if (_disposed) return;
            _disposed = true;
        }

        try
        {
            using var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await Client.DisconnectAsync(cancellationToken: closeCts.Token);
        }
        catch (Exception ex)
        {
            AppLog.Error("BackgroundState", "Client disconnect failed during dispose", ex);
        }

        await Client.DisposeAsync();
    }
}
