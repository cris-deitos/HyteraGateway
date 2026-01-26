using HyteraGateway.Radio.Configuration;

namespace HyteraGateway.Radio.Tests.Configuration;

public class RadioControllerConfigTests
{
    [Fact]
    public void RadioControllerConfig_DefaultConstructor_InitializesCollections()
    {
        // Act
        var config = new RadioControllerConfig();
        
        // Assert
        Assert.NotNull(config.Radios);
        Assert.NotNull(config.Slots);
        Assert.Empty(config.Radios);
        Assert.Empty(config.Slots);
    }
    
    [Fact]
    public void RadioControllerConfig_DefaultRadioPort_Is50000()
    {
        // Arrange & Act
        var config = new RadioControllerConfig();
        
        // Assert
        Assert.Equal(50000, config.RadioPort);
    }
    
    [Fact]
    public void RadioControllerConfig_DefaultPttTimeoutSeconds_Is180()
    {
        // Arrange & Act
        var config = new RadioControllerConfig();
        
        // Assert
        Assert.Equal(180, config.PttTimeoutSeconds);
    }
    
    [Fact]
    public void RadioControllerConfig_DefaultVoipEnabled_IsFalse()
    {
        // Arrange & Act
        var config = new RadioControllerConfig();
        
        // Assert
        Assert.False(config.VoipEnabled);
    }
    
    [Fact]
    public void RadioControllerConfig_DefaultVrsEnabled_IsFalse()
    {
        // Arrange & Act
        var config = new RadioControllerConfig();
        
        // Assert
        Assert.False(config.VrsEnabled);
    }
    
    [Fact]
    public void RadioControllerConfig_DefaultActivityCheckEnabled_IsFalse()
    {
        // Arrange & Act
        var config = new RadioControllerConfig();
        
        // Assert
        Assert.False(config.ActivityCheckEnabled);
    }
    
    [Fact]
    public void RadioControllerConfig_DefaultActivityCheckMinutes_Is60()
    {
        // Arrange & Act
        var config = new RadioControllerConfig();
        
        // Assert
        Assert.Equal(60, config.ActivityCheckMinutes);
    }
    
    [Fact]
    public void RadioControllerConfig_DefaultPositionCheckEnabled_IsFalse()
    {
        // Arrange & Act
        var config = new RadioControllerConfig();
        
        // Assert
        Assert.False(config.PositionCheckEnabled);
    }
    
    [Fact]
    public void RadioControllerConfig_DefaultPositionCheckMinutes_Is30()
    {
        // Arrange & Act
        var config = new RadioControllerConfig();
        
        // Assert
        Assert.Equal(30, config.PositionCheckMinutes);
    }
    
    [Fact]
    public void RadioControllerConfig_Properties_SetAndGetCorrectly()
    {
        // Arrange
        var config = new RadioControllerConfig
        {
            RadioIpAddress = "192.168.1.50",
            RadioPort = 50001,
            DispatcherId = 12345,
            PttTimeoutSeconds = 240,
            VoipEnabled = true,
            VrsEnabled = true,
            ActivityCheckEnabled = true,
            ActivityCheckMinutes = 45,
            PositionCheckEnabled = true,
            PositionCheckMinutes = 15
        };
        
        // Assert
        Assert.Equal("192.168.1.50", config.RadioIpAddress);
        Assert.Equal(50001, config.RadioPort);
        Assert.Equal((uint)12345, config.DispatcherId);
        Assert.Equal(240, config.PttTimeoutSeconds);
        Assert.True(config.VoipEnabled);
        Assert.True(config.VrsEnabled);
        Assert.True(config.ActivityCheckEnabled);
        Assert.Equal(45, config.ActivityCheckMinutes);
        Assert.True(config.PositionCheckEnabled);
        Assert.Equal(15, config.PositionCheckMinutes);
    }
    
    [Fact]
    public void RadioConfig_Properties_SetAndGetCorrectly()
    {
        // Arrange
        var radioConfig = new RadioConfig
        {
            DmrId = 1234567,
            Name = "Test Radio",
            IpAddress = "192.168.1.100",
            Port = 50001,
            Enabled = true
        };
        
        // Assert
        Assert.Equal(1234567, radioConfig.DmrId);
        Assert.Equal("Test Radio", radioConfig.Name);
        Assert.Equal("192.168.1.100", radioConfig.IpAddress);
        Assert.Equal(50001, radioConfig.Port);
        Assert.True(radioConfig.Enabled);
    }
    
    [Fact]
    public void RadioConfig_DefaultPort_Is50000()
    {
        // Arrange & Act
        var radioConfig = new RadioConfig();
        
        // Assert
        Assert.Equal(50000, radioConfig.Port);
    }
    
    [Fact]
    public void RadioConfig_DefaultEnabled_IsTrue()
    {
        // Arrange & Act
        var radioConfig = new RadioConfig();
        
        // Assert
        Assert.True(radioConfig.Enabled);
    }
    
    [Fact]
    public void SlotConfig_Properties_SetAndGetCorrectly()
    {
        // Arrange
        var slotConfig = new SlotConfig
        {
            Number = 1,
            Name = "Slot 1",
            TalkGroupId = 100,
            IsDefault = true,
            PttEnabled = true,
            Visible = true
        };
        
        // Assert
        Assert.Equal(1, slotConfig.Number);
        Assert.Equal("Slot 1", slotConfig.Name);
        Assert.Equal(100, slotConfig.TalkGroupId);
        Assert.True(slotConfig.IsDefault);
        Assert.True(slotConfig.PttEnabled);
        Assert.True(slotConfig.Visible);
    }
    
    [Fact]
    public void SlotConfig_DefaultPttEnabled_IsTrue()
    {
        // Arrange & Act
        var slotConfig = new SlotConfig();
        
        // Assert
        Assert.True(slotConfig.PttEnabled);
    }
    
    [Fact]
    public void SlotConfig_DefaultVisible_IsTrue()
    {
        // Arrange & Act
        var slotConfig = new SlotConfig();
        
        // Assert
        Assert.True(slotConfig.Visible);
    }
    
    [Fact]
    public void RadioControllerConfigLoader_SaveAndLoad_PreservesData()
    {
        // Arrange
        var config = new RadioControllerConfig
        {
            RadioIpAddress = "192.168.1.50",
            RadioPort = 50002,
            DispatcherId = 99999,
            PttTimeoutSeconds = 300,
            VoipEnabled = true,
            VrsEnabled = true,
            ActivityCheckEnabled = true,
            ActivityCheckMinutes = 90,
            PositionCheckEnabled = true,
            PositionCheckMinutes = 20,
            Radios = new List<RadioConfig>
            {
                new RadioConfig
                {
                    DmrId = 1234567,
                    Name = "Test Radio 1",
                    IpAddress = "192.168.1.100",
                    Port = 50000,
                    Enabled = true
                },
                new RadioConfig
                {
                    DmrId = 7654321,
                    Name = "Test Radio 2",
                    IpAddress = "192.168.1.101",
                    Port = 50001,
                    Enabled = false
                }
            },
            Slots = new List<SlotConfig>
            {
                new SlotConfig
                {
                    Number = 1,
                    Name = "Slot 1",
                    TalkGroupId = 100,
                    IsDefault = true,
                    PttEnabled = true,
                    Visible = true
                },
                new SlotConfig
                {
                    Number = 2,
                    Name = "Slot 2",
                    TalkGroupId = 200,
                    IsDefault = false,
                    PttEnabled = true,
                    Visible = false
                }
            }
        };
        
        var tempFile = Path.GetTempFileName();
        
        try
        {
            // Act
            RadioControllerConfigLoader.Save(config, tempFile);
            var loaded = RadioControllerConfigLoader.Load(tempFile);
            
            // Assert - Root properties
            Assert.Equal("192.168.1.50", loaded.RadioIpAddress);
            Assert.Equal(50002, loaded.RadioPort);
            Assert.Equal((uint)99999, loaded.DispatcherId);
            Assert.Equal(300, loaded.PttTimeoutSeconds);
            Assert.True(loaded.VoipEnabled);
            Assert.True(loaded.VrsEnabled);
            Assert.True(loaded.ActivityCheckEnabled);
            Assert.Equal(90, loaded.ActivityCheckMinutes);
            Assert.True(loaded.PositionCheckEnabled);
            Assert.Equal(20, loaded.PositionCheckMinutes);
            
            Assert.Equal(2, loaded.Radios.Count);
            Assert.Equal(2, loaded.Slots.Count);
            
            // Check first radio
            Assert.Equal(1234567, loaded.Radios[0].DmrId);
            Assert.Equal("Test Radio 1", loaded.Radios[0].Name);
            Assert.Equal("192.168.1.100", loaded.Radios[0].IpAddress);
            Assert.Equal(50000, loaded.Radios[0].Port);
            Assert.True(loaded.Radios[0].Enabled);
            
            // Check second radio
            Assert.Equal(7654321, loaded.Radios[1].DmrId);
            Assert.Equal("Test Radio 2", loaded.Radios[1].Name);
            Assert.False(loaded.Radios[1].Enabled);
            
            // Check first slot
            Assert.Equal(1, loaded.Slots[0].Number);
            Assert.Equal("Slot 1", loaded.Slots[0].Name);
            Assert.Equal(100, loaded.Slots[0].TalkGroupId);
            Assert.True(loaded.Slots[0].IsDefault);
            
            // Check second slot
            Assert.Equal(2, loaded.Slots[1].Number);
            Assert.Equal("Slot 2", loaded.Slots[1].Name);
            Assert.Equal(200, loaded.Slots[1].TalkGroupId);
            Assert.False(loaded.Slots[1].IsDefault);
            Assert.False(loaded.Slots[1].Visible);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
