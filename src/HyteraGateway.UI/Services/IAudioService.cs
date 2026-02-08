using System;
using System.Threading.Tasks;

namespace HyteraGateway.UI.Services;

/// <summary>
/// Provides audio services for real-time audio streaming between the application and radio network.
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Occurs when the audio state changes (connected, disconnected, transmitting, receiving, etc.).
    /// </summary>
    event EventHandler<AudioStateChangedEventArgs>? StateChanged;
    
    /// <summary>
    /// Occurs when the audio input level changes during transmission.
    /// </summary>
    event EventHandler<float>? AudioLevelChanged;
    
    /// <summary>
    /// Gets a value indicating whether the audio service is connected to the audio hub.
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Gets a value indicating whether audio is currently being transmitted.
    /// </summary>
    bool IsTransmitting { get; }
    
    /// <summary>
    /// Gets a value indicating whether audio is currently being received.
    /// </summary>
    bool IsReceiving { get; }
    
    /// <summary>
    /// Gets or sets the audio output volume (0.0 to 1.0+).
    /// </summary>
    float Volume { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether audio output is muted.
    /// </summary>
    bool IsMuted { get; set; }
    
    /// <summary>
    /// Connects to the audio hub asynchronously.
    /// </summary>
    Task ConnectAsync();
    
    /// <summary>
    /// Subscribes to audio from a specific radio asynchronously.
    /// </summary>
    /// <param name="radioId">The ID of the radio to subscribe to.</param>
    Task SubscribeToRadioAsync(int radioId);
    
    /// <summary>
    /// Subscribes to audio from a specific talk group asynchronously.
    /// </summary>
    /// <param name="talkGroupId">The ID of the talk group to subscribe to.</param>
    Task SubscribeToTalkGroupAsync(int talkGroupId);
    
    /// <summary>
    /// Disconnects from the audio hub asynchronously.
    /// </summary>
    Task DisconnectAsync();
    
    /// <summary>
    /// Starts audio playback for received audio.
    /// </summary>
    void StartPlayback();
    
    /// <summary>
    /// Stops audio playback.
    /// </summary>
    void StopPlayback();
    
    /// <summary>
    /// Starts transmitting audio to the specified target radio asynchronously.
    /// </summary>
    /// <param name="targetRadioId">The ID of the target radio to transmit to.</param>
    Task StartTransmitAsync(int targetRadioId);
    
    /// <summary>
    /// Stops transmitting audio.
    /// </summary>
    void StopTransmit();
}
