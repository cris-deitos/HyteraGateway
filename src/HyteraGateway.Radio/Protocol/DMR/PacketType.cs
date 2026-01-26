namespace HyteraGateway.Radio.Protocol.DMR;

/// <summary>
/// DMR packet types
/// </summary>
public enum PacketType
{
    /// <summary>
    /// Unknown packet type
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Control packet
    /// </summary>
    Control = 1,

    /// <summary>
    /// Voice/audio packet
    /// </summary>
    Voice = 2,

    /// <summary>
    /// Data packet
    /// </summary>
    Data = 3,

    /// <summary>
    /// GPS packet
    /// </summary>
    Gps = 4,

    /// <summary>
    /// Text message packet
    /// </summary>
    TextMessage = 5,

    /// <summary>
    /// Emergency packet
    /// </summary>
    Emergency = 6,

    /// <summary>
    /// Status packet
    /// </summary>
    Status = 7,

    /// <summary>
    /// PTT packet
    /// </summary>
    Ptt = 8
}
