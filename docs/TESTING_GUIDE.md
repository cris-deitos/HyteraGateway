# HyteraGateway - Testing Guide

## Overview

This guide covers how to run tests, interpret results, and measure code coverage for HyteraGateway.

## Test Framework

HyteraGateway uses:
- **xUnit** - Test framework
- **Moq** - Mocking library  
- **Coverlet** - Code coverage tool
- **FluentAssertions** - Assertion library (future)

## Running Tests

### Run All Tests

```bash
cd /path/to/HyteraGateway
dotnet test
```

### Run Specific Test Project

```bash
# Radio tests only
dotnet test tests/HyteraGateway.Radio.Tests

# Core tests only
dotnet test src/HyteraGateway.Tests
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~PacketValidationTests"
```

### Run Specific Test Method

```bash
dotnet test --filter "FullyQualifiedName~PacketValidationTests.Validator_ValidPacket_ReturnsNone"
```

### Run Tests with Verbosity

```bash
# Minimal output
dotnet test --verbosity minimal

# Normal output (default)
dotnet test --verbosity normal

# Detailed output
dotnet test --verbosity detailed
```

## Test Organization

### Project Structure

```
tests/
├── HyteraGateway.Radio.Tests/
│   ├── Configuration/
│   │   └── RadioControllerConfigTests.cs
│   ├── Protocol/
│   │   ├── DMRPacketTests.cs
│   │   ├── DMRVoiceFrameTests.cs
│   │   ├── HyteraIPSCPacketTests.cs
│   │   └── PacketValidationTests.cs
│   └── Services/
│       ├── CallRecorderTests.cs
│       ├── DualSlotManagerTests.cs
│       └── [Other service tests]
└── src/
    └── HyteraGateway.Tests/
        └── [Core tests]
```

### Naming Conventions

Test methods follow the pattern:
```
[MethodName]_[Scenario]_[ExpectedBehavior]
```

Examples:
- `Validate_ValidPacket_ReturnsNone`
- `HandleCallStart_Slot1_SetsSlotActive`
- `OnPttPressed_ValidDestination_StartsSession`

## Test Categories

### Unit Tests

Test individual components in isolation.

**Example:**
```csharp
[Fact]
public void Validator_ValidPacket_ReturnsNone()
{
    // Arrange
    var packet = HyteraIPSCPacket.CreateKeepalive(9000001);
    var bytes = packet.ToBytes();
    
    // Act
    var result = HyteraPacketValidator.Validate(bytes);
    
    // Assert
    Assert.Equal(ValidationSeverity.None, result.Severity);
    Assert.Null(result.ErrorMessage);
}
```

### Integration Tests

Test multiple components working together.

**Example:**
```csharp
[Fact]
public async Task Connection_Reconnect_RestoresConnection()
{
    // Test auto-reconnect logic with real network operations
}
```

### Theory Tests

Test same logic with multiple inputs.

**Example:**
```csharp
[Theory]
[InlineData(1, 1000)]
[InlineData(2, 2000)]
[InlineData(3, 5000)]
public void GetBackoffDelay_AttemptNumber_ReturnsCorrectDelay(
    int attempt, int expectedDelay)
{
    // Test exponential backoff logic
}
```

## Code Coverage

### Generate Coverage Report

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML report (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
    -reports:tests/**/coverage.opencover.xml \
    -targetdir:coverage-report \
    -reporttypes:Html

# View report
open coverage-report/index.html  # macOS
xdg-open coverage-report/index.html  # Linux
start coverage-report/index.html  # Windows
```

### Coverage Thresholds

Current coverage targets:
- **Overall**: >85%
- **Critical paths**: 100%
- **Protocol handling**: 100%
- **Data validation**: 100%
- **Error handling**: >90%

### View Coverage by Project

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutput=../coverage/ \
    /p:MergeWith="../coverage/coverage.json" \
    /p:CoverletOutputFormat=\"json,opencover\"
```

## Current Test Statistics

As of the latest build:

```
Project: HyteraGateway.Radio.Tests
├── Total Tests: 102
├── Passed: 102
├── Failed: 0
├── Skipped: 0
└── Duration: ~650ms

Breakdown by Category:
├── Configuration: 17 tests
├── Protocol: 45 tests
│   ├── DMR: 16 tests
│   ├── IPSC: 12 tests
│   └── Validation: 17 tests
└── Services: 40 tests
    ├── CallRecorder: 14 tests
    ├── DualSlotManager: 13 tests
    └── Others: 13 tests
```

## Writing New Tests

### Template for Unit Tests

```csharp
using HyteraGateway.Radio.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HyteraGateway.Radio.Tests.Services;

public class MyServiceTests
{
    private readonly Mock<ILogger<MyService>> _mockLogger;
    private readonly MyService _service;
    
    public MyServiceTests()
    {
        _mockLogger = new Mock<ILogger<MyService>>();
        _service = new MyService(_mockLogger.Object);
    }
    
    [Fact]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var input = "test";
        
        // Act
        var result = _service.Method(input);
        
        // Assert
        Assert.Equal("expected", result);
    }
}
```

### Template for Async Tests

```csharp
[Fact]
public async Task MethodNameAsync_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = "test";
    var cancellationToken = CancellationToken.None;
    
    // Act
    var result = await _service.MethodAsync(input, cancellationToken);
    
    // Assert
    Assert.NotNull(result);
}
```

### Testing Exceptions

```csharp
[Fact]
public void Method_InvalidInput_ThrowsArgumentException()
{
    // Arrange
    string? input = null;
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() => _service.Method(input!));
}

[Fact]
public async Task MethodAsync_InvalidInput_ThrowsArgumentExceptionAsync()
{
    // Arrange
    string? input = null;
    
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
        async () => await _service.MethodAsync(input!));
}
```

### Using Mocks

```csharp
[Fact]
public async Task SendCommand_ValidCommand_CallsConnection()
{
    // Arrange
    var mockConnection = new Mock<IHyteraConnection>();
    mockConnection
        .Setup(x => x.SendPacketAsync(It.IsAny<HyteraIPSCPacket>(), 
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);
    
    var service = new RadioService(mockConnection.Object);
    
    // Act
    var result = await service.SendCommandAsync("TEST");
    
    // Assert
    Assert.True(result);
    mockConnection.Verify(
        x => x.SendPacketAsync(It.IsAny<HyteraIPSCPacket>(), 
            It.IsAny<CancellationToken>()), 
        Times.Once);
}
```

### Testing Events

```csharp
[Fact]
public void RaiseEvent_WhenConditionMet_FiresEvent()
{
    // Arrange
    bool eventFired = false;
    _service.SomeEvent += (sender, args) => eventFired = true;
    
    // Act
    _service.DoSomething();
    
    // Assert
    Assert.True(eventFired);
}
```

### Testing File Operations

```csharp
[Fact]
public async Task SaveFile_ValidData_CreatesFile()
{
    // Arrange
    var tempPath = Path.GetTempPath();
    var fileName = $"test_{Guid.NewGuid()}.dat";
    var fullPath = Path.Combine(tempPath, fileName);
    
    try
    {
        // Act
        await _service.SaveFileAsync(fullPath, "content");
        
        // Assert
        Assert.True(File.Exists(fullPath));
        var content = await File.ReadAllTextAsync(fullPath);
        Assert.Equal("content", content);
    }
    finally
    {
        // Cleanup
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
```

## Continuous Integration

### GitHub Actions Workflow

```yaml
name: Build and Test

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal \
        /p:CollectCoverage=true \
        /p:CoverletOutputFormat=opencover
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v2
      with:
        files: tests/**/coverage.opencover.xml
```

## Performance Testing

### Benchmarking

For performance-critical code, use BenchmarkDotNet:

```bash
dotnet add package BenchmarkDotNet
```

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class PacketBenchmarks
{
    private byte[] _samplePacket;
    
    [GlobalSetup]
    public void Setup()
    {
        var packet = HyteraIPSCPacket.CreateKeepalive(9000001);
        _samplePacket = packet.ToBytes();
    }
    
    [Benchmark]
    public ValidationResult ValidatePacket()
    {
        return HyteraPacketValidator.Validate(_samplePacket);
    }
}
```

Run benchmarks:
```bash
dotnet run -c Release --project Benchmarks
```

## Test Data

### Sample Configurations

Test configurations are in:
```
tests/TestData/
├── valid-config.xml
├── invalid-config.xml
├── sample-packets/
│   ├── keepalive.bin
│   ├── ptt-press.bin
│   └── voice-frame.bin
└── expected-outputs/
    └── metadata.json
```

### Generating Test Packets

Use the ProtocolTester tool:

```bash
cd tools/ProtocolTester
dotnet run -- generate --command KEEPALIVE --output keepalive.bin
```

## Debugging Tests

### Run Tests in Debug Mode

In Visual Studio Code:
1. Set breakpoint in test
2. Click "Debug Test" above test method
3. Use debug console and variables

In Visual Studio:
1. Right-click test → Debug Test
2. Use standard debugging tools

### View Test Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Test Specific Configuration

```bash
# Set environment variables for tests
export TestConfig__RadioIp="192.168.1.100"
dotnet test
```

## Best Practices

### DO:
✅ Test one thing per test method
✅ Use descriptive test names
✅ Follow Arrange-Act-Assert pattern
✅ Clean up resources (files, connections)
✅ Mock external dependencies
✅ Test both success and failure paths
✅ Test edge cases and boundary conditions
✅ Keep tests fast (<100ms per test)

### DON'T:
❌ Test framework code
❌ Test third-party libraries
❌ Create interdependent tests
❌ Use magic numbers (use constants)
❌ Test implementation details
❌ Leave commented-out code
❌ Skip error cases

## Troubleshooting Tests

### Tests Hang

- Check for deadlocks in async code
- Verify timeouts are set
- Look for blocking I/O operations

### Flaky Tests

- Avoid time-dependent assertions
- Use proper synchronization
- Check for race conditions
- Mock time-dependent operations

### Tests Fail in CI but Pass Locally

- Check for timezone differences
- Verify file path separators
- Look for absolute paths
- Check environment variables

## Test Maintenance

### Regular Tasks

1. **Run tests before committing:**
   ```bash
   dotnet test
   ```

2. **Update tests when changing code:**
   - Update existing tests for modified behavior
   - Add tests for new functionality

3. **Review coverage reports:**
   - Identify untested code
   - Add tests for critical paths

4. **Cleanup obsolete tests:**
   - Remove tests for deleted features
   - Update tests for refactored code

### Quarterly Reviews

- Review test structure and organization
- Update test data and fixtures
- Refactor duplicated test code
- Update testing documentation

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Coverlet Coverage](https://github.com/coverlet-coverage/coverlet)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## Getting Help

If tests are failing:
1. Read the error message carefully
2. Check recent code changes
3. Run test in isolation
4. Enable verbose logging
5. Ask in GitHub Discussions

Report test infrastructure issues on GitHub Issues with:
- Test output
- System information
- Steps to reproduce
