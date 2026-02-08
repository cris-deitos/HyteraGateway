namespace HyteraGateway.Radio.Protocol.Hytera;

/// <summary>
/// Hytera UDP service port definitions
/// Based on RadioController.xml configuration
/// </summary>
public static class HyteraUdpPorts
{
    /// <summary>
    /// Radio Registration Service (RRS) - Port 3002
    /// Used for radio registration and keepalive
    /// </summary>
    public const int RRS = 3002;

    /// <summary>
    /// Location Protocol (LP) - Port 3003
    /// Used for GPS location data
    /// </summary>
    public const int LP = 3003;

    /// <summary>
    /// Text Message Protocol (TMP) - Port 3004
    /// Used for sending and receiving text messages
    /// </summary>
    public const int TMP = 3004;

    /// <summary>
    /// Radio Control Protocol (RCP) - Port 3005
    /// Used for radio control commands (PTT, etc.)
    /// </summary>
    public const int RCP = 3005;

    /// <summary>
    /// Telemetry Protocol (TP) - Port 3006
    /// Used for telemetry data
    /// </summary>
    public const int TP = 3006;

    /// <summary>
    /// Data Transfer Protocol (DTP) - Port 3007
    /// Used for data transfer
    /// </summary>
    public const int DTP = 3007;

    /// <summary>
    /// Status/Display Message Protocol (SDMP) - Port 3009
    /// Used for status and display messages
    /// </summary>
    public const int SDMP = 3009;
}

/// <summary>
/// Hytera event types from RadioController.xml
/// </summary>
public static class HyteraEventType
{
    // RX Events (Reception)
    public const string RX_CALL = "RX_CALL";
    public const string RX_CALL_ACK = "RX_CALL_ACK";
    public const string RX_CALL_STARTED = "RX_CALL_STARTED";
    public const string RX_CALL_ENDED = "RX_CALL_ENDED";
    public const string RX_GPS_SENTENCE = "RX_GPS_SENTENCE";
    public const string RX_FREE_TEXT_MESSAGE = "RX_FREE_TEXT_MESSAGE";
    public const string RX_RADIO_ON = "RX_RADIO_ON";
    public const string RX_RADIO_OFF = "RX_RADIO_OFF";
    public const string RX_CHANNEL_STATUS_CHANGED = "RX_CHANNEL_STATUS_CHANGED";
    public const string RX_END_PTT_ID = "RX_END_PTT_ID";

    // TX Events (Transmission)
    public const string TX_CALL = "TX_CALL";
    public const string TX_VOICE_CALL = "TX_VOICE_CALL";
    public const string TX_GPS_QUERY = "TX_GPS_QUERY";
    public const string TX_FREE_TEXT_MESSAGE = "TX_FREE_TEXT_MESSAGE";
    public const string TX_RADIO_CHECK = "TX_RADIO_CHECK";
    public const string TX_REMOTE_MONITOR = "TX_REMOTE_MONITOR";
    public const string TX_RADIO_DISABLE_ON_OFF = "TX_RADIO_DISABLE_ON_OFF";
    public const string TX_PRESS_RELEASE_BUTTON = "TX_PRESS_RELEASE_BUTTON";
    public const string TX_CHANGE_CHANNEL_STATUS = "TX_CHANGE_CHANNEL_STATUS";
    public const string TX_RADIO_SELECTION_TO_CONVERSATION = "TX_RADIO_SELECTION_TO_CONVERSATION";
    public const string TX_SEND_RULE = "TX_SEND_RULE";
}
