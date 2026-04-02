using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using PPgram.Logging;

namespace PPgram.Network;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("windows")]
public sealed class PPClient(PPTransport transport) : IAsyncDisposable
{
    private readonly PPTransport _transport = transport;

    public bool IsConnected => _transport.IsConnected;

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        return _transport.ConnectAsync(cancellationToken);
    }

    public Task DisconnectAsync(long errorCode = 0, CancellationToken cancellationToken = default)
    {
        AppLog.Info("PPClient", $"Disconnect requested (errorCode={errorCode}).");
        return _transport.DisconnectAsync(errorCode, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _transport.DisposeAsync();
    }

    public async Task<string> PingAsync(CancellationToken cancellationToken = default)
    {
        var (envelope, _) = await _transport.RequestAsync<Google.Protobuf.WellKnownTypes.Empty, Google.Protobuf.WellKnownTypes.Empty>(
            "ping",
            request: null,
            cancellationToken);

        return envelope.Message;
    }
}
