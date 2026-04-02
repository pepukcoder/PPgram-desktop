using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
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
public sealed class PPTransport(
    string host,
    int port,
    string alpn = "ppproto/1.0",
    string? serverName = null,
    bool insecureSkipCertificateValidation = true) : IAsyncDisposable
{
    private const byte FrameTypeData = 5;
    private const int FrameHeaderSize = 5;

    private readonly string _host = host;
    private readonly int _port = port;
    private readonly string _alpn = alpn;
    private readonly string _serverName = string.IsNullOrWhiteSpace(serverName) ? host : serverName;
    private readonly bool _insecureSkipCertificateValidation = insecureSkipCertificateValidation;

    private QuicConnection? _connection;

    public bool IsConnected => _connection is not null;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is not null)
        {
            AppLog.Info("PPTransport", "Connect skipped: already connected.");
            return;
        }

        var tlsOptions = new SslClientAuthenticationOptions
        {
            ApplicationProtocols = [new SslApplicationProtocol(_alpn)],
            TargetHost = _serverName,
        };

        if (_insecureSkipCertificateValidation)
        {
            tlsOptions.RemoteCertificateValidationCallback = static (_, _, _, _) => true;
        }

        var options = new QuicClientConnectionOptions
        {
            RemoteEndPoint = new DnsEndPoint(_host, _port),
            ClientAuthenticationOptions = tlsOptions,
            DefaultCloseErrorCode = 0,
            DefaultStreamErrorCode = 0,
        };

        AppLog.Info("PPTransport", $"Connecting to {_host}:{_port} (alpn={_alpn}, serverName={_serverName}).");
        _connection = await QuicConnection.ConnectAsync(options, cancellationToken);
        AppLog.Info("PPTransport", "Connected.");
    }

    public async Task DisconnectAsync(long errorCode = 0, CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            AppLog.Info("PPTransport", "Disconnect skipped: no active connection.");
            return;
        }

        AppLog.Info("PPTransport", $"Disconnecting (errorCode={errorCode}).");
        await _connection.CloseAsync(errorCode, cancellationToken);
        await _connection.DisposeAsync();
        _connection = null;
        AppLog.Info("PPTransport", "Disconnected.");
    }

    public async Task<(ResponseEnvelope Envelope, TResponse? Response)> RequestAsync<TRequest, TResponse>(
        string op,
        TRequest? request = null,
        CancellationToken cancellationToken = default)
        where TRequest : class, IMessage
        where TResponse : class, IMessage<TResponse>, new()
    {
        var connection = _connection ?? throw new InvalidOperationException("PPClient is not connected.");
        if (string.IsNullOrWhiteSpace(op))
        {
            throw new ArgumentException("Route op is required.", nameof(op));
        }

        await using var stream = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, cancellationToken);

        var envelope = new RequestEnvelope
        {
            Op = op,
        };

        if (request is not null)
        {
            envelope.Request = ByteString.CopyFrom(request.ToByteArray());
        }

        await WriteFrameAsync(stream, FrameTypeData, envelope.ToByteArray(), cancellationToken);

        var (FrameType, Payload) = await ReadFrameAsync(stream, cancellationToken);
        if (FrameType != FrameTypeData)
        {
            throw new InvalidDataException($"Unexpected frame type: {FrameType}.");
        }

        var responseEnvelope = ResponseEnvelope.Parser.ParseFrom(Payload);

        TResponse? response = null;
        if (responseEnvelope.HasResponse && responseEnvelope.Response.Length > 0)
        {
            response = new TResponse();
            response.MergeFrom(responseEnvelope.Response);
        }

        return (responseEnvelope, response);
    }

    public ValueTask DisposeAsync()
    {
        if (_connection is null)
        {
            return ValueTask.CompletedTask;
        }

        return new ValueTask(DisposeCoreAsync());
    }

    private async Task DisposeCoreAsync()
    {
        var connection = _connection;
        _connection = null;

        if (connection is null)
        {
            return;
        }

        try
        {
            await connection.CloseAsync(0);
        }
        catch (Exception ex)
        {
            AppLog.Error("PPTransport", "Error while closing connection during dispose.", ex);
        }

        await connection.DisposeAsync();
    }

    private static async Task WriteFrameAsync(QuicStream stream, byte frameType, byte[] payload, CancellationToken cancellationToken)
    {
        var header = new byte[FrameHeaderSize];
        header[0] = frameType;
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(1), checked((uint)payload.Length));

        await stream.WriteAsync(header, cancellationToken);
        if (payload.Length == 0)
        {
            return;
        }

        await stream.WriteAsync(payload, cancellationToken);
    }

    private static async Task<(byte FrameType, byte[] Payload)> ReadFrameAsync(QuicStream stream, CancellationToken cancellationToken)
    {
        var header = new byte[FrameHeaderSize];
        await ReadExactlyAsync(stream, header, cancellationToken);

        var length = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(1));
        var payload = new byte[length];

        if (length > 0)
        {
            await ReadExactlyAsync(stream, payload, cancellationToken);
        }

        return (header[0], payload);
    }

    private static async Task ReadExactlyAsync(QuicStream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(read), cancellationToken);
            if (bytesRead == 0)
            {
                throw new EndOfStreamException("Unexpected EOF while reading QUIC frame.");
            }

            read += bytesRead;
        }
    }
}
