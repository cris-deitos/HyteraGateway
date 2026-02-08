using System.Xml.Serialization;

namespace HyteraGateway.Radio.Configuration;

/// <summary>
/// Configuration loaded from Hytera_HyteraProtocol.xml
/// </summary>
[XmlRoot("Hytera")]
public class HyteraProtocolConfig
{
    /// <summary>
    /// Interface IP address for the dispatcher
    /// </summary>
    [XmlElement("InterfaceIPAddress")]
    public string InterfaceIPAddress { get; set; } = "192.168.10.1";

    /// <summary>
    /// Base IP address
    /// </summary>
    [XmlElement("BaseIPAddress")]
    public string BaseIPAddress { get; set; } = "12.0.0.100";

    /// <summary>
    /// Ping interval in seconds
    /// </summary>
    [XmlElement("PingInterval")]
    public int PingInterval { get; set; } = 60;

    /// <summary>
    /// RTP buffer size
    /// </summary>
    [XmlElement("RtpBufferSize")]
    public int RtpBufferSize { get; set; } = 10;

    /// <summary>
    /// RTP timeout in milliseconds
    /// </summary>
    [XmlElement("RtpTimeoutMs")]
    public int RtpTimeoutMs { get; set; } = 500;

    /// <summary>
    /// Audio RX timeout in milliseconds
    /// </summary>
    [XmlElement("AudioRxTimeoutMs")]
    public int AudioRxTimeoutMs { get; set; } = 55;

    /// <summary>
    /// Audio TX timeout in milliseconds
    /// </summary>
    [XmlElement("AudioTxTimeoutMs")]
    public int AudioTxTimeoutMs { get; set; } = 55;

    /// <summary>
    /// RX volume (0-1000)
    /// </summary>
    [XmlElement("RxVolume")]
    public int RxVolume { get; set; } = 500;

    /// <summary>
    /// TX volume (0-1000)
    /// </summary>
    [XmlElement("TxVolume")]
    public int TxVolume { get; set; } = 100;

    /// <summary>
    /// RTP audio output type (PCM, AMBE, etc.)
    /// Important: PCM means audio is already decoded, no codec needed!
    /// </summary>
    [XmlElement("RTPAudioOutputType")]
    public string RTPAudioOutputType { get; set; } = "PCM";

    /// <summary>
    /// Whether to allow transmit interrupt
    /// </summary>
    [XmlElement("AllowTransmitInterrupt")]
    public bool AllowTransmitInterrupt { get; set; } = false;

    /// <summary>
    /// Whether to allow sending emergency
    /// </summary>
    [XmlElement("AllowSendEmergency")]
    public bool AllowSendEmergency { get; set; } = false;

    /// <summary>
    /// Connection type
    /// </summary>
    [XmlElement("ConnectionType")]
    public string ConnectionType { get; set; } = "B";

    /// <summary>
    /// Timer configuration
    /// </summary>
    [XmlElement("Timer")]
    public TimerConfig? Timer { get; set; }

    /// <summary>
    /// Status message configuration
    /// </summary>
    [XmlElement("StatusMessage")]
    public StatusMessageConfig? StatusMessage { get; set; }

    /// <summary>
    /// Indicates if audio is PCM format (no AMBE decoding needed)
    /// </summary>
    [XmlIgnore]
    public bool IsPcmAudio => RTPAudioOutputType?.Equals("PCM", StringComparison.OrdinalIgnoreCase) ?? false;
}

/// <summary>
/// Timer configuration
/// </summary>
public class TimerConfig
{
    /// <summary>
    /// RRS first timer in minutes
    /// </summary>
    [XmlElement("RRSFirstTimerInMin")]
    public int RRSFirstTimerInMin { get; set; } = 35;

    /// <summary>
    /// RRS second timer in minutes
    /// </summary>
    [XmlElement("RRSSecondTimerInMin")]
    public int RRSSecondTimerInMin { get; set; } = 2;

    /// <summary>
    /// Timer sleep for packet reading in milliseconds
    /// </summary>
    [XmlElement("TimerSleepLetturaPacchettiInMs")]
    public int TimerSleepLetturaPacchettiInMs { get; set; } = 1;
}

/// <summary>
/// Status message configuration
/// </summary>
public class StatusMessageConfig
{
    /// <summary>
    /// List of status messages
    /// </summary>
    [XmlElement("message")]
    public List<StatusMessage> Messages { get; set; } = new();
}

/// <summary>
/// Individual status message
/// </summary>
public class StatusMessage
{
    /// <summary>
    /// Message code
    /// </summary>
    [XmlAttribute("code")]
    public string Code { get; set; } = "";

    /// <summary>
    /// Whether this is a voice message
    /// </summary>
    [XmlAttribute("voice")]
    public bool Voice { get; set; }

    /// <summary>
    /// Whether this is an emergency message
    /// </summary>
    [XmlAttribute("emergency")]
    public bool Emergency { get; set; }

    /// <summary>
    /// Message text
    /// </summary>
    [XmlText]
    public string Text { get; set; } = "";
}

/// <summary>
/// Utility class for loading Hytera protocol configuration
/// </summary>
public static class HyteraProtocolConfigLoader
{
    /// <summary>
    /// Loads configuration from an XML file
    /// </summary>
    public static HyteraProtocolConfig Load(string filePath)
    {
        var serializer = new XmlSerializer(typeof(HyteraProtocolConfig));
        using var reader = new StreamReader(filePath);
        return (HyteraProtocolConfig)serializer.Deserialize(reader)!;
    }

    /// <summary>
    /// Tries to load configuration, returns default if file not found
    /// </summary>
    public static HyteraProtocolConfig LoadOrDefault(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                return Load(filePath);
            }
        }
        catch
        {
            // Fall through to return default
        }

        return new HyteraProtocolConfig();
    }
}
