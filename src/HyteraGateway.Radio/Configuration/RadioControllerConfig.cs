using System.Xml.Serialization;

namespace HyteraGateway.Radio.Configuration;

/// <summary>
/// Represents RadioController.xml configuration for radio/slot setup
/// </summary>
[XmlRoot("RadioController")]
public class RadioControllerConfig
{
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
}
