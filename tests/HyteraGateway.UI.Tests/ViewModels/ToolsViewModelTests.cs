using FluentAssertions;
using HyteraGateway.UI.ViewModels;
using HyteraGateway.UI.Services;
using Moq;
using Xunit;

namespace HyteraGateway.UI.Tests.ViewModels;

public class ToolsViewModelTests
{
    private readonly Mock<NetworkDiscoveryService> _mockNetworkDiscovery;

    public ToolsViewModelTests()
    {
        _mockNetworkDiscovery = new Mock<NetworkDiscoveryService>();
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var viewModel = new ToolsViewModel(_mockNetworkDiscovery.Object);
        
        // Assert
        viewModel.PingTarget.Should().Be("192.168.1.1");
        viewModel.PingResult.Should().BeEmpty();
        viewModel.HexPacketInput.Should().BeEmpty();
    }

    [Fact]
    public void AnalyzePacketCommand_WithEmptyInput_ShowsError()
    {
        // Arrange
        var viewModel = new ToolsViewModel(_mockNetworkDiscovery.Object);
        viewModel.HexPacketInput = "";
        
        // Act
        viewModel.AnalyzePacketCommand.Execute(null);
        
        // Assert
        viewModel.PacketAnalysisResult.Should().Contain("empty");
    }

    [Fact]
    public void AnalyzePacketCommand_WithValidHex_ShowsAnalysis()
    {
        // Arrange
        var viewModel = new ToolsViewModel(_mockNetworkDiscovery.Object);
        viewModel.HexPacketInput = "48 59 54 45 52 41"; // "HYTERA" in hex
        
        // Act
        viewModel.AnalyzePacketCommand.Execute(null);
        
        // Assert
        viewModel.PacketAnalysisResult.Should().NotBeEmpty();
        viewModel.PacketAnalysisResult.Should().Contain("6 bytes");
    }
}
