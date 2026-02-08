using HyteraGateway.Radio.Configuration;
using HyteraGateway.Radio.Protocol.Hytera;
using HyteraGateway.Radio.Vendor;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Radio.Examples;

/// <summary>
/// Example demonstrating how to use the Hytera REAL protocol configuration and vendor DLLs
/// </summary>
public class HyteraProtocolExample
{
    private readonly ILogger<HyteraProtocolExample> _logger;

    public HyteraProtocolExample(ILogger<HyteraProtocolExample> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Example: Load and display configuration from XML files
    /// </summary>
    public void LoadConfigurationExample()
    {
        _logger.LogInformation("=== Loading Hytera Configuration ===");

        // Load protocol configuration
        var protocolConfig = HyteraProtocolConfigLoader.LoadOrDefault("vendor/Hytera_HyteraProtocol.xml");
        
        _logger.LogInformation("Audio Configuration:");
        _logger.LogInformation("  - RTP Audio Output Type: {Type}", protocolConfig.RTPAudioOutputType);
        _logger.LogInformation("  - Is PCM Audio: {IsPcm} (no AMBE codec needed!)", protocolConfig.IsPcmAudio);
        _logger.LogInformation("  - RX Volume: {Volume}", protocolConfig.RxVolume);
        _logger.LogInformation("  - TX Volume: {Volume}", protocolConfig.TxVolume);
        _logger.LogInformation("  - Audio RX Timeout: {Timeout}ms", protocolConfig.AudioRxTimeoutMs);
        _logger.LogInformation("  - Audio TX Timeout: {Timeout}ms", protocolConfig.AudioTxTimeoutMs);

        // Load radio controller configuration
        var radioConfig = RadioControllerConfigLoader.LoadOrDefault("vendor/RadioController.xml");
        
        _logger.LogInformation("\nUDP Service Ports:");
        if (radioConfig.Protocols.Count > 0)
        {
            var protocol = radioConfig.Protocols[0];
            foreach (var service in protocol.UDPNetworkServices)
            {
                _logger.LogInformation("  - {Name}: {Host}:{Port}", 
                    service.Name, service.Host, service.Port);
            }
        }
    }

    /// <summary>
    /// Example: Use port constants
    /// </summary>
    public void UsePortConstantsExample()
    {
        _logger.LogInformation("=== UDP Service Ports ===");
        _logger.LogInformation("RRS (Radio Registration Service): {Port}", HyteraUdpPorts.RRS);
        _logger.LogInformation("LP (Location Protocol - GPS): {Port}", HyteraUdpPorts.LP);
        _logger.LogInformation("TMP (Text Message Protocol): {Port}", HyteraUdpPorts.TMP);
        _logger.LogInformation("RCP (Radio Control Protocol): {Port}", HyteraUdpPorts.RCP);
        _logger.LogInformation("TP (Telemetry Protocol): {Port}", HyteraUdpPorts.TP);
        _logger.LogInformation("DTP (Data Transfer Protocol): {Port}", HyteraUdpPorts.DTP);
        _logger.LogInformation("SDMP (Status/Display Message Protocol): {Port}", HyteraUdpPorts.SDMP);
    }

    /// <summary>
    /// Example: Use event type constants
    /// </summary>
    public void UseEventTypesExample()
    {
        _logger.LogInformation("=== Event Types ===");
        
        _logger.LogInformation("RX Events (Reception):");
        _logger.LogInformation("  - {Event}: Call received", HyteraEventType.RX_CALL);
        _logger.LogInformation("  - {Event}: GPS data received", HyteraEventType.RX_GPS_SENTENCE);
        _logger.LogInformation("  - {Event}: Text message received", HyteraEventType.RX_FREE_TEXT_MESSAGE);
        _logger.LogInformation("  - {Event}: Radio powered on", HyteraEventType.RX_RADIO_ON);
        
        _logger.LogInformation("\nTX Events (Transmission):");
        _logger.LogInformation("  - {Event}: Initiate call", HyteraEventType.TX_CALL);
        _logger.LogInformation("  - {Event}: Request GPS", HyteraEventType.TX_GPS_QUERY);
        _logger.LogInformation("  - {Event}: Send text message", HyteraEventType.TX_FREE_TEXT_MESSAGE);
        _logger.LogInformation("  - {Event}: Radio check", HyteraEventType.TX_RADIO_CHECK);
    }

    /// <summary>
    /// Example: Load vendor DLLs
    /// </summary>
    public void LoadVendorDllsExample()
    {
        _logger.LogInformation("=== Loading Vendor DLLs ===");

        // Create wrapper without logger (null is acceptable)
        var dllWrapper = new VendorDllWrapper("vendor", null);
        
        if (dllWrapper.LoadDlls())
        {
            _logger.LogInformation("✓ Vendor DLLs loaded successfully!");
            
            if (dllWrapper.HyteraProtocolAssembly != null)
            {
                var version = dllWrapper.HyteraProtocolAssembly.GetName().Version;
                _logger.LogInformation("  - HyteraProtocol.dll version: {Version}", version);
            }
            
            if (dllWrapper.RadioControllerAssembly != null)
            {
                var version = dllWrapper.RadioControllerAssembly.GetName().Version;
                _logger.LogInformation("  - BPGRadioController.dll version: {Version}", version);
            }
            
            if (dllWrapper.AlvasAudioAssembly != null)
            {
                var version = dllWrapper.AlvasAudioAssembly.GetName().Version;
                _logger.LogInformation("  - Alvas.Audio.dll version: {Version}", version);
            }
        }
        else
        {
            _logger.LogWarning("⚠ Vendor DLLs not available - running in compatibility mode");
        }
    }

    /// <summary>
    /// Example: Check if PCM audio is configured
    /// </summary>
    public void CheckAudioFormatExample()
    {
        _logger.LogInformation("=== Audio Format Check ===");

        var config = HyteraProtocolConfigLoader.LoadOrDefault("vendor/Hytera_HyteraProtocol.xml");
        
        if (config.IsPcmAudio)
        {
            _logger.LogInformation("✓ Audio is in PCM format");
            _logger.LogInformation("  → No AMBE codec is required");
            _logger.LogInformation("  → No mbelib dependency needed");
            _logger.LogInformation("  → Can use standard PCM audio processing");
            _logger.LogInformation("  → Audio specs: 8kHz, 16-bit, Mono");
        }
        else
        {
            _logger.LogWarning("⚠ Audio is in {Format} format - codec required", 
                config.RTPAudioOutputType);
        }
    }

    /// <summary>
    /// Run all examples
    /// </summary>
    public void RunAllExamples()
    {
        LoadConfigurationExample();
        Console.WriteLine();
        
        UsePortConstantsExample();
        Console.WriteLine();
        
        UseEventTypesExample();
        Console.WriteLine();
        
        LoadVendorDllsExample();
        Console.WriteLine();
        
        CheckAudioFormatExample();
    }
}
