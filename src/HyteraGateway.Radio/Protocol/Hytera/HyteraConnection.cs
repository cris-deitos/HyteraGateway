using System.Buffers.Binary;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Radio.Protocol.Hytera;

/// <summary>
/// Manages TCP connection to a Hytera radio using IPSC protocol
/// </summary>
public class HyteraConnection : IDisposable
{
    private readonly ILogger<HyteraConnection>? _logger;
    private readonly string _radioIp;
    private readonly int _port;
    private readonly uint _dispatcherId;
    
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private CancellationTokenSource? _receiveCts;
    private Task? _receiveTask;
    private uint _sequenceNumber;
    private DateTime _lastKeepalive = DateTime.UtcNow;
    private Timer? _keepaliveTimer;

    /// <summary>
    /// Event raised when a packet is received
    /// </summary>
    public event EventHandler<HyteraIPSCPacket>? PacketReceived;

    /// <summary>
    /// Event raised when connection is lost
    /// </summary>
    public event EventHandler? ConnectionLost;

    /// <summary>
    /// Gets whether the connection is currently active
    /// </summary>
    public bool IsConnected => _tcpClient?.Connected ?? false;

    /// <summary>
    /// Initializes a new Hytera connection
    /// </summary>
    /// <param name="radioIp">Radio IP address</param>
    /// <param name="dispatcherId">Dispatcher DMR ID</param>
    /// <param name="port">TCP port (default 50000)</param>
    /// <param name="logger">Logger instance</param>
    public HyteraConnection(string radioIp, uint dispatcherId, int port = 50000, ILogger<HyteraConnection>? logger = null)
    {
        _radioIp = radioIp;
        _port = port;
        _dispatcherId = dispatcherId;
        _logger = logger;
        _sequenceNumber = 0;
    }

    /// <summary>
    /// Connects to the radio and performs login handshake
    /// </summary>
    /// <param name="authData">Optional authentication data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection successful</returns>
    public async Task<bool> ConnectAsync(byte[]? authData = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Connecting to Hytera radio at {RadioIp}:{Port}", _radioIp, _port);

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_radioIp, _port, cancellationToken);
            _stream = _tcpClient.GetStream();

            _logger?.LogDebug("TCP connection established");

            // Send login packet
            var loginPacket = HyteraIPSCPacket.CreateLogin(_dispatcherId, authData ?? Array.Empty<byte>());
            await SendPacketAsync(loginPacket, cancellationToken);

            _logger?.LogDebug("Login packet sent, waiting for response");

            // Wait for login response
            var responseData = await ReceiveDataAsync(cancellationToken);
            if (responseData != null)
            {
                var response = HyteraIPSCPacket.FromBytes(responseData);
                if (response.Command == HyteraCommand.LOGIN_RESPONSE)
                {
                    _logger?.LogInformation("Login successful");

                    // Start background receive loop
                    StartReceiveLoop();

                    // Start keepalive timer (every 10 seconds)
                    _keepaliveTimer = new Timer(SendKeepalive, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

                    return true;
                }
                else
                {
                    _logger?.LogWarning("Unexpected response to login: {Command}", response.Command);
                }
            }

            _logger?.LogError("Login failed - no response received");
            await DisconnectAsync();
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to radio");
            await DisconnectAsync();
            return false;
        }
    }

    /// <summary>
    /// Disconnects from the radio
    /// </summary>
    public async Task DisconnectAsync()
    {
        _logger?.LogInformation("Disconnecting from radio");

        // Stop keepalive timer
        _keepaliveTimer?.Dispose();
        _keepaliveTimer = null;

        // Stop receive loop
        _receiveCts?.Cancel();
        if (_receiveTask != null)
        {
            try
            {
                await _receiveTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        // Send disconnect packet
        if (IsConnected)
        {
            try
            {
                var disconnectPacket = HyteraIPSCPacket.CreateDisconnect(_dispatcherId);
                await SendPacketAsync(disconnectPacket, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Error sending disconnect packet");
            }
        }

        // Close connection
        _stream?.Close();
        _tcpClient?.Close();
        _stream = null;
        _tcpClient = null;

        _logger?.LogInformation("Disconnected from radio");
    }

    /// <summary>
    /// Sends a PTT command
    /// </summary>
    public async Task<bool> SendPttAsync(uint destinationId, bool press, byte slot = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            var packet = press 
                ? HyteraIPSCPacket.CreatePttPress(_dispatcherId, destinationId, slot)
                : HyteraIPSCPacket.CreatePttRelease(_dispatcherId, destinationId, slot);

            await SendPacketAsync(packet, cancellationToken);
            _logger?.LogDebug("PTT {Action} sent to {DestId}", press ? "PRESS" : "RELEASE", destinationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send PTT command");
            return false;
        }
    }

    /// <summary>
    /// Requests GPS position from a radio
    /// </summary>
    public async Task<bool> RequestGpsAsync(uint targetId, CancellationToken cancellationToken = default)
    {
        try
        {
            var packet = HyteraIPSCPacket.CreateGpsRequest(_dispatcherId, targetId);
            await SendPacketAsync(packet, cancellationToken);
            _logger?.LogDebug("GPS request sent to {TargetId}", targetId);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send GPS request");
            return false;
        }
    }

    /// <summary>
    /// Sends a text message to a radio
    /// </summary>
    public async Task<bool> SendTextMessageAsync(uint destinationId, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var packet = HyteraIPSCPacket.CreateTextMessage(_dispatcherId, destinationId, message);
            await SendPacketAsync(packet, cancellationToken);
            _logger?.LogDebug("Text message sent to {DestId}: {Message}", destinationId, message);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send text message");
            return false;
        }
    }

    /// <summary>
    /// Sends a packet to the radio
    /// </summary>
    private async Task SendPacketAsync(HyteraIPSCPacket packet, CancellationToken cancellationToken)
    {
        if (!IsConnected || _stream == null)
        {
            throw new InvalidOperationException("Not connected to radio");
        }

        packet.Sequence = _sequenceNumber++;
        byte[] data = packet.ToBytes();

        _logger?.LogTrace("Sending packet: Command={Command}, Seq={Seq}, Length={Length}", 
            packet.Command, packet.Sequence, data.Length);

        await _stream.WriteAsync(data, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Receives data from the radio
    /// </summary>
    private async Task<byte[]?> ReceiveDataAsync(CancellationToken cancellationToken)
    {
        if (_stream == null)
        {
            return null;
        }

        // Read header first (minimum 19 bytes)
        byte[] header = new byte[8];
        int bytesRead = await _stream.ReadAsync(header, 0, 8, cancellationToken);
        
        if (bytesRead < 8)
        {
            return null;
        }

        // Parse length from header (at offset 6) - little-endian
        ushort length = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6, 2));
        
        // Read full packet
        byte[] fullPacket = new byte[length];
        Array.Copy(header, fullPacket, 8);
        
        int remaining = length - 8;
        int offset = 8;
        
        while (remaining > 0)
        {
            bytesRead = await _stream.ReadAsync(fullPacket, offset, remaining, cancellationToken);
            if (bytesRead == 0)
            {
                return null;
            }
            offset += bytesRead;
            remaining -= bytesRead;
        }

        return fullPacket;
    }

    /// <summary>
    /// Background receive loop
    /// </summary>
    private void StartReceiveLoop()
    {
        _receiveCts = new CancellationTokenSource();
        _receiveTask = Task.Run(async () =>
        {
            _logger?.LogDebug("Receive loop started");
            
            try
            {
                while (!_receiveCts.Token.IsCancellationRequested && IsConnected)
                {
                    try
                    {
                        var data = await ReceiveDataAsync(_receiveCts.Token);
                        if (data != null)
                        {
                            var packet = HyteraIPSCPacket.FromBytes(data);
                            _logger?.LogTrace("Received packet: Command={Command}, Seq={Seq}", 
                                packet.Command, packet.Sequence);

                            // Handle keepalive responses internally
                            if (packet.Command == HyteraCommand.KEEPALIVE_RESPONSE)
                            {
                                _lastKeepalive = DateTime.UtcNow;
                                _logger?.LogTrace("Keepalive acknowledged");
                            }
                            else
                            {
                                // Raise event for other packets
                                PacketReceived?.Invoke(this, packet);
                            }
                        }
                        else
                        {
                            // Connection lost
                            _logger?.LogWarning("Connection lost - no data received");
                            ConnectionLost?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger?.LogError(ex, "Error in receive loop");
                        ConnectionLost?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                }
            }
            finally
            {
                _logger?.LogDebug("Receive loop stopped");
            }
        }, _receiveCts.Token);
    }

    /// <summary>
    /// Sends keepalive packet (timer callback)
    /// </summary>
    private async void SendKeepalive(object? state)
    {
        if (!IsConnected)
        {
            return;
        }

        try
        {
            var packet = HyteraIPSCPacket.CreateKeepalive(_dispatcherId);
            await SendPacketAsync(packet, CancellationToken.None);
            _logger?.LogTrace("Keepalive sent");

            // Check if keepalive response is overdue
            if ((DateTime.UtcNow - _lastKeepalive).TotalSeconds > 30)
            {
                _logger?.LogWarning("Keepalive timeout - no response for 30 seconds");
                ConnectionLost?.Invoke(this, EventArgs.Empty);
                await DisconnectAsync();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send keepalive");
        }
    }

    /// <summary>
    /// Disposes the connection
    /// </summary>
    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        _receiveCts?.Dispose();
        _keepaliveTimer?.Dispose();
    }
}
