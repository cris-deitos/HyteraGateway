using FluentAssertions;
using HyteraGateway.UI.ViewModels;
using HyteraGateway.UI.Services;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace HyteraGateway.UI.Tests.ViewModels;

public class AudioControlViewModelTests
{
    private readonly Mock<AudioService> _mockAudioService;
    private readonly AudioControlViewModel _viewModel;

    public AudioControlViewModelTests()
    {
        _mockAudioService = new Mock<AudioService>();
        _viewModel = new AudioControlViewModel(_mockAudioService.Object);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Assert
        _viewModel.Volume.Should().Be(0.8f);
        _viewModel.SelectedTalkGroupId.Should().Be(9);
        _viewModel.AudioState.Should().Be("Disconnected");
        _viewModel.IsConnected.Should().BeFalse();
        _viewModel.IsTransmitting.Should().BeFalse();
        _viewModel.IsReceiving.Should().BeFalse();
        _viewModel.IsMuted.Should().BeFalse();
        _viewModel.AudioLevel.Should().Be(0);
    }

    [Fact]
    public void ToggleMute_ChangesIsMutedState()
    {
        // Arrange
        var initialState = _viewModel.IsMuted;

        // Act
        _viewModel.ToggleMuteCommand.Execute(null);

        // Assert
        _viewModel.IsMuted.Should().Be(!initialState);
    }

    [Fact]
    public void ToggleMute_TwiceReturnsToOriginalState()
    {
        // Arrange
        var initialState = _viewModel.IsMuted;

        // Act
        _viewModel.ToggleMuteCommand.Execute(null);
        _viewModel.ToggleMuteCommand.Execute(null);

        // Assert
        _viewModel.IsMuted.Should().Be(initialState);
    }

    [Fact]
    public void OnVolumeChanged_UpdatesAudioServiceVolume()
    {
        // Act
        _viewModel.Volume = 0.5f;

        // Assert
        _mockAudioService.Object.Volume.Should().Be(0.5f);
    }

    [Fact]
    public void SelectedRadioId_CanBeSet()
    {
        // Act
        _viewModel.SelectedRadioId = 12345;

        // Assert
        _viewModel.SelectedRadioId.Should().Be(12345);
    }

    [Fact]
    public void SelectedTalkGroupId_CanBeSet()
    {
        // Act
        _viewModel.SelectedTalkGroupId = 99;

        // Assert
        _viewModel.SelectedTalkGroupId.Should().Be(99);
    }

    [Fact]
    public void AudioState_InitiallyDisconnected()
    {
        // Assert
        _viewModel.AudioState.Should().Be("Disconnected");
    }
}
