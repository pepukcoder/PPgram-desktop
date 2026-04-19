using System;
using System.Net.Quic;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using PPgram.Logging;
using PPgram.Protobuf;

namespace PPgram.Network;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("windows")]
public sealed class PPClient(PPTransport transport) : IAsyncDisposable
{
    private readonly PPTransport _transport = transport;
    private readonly SemaphoreSlim _reconnectGate = new(1, 1);
    private bool _disposed;

    public bool IsConnected => _transport.IsConnected;

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (IsConnected) return Task.CompletedTask;
        return _transport.ConnectAsync(cancellationToken);
    }

    public Task DisconnectAsync(long errorCode = 0, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        return _transport.DisconnectAsync(errorCode, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await _transport.DisposeAsync();
    }

    public Task<(ResponseEnvelope Envelope, TResponse? Response)> RequestAsync<TRequest, TResponse>(
        string op,
        TRequest? request = null,
        CancellationToken cancellationToken = default)
        where TRequest : class, IMessage
        where TResponse : class, IMessage<TResponse>, new()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return ExecuteWithReconnectAsync(
            async ct => await _transport.RequestAsync<TRequest, TResponse>(op, request, ct),
            op,
            cancellationToken);
    }

    private async Task<T> ExecuteWithReconnectAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string op,
        CancellationToken cancellationToken)
    {
        await ConnectAsync(cancellationToken);

        try
        {
            return await operation(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (IsConnectionFailure(ex))
        {
            AppLog.Info("PPClient", $"Request '{op}' hit a connection closure; reconnecting and retrying once.");
            await ReconnectAsync(cancellationToken);
            return await operation(cancellationToken);
        }
    }

    private async Task ReconnectAsync(CancellationToken cancellationToken)
    {
        await _reconnectGate.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            await _transport.DisconnectAsync(cancellationToken: cancellationToken);
            await _transport.ConnectAsync(cancellationToken);
        }
        finally
        {
            _reconnectGate.Release();
        }
    }

    private static bool IsConnectionFailure(Exception ex)
    {
        return ex is QuicException { QuicError: QuicError.ConnectionAborted or QuicError.ConnectionTimeout }
            || (ex is InvalidOperationException ioe && ioe.Message.Contains("not connected", StringComparison.OrdinalIgnoreCase));
    }
}
