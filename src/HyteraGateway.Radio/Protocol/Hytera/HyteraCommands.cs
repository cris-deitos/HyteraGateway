namespace HyteraGateway.Radio.Protocol.Hytera;

/// <summary>
/// Hytera IPSC protocol command codes
/// </summary>
public enum HyteraCommand : ushort
{
    // Connection commands
    LOGIN = 0x7000,
    LOGIN_RESPONSE = 0x7001,
    KEEPALIVE = 0x7100,
    KEEPALIVE_RESPONSE = 0x7101,
    DISCONNECT = 0x7200,

    // Call commands
    PTT_PRESS = 0x8001,
    PTT_RELEASE = 0x8002,
    CALL_START = 0x8010,
    CALL_END = 0x8011,
    CALL_ACK = 0x8012,

    // Voice commands
    VOICE_FRAME = 0x8080,
    VOICE_HEADER = 0x8081,
    VOICE_TERMINATOR = 0x8082,

    // GPS commands
    GPS_REQUEST = 0x9001,
    GPS_RESPONSE = 0x9002,
    GPS_TRIGGER = 0x9003,

    // Text messaging commands
    TEXT_MESSAGE_SEND = 0xA001,
    TEXT_MESSAGE_RECEIVE = 0xA002,
    TEXT_MESSAGE_ACK = 0xA003,

    // Emergency commands
    EMERGENCY_DECLARE = 0xF001,
    EMERGENCY_ACK = 0xF002,
    EMERGENCY_CANCEL = 0xF003,

    // Status commands
    STATUS_REQUEST = 0xB001,
    STATUS_RESPONSE = 0xB002,
    RADIO_CHECK = 0xB010,
    RADIO_CHECK_ACK = 0xB011
}
