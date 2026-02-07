using FluentAssertions;
using HyteraGateway.UI.ViewModels;
using HyteraGateway.UI.Services;
using HyteraGateway.UI.Models;
using HyteraGateway.Core.Configuration;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace HyteraGateway.UI.Tests.ViewModels;

public class SettingsViewModelTests
{
    private readonly Mock<ConfigurationService> _mockConfig;
    private readonly Mock<NetworkDiscoveryService> _mockNetworkDiscovery;

    public SettingsViewModelTests()
    {
        _mockConfig = new Mock<ConfigurationService>();
        _mockNetworkDiscovery = new Mock<NetworkDiscoveryService>();
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange
        var mockConfig = new UIConfiguration
        {
            RadioIpAddress = "192.168.1.1",
            RadioControlPort = 50000,
            AutoReconnect = true,
            DbHost = "localhost",
            DbPort = 3306
        };
        _mockConfig.Setup(c => c.Configuration).Returns(mockConfig);
        _mockNetworkDiscovery.Setup(n => n.GetUsbNetworkInterfaces()).Returns(new List<NetworkInterfaceInfo>());
        
        // Act
        var viewModel = new SettingsViewModel(_mockConfig.Object, _mockNetworkDiscovery.Object);
        
        // Assert
        viewModel.RadioIpAddress.Should().Be("192.168.1.1");
        viewModel.RadioControlPort.Should().Be(50000);
        viewModel.AutoReconnect.Should().BeTrue();
        viewModel.DbHost.Should().Be("localhost");
        viewModel.DbPort.Should().Be(3306);
    }

    [Fact]
    public void ConnectionTypes_ContainsAllValues()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockConfig.Object, _mockNetworkDiscovery.Object);
        
        // Assert
        viewModel.ConnectionTypes.Should().Contain(RadioConnectionType.USB);
        viewModel.ConnectionTypes.Should().Contain(RadioConnectionType.Ethernet);
        viewModel.ConnectionTypes.Should().Contain(RadioConnectionType.Auto);
    }

    [Fact]
    public void IsUsbMode_WhenUsbSelected_ReturnsTrue()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockConfig.Object, _mockNetworkDiscovery.Object);
        viewModel.SelectedConnectionType = RadioConnectionType.USB;
        
        // Assert
        viewModel.IsUsbMode.Should().BeTrue();
        viewModel.IsEthernetMode.Should().BeFalse();
    }

    [Fact]
    public void IsEthernetMode_WhenEthernetSelected_ReturnsTrue()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockConfig.Object, _mockNetworkDiscovery.Object);
        viewModel.SelectedConnectionType = RadioConnectionType.Ethernet;
        
        // Assert
        viewModel.IsEthernetMode.Should().BeTrue();
        viewModel.IsUsbMode.Should().BeFalse();
    }
}
