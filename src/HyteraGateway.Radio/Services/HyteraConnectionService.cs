using HyteraGateway.Core.Interfaces;
using HyteraGateway.Core.Models;
using HyteraGateway.Radio.Protocol.Hytera;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace HyteraGateway.Radio.Services;

/// <summary>
/// Service for connecting to and communicating with Hytera radios
/// </summary>
public class HyteraConnectionService : IRadioService, IDisposable
{
    private readonly ILogger<HyteraConnectionService> _logger;
    private HyteraConnection? _connection;
    private string? _radioIp;
    private uint _dispatcherId;

    /// <summary>
    /// Event raised when a radio event occurs
    /// </summary>
    public event EventHandler<RadioEvent>? RadioEvent;

    /// <summary>
    /// Initializes a new instance of the HyteraConnectionService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public HyteraConnectionService(ILogger<HyteraConnectionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Configures the connection parameters
    /// </summary>
    /// <param name="radioIp">Radio IP address</param>
    /// <param name="dispatcherId">Dispatcher DMR ID</param>
    public void Configure(string radioIp, uint dispatcherId)
    {
        _radioIp = radioIp;
        _dispatcherId = dispatcherId;
    }

    /// <inheritdoc/>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_radioIp))
        {
            _logger.LogError("Radio IP not configured. Call Configure() first.");
            return false;
        }

        _logger.LogInformation("Connecting to Hytera radio at {RadioIp}...", _radioIp);

        try
        {
            _connection = new HyteraConnection(_radioIp, _dispatcherId, 50000);
            
            // Subscribe to events
            _connection.PacketReceived += OnPacketReceived;
            _connection.ConnectionLost += OnConnectionLost;

            // Connect with optional authentication
            bool success = await _connection.ConnectAsync(null, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Successfully connected to Hytera radio");
                
                // Raise connected event
                RaiseRadioEvent(new RadioEvent
                {
                    EventType = RadioEventType.Connected,
                    RadioDmrId = (int)_dispatcherId
                });
            }
            else
            {
                _logger.LogError("Failed to connect to Hytera radio");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to Hytera radio");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Disconnecting from Hytera radio...");

        if (_connection != null)
        {
            _connection.PacketReceived -= OnPacketReceived;
            _connection.ConnectionLost -= OnConnectionLost;
            
            await _connection.DisconnectAsync();
            _connection.Dispose();
            _connection = null;

            // Raise disconnected event
            RaiseRadioEvent(new RadioEvent
            {
                EventType = RadioEventType.Disconnected,
                RadioDmrId = (int)_dispatcherId
            });
        }

        _logger.LogInformation("Disconnected from Hytera radio");
    }

    /// <inheritdoc/>
    public async Task<bool> SendPttAsync(int dmrId, bool press, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending PTT command to radio {DmrId}: {Action}", dmrId, press ? "PRESS" : "RELEASE");

        if (_connection == null || !_connection.IsConnected)
        {
            _logger.LogWarning("Cannot send PTT - not connected to radio");
            return false;
        }

        try
        {
            bool success = await _connection.SendPttAsync((uint)dmrId, press, 0, cancellationToken);
            
            if (success)
            {
                _logger.LogDebug("PTT command sent successfully");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending PTT command");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<GpsPosition?> RequestGpsAsync(int dmrId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Requesting GPS position from radio {DmrId}", dmrId);

        if (_connection == null || !_connection.IsConnected)
        {
            _logger.LogWarning("Cannot request GPS - not connected to radio");
            return null;
        }

        try
        {
            await _connection.RequestGpsAsync((uint)dmrId, cancellationToken);
            _logger.LogDebug("GPS request sent");
            
            // Note: Actual GPS response will come via PacketReceived event
            // This is asynchronous - caller should listen to RadioEvent
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting GPS position");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SendTextMessageAsync(int dmrId, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending text message to radio {DmrId}: {Message}", dmrId, message);

        if (_connection == null || !_connection.IsConnected)
        {
            _logger.LogWarning("Cannot send text message - not connected to radio");
            return false;
        }

        try
        {
            bool success = await _connection.SendTextMessageAsync((uint)dmrId, message, cancellationToken);
            
            if (success)
            {
                _logger.LogDebug("Text message sent successfully");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending text message");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<RadioStatus?> GetStatusAsync(int dmrId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting status for radio {DmrId}", dmrId);

        if (_connection == null || !_connection.IsConnected)
        {
            _logger.LogWarning("Cannot get status - not connected to radio");
            return null;
        }

        // Return basic status based on connection state
        await Task.CompletedTask;
        
        return new RadioStatus
        {
            RadioDmrId = dmrId,
            State = _connection.IsConnected ? RadioState.Online : RadioState.Offline,
            LastSeen = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Handles received packets from the radio
    /// </summary>
    private void OnPacketReceived(object? sender, HyteraIPSCPacket packet)
    {
        _logger.LogDebug("Received packet: Command={Command}, Source={SourceId}, Dest={DestId}", 
            packet.Command, packet.SourceId, packet.DestinationId);

        RadioEvent? radioEvent = packet.Command switch
        {
            HyteraCommand.PTT_PRESS => new RadioEvent
            {
                EventType = RadioEventType.PttPressed,
                RadioDmrId = (int)packet.SourceId
            },
            HyteraCommand.PTT_RELEASE => new RadioEvent
            {
                EventType = RadioEventType.PttReleased,
                RadioDmrId = (int)packet.SourceId
            },
            HyteraCommand.CALL_START => new RadioEvent
            {
                EventType = RadioEventType.CallStart,
                RadioDmrId = (int)packet.SourceId
            },
            HyteraCommand.CALL_END => new RadioEvent
            {
                EventType = RadioEventType.CallEnd,
                RadioDmrId = (int)packet.SourceId
            },
            HyteraCommand.GPS_RESPONSE => ParseGpsResponse(packet),
            HyteraCommand.TEXT_MESSAGE_RECEIVE => new RadioEvent
            {
                EventType = RadioEventType.TextMessage,
                RadioDmrId = (int)packet.SourceId,
                Data = Encoding.UTF8.GetString(packet.Payload)
            },
            HyteraCommand.EMERGENCY_DECLARE => new RadioEvent
            {
                EventType = RadioEventType.Emergency,
                RadioDmrId = (int)packet.SourceId
            },
            _ => null
        };

        if (radioEvent != null)
        {
            RaiseRadioEvent(radioEvent);
        }
    }

    /// <summary>
    /// Parses GPS response packet
    /// </summary>
    private RadioEvent? ParseGpsResponse(HyteraIPSCPacket packet)
    {
        try
        {
            if (packet.Payload.Length < 16)
            {
                _logger.LogWarning("GPS response payload too small");
                return null;
            }

            // Parse GPS data (simplified - actual format may vary)
            double latitude = BitConverter.ToDouble(packet.Payload, 0);
            double longitude = BitConverter.ToDouble(packet.Payload, 8);

            var gpsPosition = new GpsPosition
            {
                RadioDmrId = (int)packet.SourceId,
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = DateTime.UtcNow
            };

            return new RadioEvent
            {
                EventType = RadioEventType.GpsPosition,
                RadioDmrId = (int)packet.SourceId,
                Data = JsonSerializer.Serialize(gpsPosition)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing GPS response");
            return null;
        }
    }

    /// <summary>
    /// Handles connection lost event
    /// </summary>
    private void OnConnectionLost(object? sender, EventArgs e)
    {
        _logger.LogWarning("Connection to radio lost");
        
        RaiseRadioEvent(new RadioEvent
        {
            EventType = RadioEventType.Disconnected,
            RadioDmrId = (int)_dispatcherId
        });
    }

    /// <summary>
    /// Raises a radio event
    /// </summary>
    private void RaiseRadioEvent(RadioEvent radioEvent)
    {
        try
        {
            RadioEvent?.Invoke(this, radioEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising radio event");
        }
    }

    /// <summary>
    /// Disposes the service
    /// </summary>
    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }
}
