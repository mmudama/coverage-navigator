using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using CoverageNavigator.Contracts.Models;
using CoverageNavigator.Api.Services;

namespace CoverageNavigator.Api.Tests.Services;

/// <summary>
/// Tests for SystemPromptService documenting expected behaviors around:
/// - Configuration validation
/// - Directory and file existence validation
/// - Prompt loading and concatenation logic
/// - Additional prompt resolution
/// </summary>
public class SystemPromptServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _promptsDirectory;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<SystemPromptService>> _mockLogger;

    public SystemPromptServiceTests()
    {
        // Create temporary directory structure for testing
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"SystemPromptServiceTests_{Guid.NewGuid()}");
        _promptsDirectory = Path.Combine(_tempDirectory, "prompts");
        Directory.CreateDirectory(_promptsDirectory);

        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<SystemPromptService>>();
    }

    public void Dispose()
    {
        // Cleanup temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    #region Constructor Tests - Configuration Validation

    [Fact]
    public void Constructor_WhenPromptsBaseDirectoryNotConfigured_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns((string?)null);
        Environment.SetEnvironmentVariable("PROMPTS_BASE_DIRECTORY", null);

        // Act
        var act = () => new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PROMPTS_BASE_DIRECTORY not found in configuration or environment variables");
    }

    [Fact]
    public void Constructor_WhenPromptsDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");
        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(nonExistentPath);

        // Act
        var act = () => new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<DirectoryNotFoundException>()
            .WithMessage($"Prompts directory not found: {Path.Combine(nonExistentPath, "prompts")}");
    }

    [Fact]
    public void Constructor_WhenPromptsBaseDirectorySetInConfiguration_UsesConfigurationValue()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);

        // Act
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(_promptsDirectory)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WhenPromptsBaseDirectorySetInEnvironmentVariable_UsesEnvironmentValue()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns((string?)null);
        Environment.SetEnvironmentVariable("PROMPTS_BASE_DIRECTORY", _tempDirectory);

        try
        {
            // Act
            var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(_promptsDirectory)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PROMPTS_BASE_DIRECTORY", null);
        }
    }

    #endregion

    #region GetSystemPromptAsync Tests - File Loading Behavior

    [Fact]
    public async Task GetSystemPromptAsync_WhenDefaultPromptMissing_ThrowsFileNotFoundException()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest { Message = "test" };

        // Act
        var act = async () => await service.GetSystemPromptAsync(request);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage($"Default system prompt not found: {Path.Combine(_promptsDirectory, "system-default.md")}");
    }

    [Fact]
    public async Task GetSystemPromptAsync_WhenOnlyDefaultPromptExists_ReturnsDefaultPromptContent()
    {
        // Arrange
        var defaultPromptContent = "This is the default system prompt.";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest { Message = "test" };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().Be(defaultPromptContent);
    }

    [Fact]
    public async Task GetSystemPromptAsync_WhenAdditionalPromptSpecifiedButDoesNotExist_ReturnsOnlyDefaultPrompt()
    {
        // Arrange
        var defaultPromptContent = "Default prompt";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest 
        { 
            Message = "test",
            SystemPromptIdentifier = "nonexistent"
        };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().Be(defaultPromptContent);
        
        // Verify warning was logged
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSystemPromptAsync_WhenAdditionalPromptExists_ConcatenatesDefaultAndAdditionalPrompts()
    {
        // Arrange
        var defaultPromptContent = "Default instructions.";
        var additionalPromptContent = "Additional specific instructions.";
        
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-codeanalysis.md"), additionalPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest 
        { 
            Message = "test",
            SystemPromptIdentifier = "codeanalysis"
        };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().Contain(defaultPromptContent);
        result.Should().Contain(additionalPromptContent);
        
        // Verify order: default comes before additional
        var defaultIndex = result.IndexOf(defaultPromptContent);
        var additionalIndex = result.IndexOf(additionalPromptContent);
        defaultIndex.Should().BeLessThan(additionalIndex, "default prompt should come before additional prompt");
        
        // Verify they are separated by blank line(s) - platform agnostic
        var normalizedResult = result.Replace("\r\n", "\n");
        normalizedResult.Should().Contain($"{defaultPromptContent}\n\n{additionalPromptContent}");
    }

    [Fact]
    public async Task GetSystemPromptAsync_WithNullSystemPromptIdentifier_ReturnsOnlyDefaultPrompt()
    {
        // Arrange
        var defaultPromptContent = "Default prompt only";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest 
        { 
            Message = "test",
            SystemPromptIdentifier = null
        };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().Be(defaultPromptContent);
    }

    [Fact]
    public async Task GetSystemPromptAsync_WithEmptySystemPromptIdentifier_ReturnsOnlyDefaultPrompt()
    {
        // Arrange
        var defaultPromptContent = "Default prompt only";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest 
        { 
            Message = "test",
            SystemPromptIdentifier = ""
        };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().Be(defaultPromptContent);
    }

    #endregion

    #region Prompt Content and Formatting Tests

    [Fact]
    public async Task GetSystemPromptAsync_TrimsWhitespaceFromResult()
    {
        // Arrange
        var defaultPromptContent = "  Default prompt with spaces  \n\n";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest { Message = "test" };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().NotStartWith(" ");
        result.Should().NotEndWith(" ");
        result.Should().NotEndWith("\n");
        result.Should().Be("Default prompt with spaces");
    }

    [Fact]
    public async Task GetSystemPromptAsync_PreservesNewlinesWithinPromptContent()
    {
        // Arrange
        var defaultPromptContent = "Line 1\nLine 2\nLine 3";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest { Message = "test" };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().Contain("\n");
        result.Split('\n').Should().HaveCount(3);
    }

    [Fact]
    public async Task GetSystemPromptAsync_HandlesMultibyteCharactersCorrectly()
    {
        // Arrange
        var defaultPromptContent = "Unicode test: ???, Émojis: ????, Special: €£¥";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest { Message = "test" };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().Be(defaultPromptContent);
    }

    #endregion

    #region Logging Behavior Tests

    [Fact]
    public async Task GetSystemPromptAsync_LogsDebugInformationAboutPromptLength()
    {
        // Arrange
        var defaultPromptContent = "Test prompt content";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest { Message = "test" };

        // Act
        await service.GetSystemPromptAsync(request);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generated system prompt with length")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSystemPromptAsync_WhenAdditionalPromptLoaded_LogsDebugInformationAboutIdentifier()
    {
        // Arrange
        var defaultPromptContent = "Default";
        var additionalPromptContent = "Additional";
        
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-test.md"), additionalPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest 
        { 
            Message = "test",
            SystemPromptIdentifier = "test"
        };

        // Act
        await service.GetSystemPromptAsync(request);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Loaded additional system prompt") && v.ToString()!.Contains("test")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task GetSystemPromptAsync_WithVeryLargePromptFile_HandlesSuccessfully()
    {
        // Arrange
        var largeContent = new string('A', 100_000); // 100KB of content
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), largeContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest { Message = "test" };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().HaveLength(100_000);
        result.Should().Be(largeContent);
    }

    [Fact]
    public async Task GetSystemPromptAsync_WithEmptyDefaultPromptFile_ReturnsEmptyString()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), "");

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest { Message = "test" };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSystemPromptAsync_WithSpecialCharactersInIdentifier_ConstructsCorrectFilePath()
    {
        // Arrange
        var defaultPrompt = "Default";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPrompt);
        
        // Create a file with special but valid characters
        var specialIdentifier = "code-analysis-v2";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, $"system-{specialIdentifier}.md"), "Special");

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        var request = new ChatRequest 
        { 
            Message = "test",
            SystemPromptIdentifier = specialIdentifier
        };

        // Act
        var result = await service.GetSystemPromptAsync(request);

        // Assert
        result.Should().Contain("Default");
        result.Should().Contain("Special");
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task GetSystemPromptAsync_CalledConcurrently_HandlesMultipleRequestsSafely()
    {
        // Arrange
        var defaultPromptContent = "Concurrent test prompt";
        await File.WriteAllTextAsync(Path.Combine(_promptsDirectory, "system-default.md"), defaultPromptContent);

        _mockConfiguration.Setup(c => c["PROMPTS_BASE_DIRECTORY"]).Returns(_tempDirectory);
        var service = new SystemPromptService(_mockConfiguration.Object, _mockLogger.Object);
        
        var requests = Enumerable.Range(0, 10).Select(i => new ChatRequest 
        { 
            Message = $"test {i}" 
        }).ToList();

        // Act
        var tasks = requests.Select(r => service.GetSystemPromptAsync(r));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.Should().Be(defaultPromptContent));
        results.Should().HaveCount(10);
    }

    #endregion
}

