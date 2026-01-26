namespace HyteraGateway.Radio.Protocol.DMR;

/// <summary>
/// Represents a DMR voice frame with AMBE+2 encoded audio data
/// Based on ETSI TS 102 361 standard
/// </summary>
public class DMRVoiceFrame
{
    /// <summary>
    /// AMBE+2 voice data (33 bytes)
    /// Contains compressed voice data at 2450 bps voice + 3150 bps FEC
    /// </summary>
    public byte[] AmbeData { get; set; } = new byte[33];

    /// <summary>
    /// Frame sequence number (0-5 in a superframe)
    /// </summary>
    public byte FrameNumber { get; set; }

    /// <summary>
    /// True if this is the last frame in the voice transmission
    /// </summary>
    public bool IsLastFrame { get; set; }

    /// <summary>
    /// Timestamp when the frame was created or received
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a voice frame from raw AMBE+2 data
    /// </summary>
    /// <param name="ambeData">AMBE+2 encoded data (must be 33 bytes)</param>
    /// <param name="frameNumber">Frame sequence number</param>
    /// <param name="isLastFrame">Whether this is the last frame</param>
    /// <returns>Voice frame object</returns>
    /// <exception cref="ArgumentException">Thrown if AMBE data is not 33 bytes</exception>
    public static DMRVoiceFrame FromAmbeData(byte[] ambeData, byte frameNumber = 0, bool isLastFrame = false)
    {
        if (ambeData.Length != 33)
        {
            throw new ArgumentException("AMBE+2 data must be exactly 33 bytes", nameof(ambeData));
        }

        var frame = new DMRVoiceFrame
        {
            FrameNumber = frameNumber,
            IsLastFrame = isLastFrame
        };
        Array.Copy(ambeData, frame.AmbeData, 33);
        return frame;
    }

    /// <summary>
    /// Gets the AMBE+2 data as a byte array
    /// </summary>
    /// <returns>33-byte AMBE+2 data array</returns>
    public byte[] ToBytes()
    {
        byte[] result = new byte[33];
        Array.Copy(AmbeData, result, 33);
        return result;
    }

    /// <summary>
    /// Creates a new voice frame from a byte array
    /// </summary>
    /// <param name="data">Byte array containing voice frame data (at least 33 bytes)</param>
    /// <returns>Voice frame object</returns>
    /// <exception cref="ArgumentException">Thrown if data is less than 33 bytes</exception>
    public static DMRVoiceFrame FromBytes(byte[] data)
    {
        if (data.Length < 33)
        {
            throw new ArgumentException("Voice frame data must be at least 33 bytes", nameof(data));
        }

        var frame = new DMRVoiceFrame();
        Array.Copy(data, frame.AmbeData, 33);
        return frame;
    }
}
