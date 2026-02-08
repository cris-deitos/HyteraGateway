using System.Xml.Serialization;

namespace HyteraGateway.Radio.Configuration;

/// <summary>
/// Represents RadioController.xml configuration for radio/slot setup
/// </summary>
[XmlRoot("RadioController")]
public class RadioControllerConfig
{
    /// <summary>
    /// Radio name
    /// </summary>
    [XmlAttribute("radioName")]
    public string? RadioName { get; set; }

    /// <summary>
    /// Radio ID
    /// </summary>
    [XmlAttribute("radioId")]
    public int RadioId { get; set; }

    /// <summary>
    /// IP address of the radio
    /// </summary>
    [XmlAttribute("RadioIpAddress")]
    public string? RadioIpAddress { get; set; }
    
    /// <summary>
    /// Port number for radio communication
    /// </summary>
    [XmlAttribute("RadioPort")]
    public int RadioPort { get; set; } = 50000;
    
    /// <summary>
    /// Dispatcher identifier
    /// </summary>
    [XmlAttribute("DispatcherId")]
    public uint DispatcherId { get; set; }
    
    /// <summary>
    /// PTT timeout in seconds
    /// </summary>
    [XmlAttribute("PttTimeoutSeconds")]
    public int PttTimeoutSeconds { get; set; } = 180;
    
    /// <summary>
    /// Indicates whether VoIP is enabled
    /// </summary>
    [XmlAttribute("VoipEnabled")]
    public bool VoipEnabled { get; set; } = false;
    
    /// <summary>
    /// Indicates whether VRS is enabled
    /// </summary>
    [XmlAttribute("VrsEnabled")]
    public bool VrsEnabled { get; set; } = false;
    
    /// <summary>
    /// Indicates whether activity check is enabled
    /// </summary>
    [XmlAttribute("ActivityCheckEnabled")]
    public bool ActivityCheckEnabled { get; set; } = false;
    
    /// <summary>
    /// Activity check interval in minutes
    /// </summary>
    [XmlAttribute("ActivityCheckMinutes")]
    public int ActivityCheckMinutes { get; set; } = 60;
    
    /// <summary>
    /// Indicates whether position check is enabled
    /// </summary>
    [XmlAttribute("PositionCheckEnabled")]
    public bool PositionCheckEnabled { get; set; } = false;
    
    /// <summary>
    /// Position check interval in minutes
    /// </summary>
    [XmlAttribute("PositionCheckMinutes")]
    public int PositionCheckMinutes { get; set; } = 30;
    
    /// <summary>
    /// Protocols configuration
    /// </summary>
    [XmlArray("protocols")]
    [XmlArrayItem("UDPComProtocol")]
    public List<UDPComProtocol> Protocols { get; set; } = new();

    [XmlArray("Radios")]
    [XmlArrayItem("Radio")]
    public List<RadioConfig> Radios { get; set; } = new();
    
    [XmlArray("Slots")]
    [XmlArrayItem("Slot")]
    public List<SlotConfig> Slots { get; set; } = new();
}

/// <summary>
/// Configuration for a radio
/// </summary>
public class RadioConfig
{
    [XmlAttribute("DmrId")]
    public int DmrId { get; set; }
    
    [XmlAttribute("Name")]
    public string Name { get; set; } = "";
    
    [XmlAttribute("IpAddress")]
    public string? IpAddress { get; set; }
    
    [XmlAttribute("Port")]
    public int Port { get; set; } = 50000;
    
    [XmlAttribute("Enabled")]
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Configuration for a slot (talkgroup)
/// </summary>
public class SlotConfig
{
    [XmlAttribute("Number")]
    public int Number { get; set; }
    
    [XmlAttribute("Name")]
    public string Name { get; set; } = "";
    
    [XmlAttribute("TalkGroupId")]
    public int TalkGroupId { get; set; }
    
    [XmlAttribute("IsDefault")]
    public bool IsDefault { get; set; }
    
    [XmlAttribute("PttEnabled")]
    public bool PttEnabled { get; set; } = true;
    
    [XmlAttribute("Visible")]
    public bool Visible { get; set; } = true;
}

/// <summary>
/// UDP communication protocol configuration
/// </summary>
public class UDPComProtocol
{
    /// <summary>
    /// Path to the protocol DLL
    /// </summary>
    [XmlAttribute("path")]
    public string? Path { get; set; }

    /// <summary>
    /// Protocol class name
    /// </summary>
    [XmlAttribute("class")]
    public string? Class { get; set; }

    /// <summary>
    /// Device name
    /// </summary>
    [XmlAttribute("deviceName")]
    public string? DeviceName { get; set; }

    /// <summary>
    /// UDP network services (ports)
    /// </summary>
    [XmlArray("UDPNetworkServices")]
    [XmlArrayItem("UDPService")]
    public List<UDPService> UDPNetworkServices { get; set; } = new();

    /// <summary>
    /// Enabled channel rules (RX events)
    /// </summary>
    [XmlArray("channelRulesEnabled")]
    public List<ChannelRule> ChannelRulesEnabled { get; set; } = new();
}

/// <summary>
/// UDP service configuration (port definition)
/// </summary>
public class UDPService
{
    /// <summary>
    /// Service name (RRS, LP, TMP, RCP, TP, DTP, SDMP)
    /// </summary>
    [XmlAttribute("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Host IP address
    /// </summary>
    [XmlAttribute("host")]
    public string Host { get; set; } = "192.168.10.2";

    /// <summary>
    /// Port number
    /// </summary>
    [XmlAttribute("port")]
    public int Port { get; set; }

    /// <summary>
    /// Whether multicast is enabled
    /// </summary>
    [XmlAttribute("multicast")]
    public bool Multicast { get; set; }
}

/// <summary>
/// Channel rule (event type)
/// </summary>
public class ChannelRule
{
    /// <summary>
    /// Rule name (event type like RX_CALL, RX_GPS_SENTENCE, etc.)
    /// </summary>
    [XmlText]
    public string? Name { get; set; }
}

/// <summary>
/// Utility class for loading and saving RadioController configuration
/// </summary>
public static class RadioControllerConfigLoader
{
    /// <summary>
    /// Loads configuration from an XML file
    /// </summary>
    public static RadioControllerConfig Load(string filePath)
    {
        var serializer = new XmlSerializer(typeof(RadioControllerConfig));
        using var reader = new StreamReader(filePath);
        return (RadioControllerConfig)serializer.Deserialize(reader)!;
    }
    
    /// <summary>
    /// Saves configuration to an XML file
    /// </summary>
    public static void Save(RadioControllerConfig config, string filePath)
    {
        var serializer = new XmlSerializer(typeof(RadioControllerConfig));
        using var writer = new StreamWriter(filePath);
        serializer.Serialize(writer, config);
    }

    /// <summary>
    /// Tries to load configuration, returns default if file not found
    /// </summary>
    public static RadioControllerConfig LoadOrDefault(string filePath)
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

        return new RadioControllerConfig();
    }
}
