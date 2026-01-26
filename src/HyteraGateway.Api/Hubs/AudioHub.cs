using Microsoft.AspNetCore.SignalR;
using HyteraGateway.Audio.Streaming;
using HyteraGateway.Audio.Processing;

namespace HyteraGateway.Api.Hubs;

/// <summary>
/// SignalR hub for real-time audio streaming between radio and web clients
/// </summary>
public class AudioHub : Hub
{
    private readonly ILogger<AudioHub> _logger;
    private readonly AudioStreamManager _streamManager;
    private static readonly Dictionary<string, AudioSubscription> _subscriptions = new();
    
    public AudioHub(ILogger<AudioHub> logger, AudioStreamManager streamManager)
    {
        _logger = logger;
        _streamManager = streamManager;
    }
    
    /// <summary>
    /// Subscribe to audio from a specific radio or TalkGroup
    /// </summary>
    public async Task SubscribeToAudio(int? radioId = null, int? talkGroupId = null)
    {
        var subscription = new AudioSubscription
        {
            ConnectionId = Context.ConnectionId,
            RadioId = radioId,
            TalkGroupId = talkGroupId
        };
        
        _subscriptions[Context.ConnectionId] = subscription;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(radioId, talkGroupId));
        
        _logger.LogInformation("Client {ConnectionId} subscribed to Radio={RadioId}, TG={TalkGroupId}", 
            Context.ConnectionId, radioId, talkGroupId);
    }
    
    /// <summary>
    /// Unsubscribe from audio
    /// </summary>
    public async Task UnsubscribeFromAudio()
    {
        if (_subscriptions.TryGetValue(Context.ConnectionId, out var subscription))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, 
                GetGroupName(subscription.RadioId, subscription.TalkGroupId));
            _subscriptions.Remove(Context.ConnectionId);
        }
    }
    
    /// <summary>
    /// Send audio from web client to radio (PTT transmit)
    /// </summary>
    public async Task SendAudioToRadio(byte[] opusData, int targetRadioId)
    {
        try
        {
            // Decode Opus to PCM
            byte[] pcm48kHz = _streamManager.DecodeOpusToPcm(opusData);
            
            // Resample to 8kHz
            byte[] pcm8kHz = AudioPipeline.Resample(pcm48kHz, 8000);
            
            // TODO: Encode to AMBE and send to radio
            // For now, just log
            _logger.LogDebug("Received {Bytes} bytes of audio for radio {RadioId}", opusData.Length, targetRadioId);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process audio from client");
        }
    }
    
    /// <summary>
    /// Broadcast audio packet to subscribed clients
    /// </summary>
    public async Task BroadcastAudioPacket(AudioPacket packet, int radioId, int talkGroupId)
    {
        try
        {
            // Encode to Opus for web
            byte[] opusData = packet.Codec == AudioCodec.PCM 
                ? _streamManager.EncodePcmToOpus(packet.Data)
                : packet.Data;
            
            // Send to subscribed group
            await Clients.Group(GetGroupName(radioId, talkGroupId))
                .SendAsync("ReceiveAudio", opusData, radioId, talkGroupId, packet.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast audio packet");
        }
    }
    
    private string GetGroupName(int? radioId, int? talkGroupId)
    {
        if (radioId.HasValue)
            return $"radio-{radioId}";
        if (talkGroupId.HasValue)
            return $"talkgroup-{talkGroupId}";
        return "all";
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _subscriptions.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

public class AudioSubscription
{
    public string ConnectionId { get; set; } = "";
    public int? RadioId { get; set; }
    public int? TalkGroupId { get; set; }
}
