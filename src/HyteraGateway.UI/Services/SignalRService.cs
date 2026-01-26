using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using HyteraGateway.Core.Models;

namespace HyteraGateway.UI.Services;

public class SignalRService
{
    private HubConnection? _hubConnection;
    
    public event EventHandler<RadioEvent>? RadioEventReceived;
    public event EventHandler<string>? LogReceived;

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

        _hubConnection.On<string>("LogMessage", log =>
        {
            LogReceived?.Invoke(this, log);
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch
        {
            // Silently fail if server is not available
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
