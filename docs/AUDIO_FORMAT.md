# Audio Format Configuration

## Important: PCM Audio Format

According to the `Hytera_HyteraProtocol.xml` configuration file:

```xml
<RTPAudioOutputType>PCM</RTPAudioOutputType>
```

This indicates that **audio is already in PCM format** when received from the Hytera protocol.

### What This Means

- **No AMBE/AMBE+ codec is required**: The audio data does not need decoding from AMBE format
- **Direct PCM processing**: Audio can be used directly as 16-bit PCM samples
- **No mbelib dependency**: The mbelib library is not needed for Hytera protocol

### Audio Configuration Parameters

From `Hytera_HyteraProtocol.xml`:

| Parameter | Value | Description |
|-----------|-------|-------------|
| `RTPAudioOutputType` | PCM | Audio format (already decoded) |
| `AudioRxTimeoutMs` | 55 | RX audio timeout in milliseconds |
| `AudioTxTimeoutMs` | 55 | TX audio timeout in milliseconds |
| `RxVolume` | 500 | Receive volume (0-1000) |
| `TxVolume` | 100 | Transmit volume (0-1000) |
| `RtpBufferSize` | 10 | RTP buffer size |
| `RtpTimeoutMs` | 500 | RTP timeout in milliseconds |

### Audio Specifications

Based on typical Hytera configurations:
- **Sample Rate**: 8 kHz
- **Bit Depth**: 16-bit
- **Channels**: Mono
- **Format**: Linear PCM

### Usage in Code

Check if audio is PCM format:

```csharp
var config = HyteraProtocolConfigLoader.Load("Hytera_HyteraProtocol.xml");

if (config.IsPcmAudio)
{
    // Process audio directly as PCM
    // No AMBE decoding needed
}
```

### Audio Processing Libraries

Since audio is already PCM, you can use:
- **NAudio**: For audio playback and recording (already in project)
- **Alvas.Audio.dll**: Vendor audio library (available in lib/vendor/)
- Any standard PCM audio processing library

### Migration from AMBE

If previous code used AMBE codec:
1. Remove mbelib dependencies
2. Remove AMBE decoding/encoding steps
3. Process audio directly as PCM samples
4. Use vendor DLLs (Alvas.Audio.dll) if needed
