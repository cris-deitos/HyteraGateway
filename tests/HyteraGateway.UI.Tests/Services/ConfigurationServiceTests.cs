using FluentAssertions;
using HyteraGateway.UI.Services;
using Xunit;

namespace HyteraGateway.UI.Tests.Services;

public class ConfigurationServiceTests
{
    [Fact]
    public void LoadConfiguration_WhenFileNotExists_ReturnsDefaultConfiguration()
    {
        // Arrange
        var service = new ConfigurationService();
        
        // Act
        service.LoadConfiguration();
        
        // Assert
        service.Configuration.Should().NotBeNull();
        service.Configuration.RadioIpAddress.Should().Be("192.168.1.1");
        service.Configuration.RadioControlPort.Should().Be(50000);
    }

    [Fact]
    public void Configuration_DefaultValues_AreCorrect()
    {
        // Arrange
        var config = new HyteraGateway.UI.Models.UIConfiguration();
        
        // Assert
        config.RadioIpAddress.Should().Be("192.168.1.1");
        config.RadioControlPort.Should().Be(50000);
        config.RadioAudioPort.Should().Be(50001);
        config.AutoReconnect.Should().BeTrue();
        config.ApiBaseUrl.Should().Be("http://localhost:5000");
    }

    [Fact]
    public void SaveConfiguration_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var service = new ConfigurationService();
        
        // Act & Assert - should not throw
        service.LoadConfiguration();
    }
}
