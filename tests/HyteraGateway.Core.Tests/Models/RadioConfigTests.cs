using FluentAssertions;
using HyteraGateway.Core.Configuration;
using Xunit;

namespace HyteraGateway.Core.Tests.Models;

public class RadioConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new RadioConfig();
        
        // Assert
        config.ConnectionType.Should().Be(RadioConnectionType.USB);
        config.InterfaceName.Should().Be("USB NCM");
        config.IpAddress.Should().Be("192.168.1.1");
        config.ControlPort.Should().Be(50000);
        config.AudioPort.Should().Be(50001);
        config.TimeoutSeconds.Should().Be(30);
        config.AutoReconnect.Should().BeTrue();
        config.ReconnectIntervalSeconds.Should().Be(10);
    }

    [Theory]
    [InlineData(RadioConnectionType.USB)]
    [InlineData(RadioConnectionType.Ethernet)]
    [InlineData(RadioConnectionType.Auto)]
    public void ConnectionType_CanBeSetToAllValues(RadioConnectionType type)
    {
        // Arrange
        var config = new RadioConfig();
        
        // Act
        config.ConnectionType = type;
        
        // Assert
        config.ConnectionType.Should().Be(type);
    }
}
