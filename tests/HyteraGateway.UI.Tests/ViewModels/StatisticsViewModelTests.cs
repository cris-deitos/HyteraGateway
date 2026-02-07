using FluentAssertions;
using HyteraGateway.UI.ViewModels;
using HyteraGateway.UI.Services;
using Moq;
using Xunit;

namespace HyteraGateway.UI.Tests.ViewModels;

public class StatisticsViewModelTests
{
    private readonly Mock<ApiService> _mockApi;

    public StatisticsViewModelTests()
    {
        _mockApi = new Mock<ApiService>();
    }

    [Fact]
    public void DefaultDateRange_IsLastSevenDays()
    {
        // Arrange & Act
        var viewModel = new StatisticsViewModel(_mockApi.Object);
        
        // Assert
        viewModel.StartDate.Should().BeCloseTo(DateTime.Today.AddDays(-7), TimeSpan.FromDays(1));
        viewModel.EndDate.Should().BeCloseTo(DateTime.Today, TimeSpan.FromDays(1));
    }

    [Fact]
    public void TotalCalls_InitialValue_IsZero()
    {
        // Arrange & Act
        var viewModel = new StatisticsViewModel(_mockApi.Object);
        
        // Assert
        viewModel.TotalCalls.Should().Be(0);
        viewModel.ActiveRadios.Should().Be(0);
    }
}
