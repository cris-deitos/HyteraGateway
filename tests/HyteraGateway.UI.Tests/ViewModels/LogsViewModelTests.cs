using System;
using FluentAssertions;
using HyteraGateway.UI.ViewModels;
using HyteraGateway.UI.Services;
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
        viewModel.Logs.Should().BeEmpty(); // No sample logs in new implementation
        viewModel.AutoScroll.Should().BeTrue();
        viewModel.ShowInfo.Should().BeTrue();
        viewModel.ShowWarning.Should().BeTrue();
        viewModel.ShowError.Should().BeTrue();
        viewModel.FilterText.Should().BeEmpty();
        viewModel.MaxLogEntries.Should().Be(1000);
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

    [Theory]
    [InlineData("[ERROR] Test message", LogLevel.Error)]
    [InlineData("[ERR] Test message", LogLevel.Error)]
    [InlineData("[WARNING] Test message", LogLevel.Warning)]
    [InlineData("[WARN] Test message", LogLevel.Warning)]
    [InlineData("[INFO] Test message", LogLevel.Info)]
    [InlineData("Plain message", LogLevel.Info)]
    public void ParseLogEntry_ShouldDetectCorrectLevel(string message, LogLevel expectedLevel)
    {
        // Arrange
        var viewModel = new LogsViewModel(_mockSignalR.Object);
        
        // Act - Use reflection to call private method
        var method = typeof(LogsViewModel).GetMethod("ParseLogEntry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var entry = (LogEntry)method!.Invoke(viewModel, new object[] { message })!;
        
        // Assert
        entry.Level.Should().Be(expectedLevel);
        entry.Message.Should().Be(message);
    }

    [Fact]
    public void FilteredLogs_RespectsLevelFilters()
    {
        // Arrange
        var viewModel = new LogsViewModel(_mockSignalR.Object);
        viewModel.Logs.Add(new LogEntry { Level = LogLevel.Info, Message = "Info message" });
        viewModel.Logs.Add(new LogEntry { Level = LogLevel.Warning, Message = "Warning message" });
        viewModel.Logs.Add(new LogEntry { Level = LogLevel.Error, Message = "Error message" });
        
        // Act - Show only errors
        viewModel.ShowInfo = false;
        viewModel.ShowWarning = false;
        viewModel.ShowError = true;
        
        // Assert
        var filtered = viewModel.FilteredLogs;
        filtered.Should().HaveCount(1);
        filtered.First().Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public void FilteredLogs_RespectsTextFilter()
    {
        // Arrange
        var viewModel = new LogsViewModel(_mockSignalR.Object);
        viewModel.Logs.Add(new LogEntry { Level = LogLevel.Info, Message = "Connection established" });
        viewModel.Logs.Add(new LogEntry { Level = LogLevel.Info, Message = "Data received" });
        
        // Act
        viewModel.FilterText = "connection";
        
        // Assert
        var filtered = viewModel.FilteredLogs;
        filtered.Should().HaveCount(1);
        filtered.First().Message.Should().ContainEquivalentOf("connection");
    }
}
