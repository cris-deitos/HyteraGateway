using HyteraGateway.Radio.Configuration;

namespace HyteraGateway.Radio.Tests.Configuration;

public class HyteraProtocolConfigTests
{
    [Fact]
    public void HyteraProtocolConfig_DefaultValues_AreCorrect()
    {
        // Act
        var config = new HyteraProtocolConfig();
        
        // Assert
        Assert.Equal("192.168.10.1", config.InterfaceIPAddress);
        Assert.Equal("12.0.0.100", config.BaseIPAddress);
        Assert.Equal(60, config.PingInterval);
        Assert.Equal(10, config.RtpBufferSize);
        Assert.Equal(500, config.RtpTimeoutMs);
        Assert.Equal(55, config.AudioRxTimeoutMs);
        Assert.Equal(55, config.AudioTxTimeoutMs);
        Assert.Equal(500, config.RxVolume);
        Assert.Equal(100, config.TxVolume);
        Assert.Equal("PCM", config.RTPAudioOutputType);
        Assert.False(config.AllowTransmitInterrupt);
        Assert.False(config.AllowSendEmergency);
        Assert.Equal("B", config.ConnectionType);
    }

    [Fact]
    public void HyteraProtocolConfig_IsPcmAudio_ReturnsTrueForPCM()
    {
        // Arrange
        var config = new HyteraProtocolConfig
        {
            RTPAudioOutputType = "PCM"
        };
        
        // Act & Assert
        Assert.True(config.IsPcmAudio);
    }

    [Fact]
    public void HyteraProtocolConfig_IsPcmAudio_ReturnsFalseForAMBE()
    {
        // Arrange
        var config = new HyteraProtocolConfig
        {
            RTPAudioOutputType = "AMBE"
        };
        
        // Act & Assert
        Assert.False(config.IsPcmAudio);
    }

    [Fact]
    public void HyteraProtocolConfig_IsPcmAudio_IsCaseInsensitive()
    {
        // Arrange
        var config1 = new HyteraProtocolConfig { RTPAudioOutputType = "pcm" };
        var config2 = new HyteraProtocolConfig { RTPAudioOutputType = "Pcm" };
        var config3 = new HyteraProtocolConfig { RTPAudioOutputType = "PCM" };
        
        // Act & Assert
        Assert.True(config1.IsPcmAudio);
        Assert.True(config2.IsPcmAudio);
        Assert.True(config3.IsPcmAudio);
    }

    [Fact]
    public void HyteraProtocolConfigLoader_LoadOrDefault_ReturnsDefaultWhenFileNotFound()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xml");
        
        // Act
        var config = HyteraProtocolConfigLoader.LoadOrDefault(nonExistentFile);
        
        // Assert
        Assert.NotNull(config);
        Assert.Equal("192.168.10.1", config.InterfaceIPAddress);
    }

    [Fact]
    public void HyteraProtocolConfigLoader_Load_LoadsActualXmlFile()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Hytera>
  <InterfaceIPAddress>192.168.10.1</InterfaceIPAddress>
  <BaseIPAddress>12.0.0.100</BaseIPAddress>
  <PingInterval>60</PingInterval>
  <RtpBufferSize>10</RtpBufferSize>
  <RtpTimeoutMs>500</RtpTimeoutMs>
  <AudioRxTimeoutMs>55</AudioRxTimeoutMs>
  <AudioTxTimeoutMs>55</AudioTxTimeoutMs>
  <RxVolume>500</RxVolume>
  <TxVolume>100</TxVolume>
  <RTPAudioOutputType>PCM</RTPAudioOutputType>
  <AllowTransmitInterrupt>false</AllowTransmitInterrupt>
  <AllowSendEmergency>false</AllowSendEmergency>
  <ConnectionType>B</ConnectionType>
</Hytera>";
        
        var tempFile = Path.GetTempFileName();
        
        try
        {
            // Act
            File.WriteAllText(tempFile, xmlContent);
            var config = HyteraProtocolConfigLoader.Load(tempFile);
            
            // Assert
            Assert.NotNull(config);
            Assert.Equal("192.168.10.1", config.InterfaceIPAddress);
            Assert.Equal("12.0.0.100", config.BaseIPAddress);
            Assert.Equal(60, config.PingInterval);
            Assert.Equal("PCM", config.RTPAudioOutputType);
            Assert.True(config.IsPcmAudio);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void HyteraProtocolConfigLoader_Load_LoadsVendorXmlFile()
    {
        // Arrange
        var vendorXmlPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "vendor", 
            "Hytera_HyteraProtocol.xml"
        );
        
        // Act & Assert - only test if file exists
        if (File.Exists(vendorXmlPath))
        {
            var config = HyteraProtocolConfigLoader.Load(vendorXmlPath);
            
            Assert.NotNull(config);
            Assert.Equal("PCM", config.RTPAudioOutputType);
            Assert.True(config.IsPcmAudio);
            Assert.Equal(55, config.AudioRxTimeoutMs);
            Assert.Equal(55, config.AudioTxTimeoutMs);
        }
    }

    [Fact]
    public void TimerConfig_DefaultValues_AreCorrect()
    {
        // Act
        var config = new TimerConfig();
        
        // Assert
        Assert.Equal(35, config.RRSFirstTimerInMin);
        Assert.Equal(2, config.RRSSecondTimerInMin);
        Assert.Equal(1, config.TimerSleepLetturaPacchettiInMs);
    }

    [Fact]
    public void StatusMessage_Properties_SetCorrectly()
    {
        // Arrange & Act
        var message = new StatusMessage
        {
            Code = "91",
            Voice = false,
            Emergency = false,
            Text = "MARCATURA"
        };
        
        // Assert
        Assert.Equal("91", message.Code);
        Assert.False(message.Voice);
        Assert.False(message.Emergency);
        Assert.Equal("MARCATURA", message.Text);
    }
}
