using Microsoft.AspNetCore.SignalR;

namespace HyteraGateway.Api.Hubs;

/// <summary>
/// SignalR hub for real-time radio events
/// </summary>
public class RadioEventsHub : Hub
{
    private readonly ILogger<RadioEventsHub> _logger;

    /// <summary>
    /// Initializes a new instance of the RadioEventsHub
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public RadioEventsHub(ILogger<RadioEventsHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects
    /// </summary>
    /// <param name="exception">Exception if any</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to events for a specific radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the radio</param>
    public async Task SubscribeToRadio(int dmrId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"radio-{dmrId}");
        _logger.LogInformation("Client {ConnectionId} subscribed to radio {DmrId}", Context.ConnectionId, dmrId);
    }

    /// <summary>
    /// Unsubscribe from events for a specific radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the radio</param>
    public async Task UnsubscribeFromRadio(int dmrId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"radio-{dmrId}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from radio {DmrId}", Context.ConnectionId, dmrId);
    }
}
