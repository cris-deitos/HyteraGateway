using FluentAssertions;
using HyteraGateway.UI.ViewModels;
using HyteraGateway.UI.Services;
using HyteraGateway.UI.Models;
using Moq;
using Xunit;

namespace HyteraGateway.UI.Tests.ViewModels;

public class LogsViewModelTests
{
    private readonly Mock<SignalRService> _mockSignalR;

    public LogsViewModelTests()
    {
        _mockSignalR = new Mock<SignalRService>();
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var viewModel = new LogsViewModel(_mockSignalR.Object);
        
        // Assert
        viewModel.Logs.Should().NotBeNull();
        viewModel.Logs.Should().NotBeEmpty(); // Contains sample logs
        viewModel.AutoScroll.Should().BeTrue();
        viewModel.SelectedLogLevel.Should().Be("All");
        viewModel.FilterText.Should().BeEmpty();
    }

    [Fact]
    public void LogLevels_ContainsExpectedValues()
    {
        // Arrange
        var viewModel = new LogsViewModel(_mockSignalR.Object);
        
        // Assert
        viewModel.LogLevels.Should().Contain("All");
        viewModel.LogLevels.Should().Contain("Debug");
        viewModel.LogLevels.Should().Contain("Info");
        viewModel.LogLevels.Should().Contain("Warning");
        viewModel.LogLevels.Should().Contain("Error");
    }

    [Fact]
    public void ClearLogsCommand_ClearsAllLogs()
    {
        // Arrange
        var viewModel = new LogsViewModel(_mockSignalR.Object);
        viewModel.Logs.Add(new LogEntry { Message = "Test" });
        
        // Act
        viewModel.ClearLogsCommand.Execute(null);
        
        // Assert
        viewModel.Logs.Should().BeEmpty();
    }

    [Fact]
    public void FilteredLogs_WithLevelFilter_ReturnsFilteredResults()
    {
        // Arrange
        var viewModel = new LogsViewModel(_mockSignalR.Object);
        viewModel.Logs.Clear();
        viewModel.Logs.Add(new LogEntry { Level = "Info", Message = "Info message" });
        viewModel.Logs.Add(new LogEntry { Level = "Error", Message = "Error message" });
        
        // Act
        viewModel.SelectedLogLevel = "Error";
        
        // Assert
        viewModel.FilteredLogs.Should().HaveCount(1);
        viewModel.FilteredLogs.First().Level.Should().Be("Error");
    }
}
