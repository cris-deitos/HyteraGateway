using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using HyteraGateway.Core.Models;
using HyteraGateway.UI.Models;

namespace HyteraGateway.UI.Services;

public class SignalRService
{
    private HubConnection? _hubConnection;
    
    public event EventHandler<RadioEvent>? RadioEventReceived;
    public event EventHandler<LogEntry>? LogReceived;

    public async Task ConnectAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/hubs/radio-events")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<RadioEvent>("RadioEventReceived", evt =>
        {
            RadioEventReceived?.Invoke(this, evt);
        });

        _hubConnection.On<LogEntry>("LogMessage", log =>
        {
            LogReceived?.Invoke(this, log);
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            // Server is not available - continue without SignalR
            System.Diagnostics.Debug.WriteLine($"Failed to connect to SignalR hub: {ex.Message}");
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}
