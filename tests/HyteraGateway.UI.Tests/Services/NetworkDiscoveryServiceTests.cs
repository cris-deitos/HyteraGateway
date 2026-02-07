using FluentAssertions;
using HyteraGateway.UI.Services;
using HyteraGateway.Core.Configuration;
using Xunit;

namespace HyteraGateway.UI.Tests.Services;

public class NetworkDiscoveryServiceTests
{
    private readonly NetworkDiscoveryService _service;

    public NetworkDiscoveryServiceTests()
    {
        _service = new NetworkDiscoveryService();
    }

    [Fact]
    public void GetUsbNetworkInterfaces_ReturnsListOfInterfaces()
    {
        // Act
        var interfaces = _service.GetUsbNetworkInterfaces();
        
        // Assert
        interfaces.Should().NotBeNull();
        // May be empty if no USB interfaces present
    }

    [Fact]
    public async Task TestRadioConnectionAsync_WithInvalidIp_ReturnsFalse()
    {
        // Act
        var result = await _service.TestRadioConnectionAsync("192.168.255.255", 50000, 100);
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ScanSubnetForRadiosAsync_WithCancellation_StopsScanning()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        // Act
        var radios = await _service.ScanSubnetForRadiosAsync("192.168.1", 1, 10, 100, null, cts.Token);
        
        // Assert
        radios.Should().BeEmpty();
    }

    [Fact]
    public async Task IdentifyRadioAsync_ReturnsDiscoveredRadio()
    {
        // Act
        var radio = await _service.IdentifyRadioAsync("192.168.1.1");
        
        // Assert
        radio.Should().NotBeNull();
        radio.IpAddress.Should().Be("192.168.1.1");
        radio.Port.Should().Be(50000);
        radio.ConnectionType.Should().Be(RadioConnectionType.Ethernet);
    }

    [Theory]
    [InlineData("192.168.1")]
    [InlineData("10.0.0")]
    [InlineData("172.16.0")]
    public async Task ScanSubnetForRadiosAsync_WithValidSubnet_DoesNotThrow(string subnet)
    {
        // Act & Assert - should not throw
        var radios = await _service.ScanSubnetForRadiosAsync(subnet, 1, 5, 50);
        radios.Should().NotBeNull();
    }
}
