using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using PPgram.Logging;
using PPgram.Network;

namespace PPgram.State;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("windows")]
public sealed class BackgroundState : IAsyncDisposable
{
    private readonly CancellationTokenSource _globalCancellation = new();
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public BackgroundState()
    {
        Client = new PPClient(new PPTransport("127.0.0.1", 4433));
    }

    public PPClient Client { get; }
    public CancellationToken GlobalCancellationToken => _globalCancellation.Token;

    public void RequestCancellation()
    {
        _globalCancellation.Cancel();
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_loopTask is not null)
        {
            AppLog.Info("BackgroundState", "Start skipped: loop already running.");
            return Task.CompletedTask;
        }

        AppLog.Info("BackgroundState", "Starting background loop.");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(_globalCancellation.Token, cancellationToken);
        _loopTask = Task.Run(() => RunAsync(_cts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(TimeSpan timeout)
    {
        AppLog.Info("BackgroundState", $"Stop requested (timeout={timeout}).");
        RequestCancellation();

        if (_loopTask is not null)
        {
            try
            {
                await _loopTask.WaitAsync(timeout);
            }
            catch (OperationCanceledException)
            {
                AppLog.Info("BackgroundState", "Background loop canceled.");
            }
            catch (TimeoutException)
            {
                AppLog.Error("BackgroundState", "Timed out waiting for background loop to stop.");
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _loopTask = null;
            }
        }

        try
        {
            using var closeCts = new CancellationTokenSource(timeout);
            await Client.DisconnectAsync(cancellationToken: closeCts.Token);
        }
        catch (Exception ex)
        {
            AppLog.Error("BackgroundState", "Client disconnect failed during stop.", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(TimeSpan.FromSeconds(2));
        _globalCancellation.Dispose();
        await Client.DisposeAsync();
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Client.ConnectAsync(cancellationToken);

                // Keep transport hot while UI can come and go.
                await Client.PingAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                AppLog.Info("BackgroundState", "Run loop canceled.");
                break;
            }
            catch (Exception ex)
            {
                AppLog.Error("BackgroundState", "Background iteration failed; reconnecting.", ex);
                try
                {
                    await Client.DisconnectAsync(cancellationToken: cancellationToken);
                }
                catch (Exception disconnectEx)
                {
                    AppLog.Error("BackgroundState", "Disconnect after failure also failed.", disconnectEx);
                }

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }
    }
}
