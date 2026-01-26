using System.Net;
using System.Net.Sockets;
using HyteraGateway.Core.Interfaces;
using HyteraGateway.Radio.Protocol.RadioServer;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Radio.Services;

/// <summary>
/// Implements NETRadioServer-compatible TCP server on port 8000
/// Allows multiple clients (dispatchers) to connect and control radio
/// </summary>
public class RadioServerService : IDisposable
{
    private readonly ILogger<RadioServerService> _logger;
    private readonly IRadioService _radioService;
    private readonly int _port;
    
    private TcpListener? _listener;
    private readonly List<ConnectedClient> _clients = new();
    private CancellationTokenSource? _cts;
    
    public RadioServerService(ILogger<RadioServerService> logger, IRadioService radioService, int port = 8000)
    {
        _logger = logger;
        _radioService = radioService;
        _port = port;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        _logger.LogInformation("RadioServer listening on port {Port}", _port);
        
        _cts = new CancellationTokenSource();
        
        // Accept clients loop
        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    _ = HandleClientAsync(client, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting client");
                }
            }
        }, _cts.Token);
    }
    
    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        
        foreach (var client in _clients.ToList())
        {
            client.TcpClient.Close();
        }
        _clients.Clear();
    }
    
    private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        var client = new ConnectedClient { TcpClient = tcpClient };
        _clients.Add(client);
        
        var stream = tcpClient.GetStream();
        _logger.LogInformation("Client connected: {RemoteEndPoint}", tcpClient.Client.RemoteEndPoint);
        
        try
        {
            while (!cancellationToken.IsCancellationRequested && tcpClient.Connected)
            {
                // Read packet
                var buffer = new byte[4096];
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                
                if (bytesRead == 0)
                    break;
                
                var packet = RadioServerPacket.FromBytes(buffer[..bytesRead]);
                await ProcessClientPacketAsync(client, packet, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Client handler error");
        }
        finally
        {
            _clients.Remove(client);
            tcpClient.Close();
            _logger.LogInformation("Client disconnected");
        }
    }
    
    private async Task ProcessClientPacketAsync(ConnectedClient client, RadioServerPacket packet, CancellationToken ct)
    {
        try
        {
            switch ((RadioServerProtocol.ServerCommand)packet.Command)
            {
                case RadioServerProtocol.ServerCommand.CLIENT_REGISTER:
                    client.ClientId = packet.ClientId;
                    await SendAckAsync(client, packet.Sequence);
                    break;
                
                case RadioServerProtocol.ServerCommand.SEND_PTT:
                    // Parse: [4B TalkGroupId][1B Press]
                    var tgId = BitConverter.ToUInt32(packet.Payload, 0);
                    var press = packet.Payload[4] == 1;
                    await _radioService.SendPttAsync((int)tgId, press, ct);
                    break;
                
                case RadioServerProtocol.ServerCommand.SEND_GPS_REQUEST:
                    var radioId = BitConverter.ToInt32(packet.Payload, 0);
                    await _radioService.RequestGpsAsync(radioId, ct);
                    break;
                
                // Additional commands can be handled here...
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing client packet");
        }
    }
    
    private async Task SendAckAsync(ConnectedClient client, ushort sequence)
    {
        var ack = new RadioServerPacket
        {
            Command = (ushort)RadioServerProtocol.ServerCommand.CLIENT_REGISTER_ACK,
            ClientId = client.ClientId,
            Sequence = sequence
        };
        
        var stream = client.TcpClient.GetStream();
        await stream.WriteAsync(ack.ToBytes());
    }
    
    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
    
    private class ConnectedClient
    {
        public TcpClient TcpClient { get; set; } = null!;
        public uint ClientId { get; set; }
    }
}
