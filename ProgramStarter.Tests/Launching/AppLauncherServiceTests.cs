using System.Diagnostics;
using ProgramStarter.App.Models;
using ProgramStarter.App.Services;
using ProgramStarter.Tests.GroupManagement;

namespace ProgramStarter.Tests.Launching;

/// <summary>
/// Unit tests for <see cref="AppLauncherService"/>.
/// Uses a fake <see cref="IProcessStarter"/> and fake <see cref="IPathValidationService"/>
/// to verify launch logic in isolation.
/// </summary>
public class AppLauncherServiceTests
{
    // ──────────────────────────────────────────────
    //  LaunchOneAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task LaunchOneAsync_ValidApp_ReturnsSuccess()
    {
        // Arrange
        var app = new AppEntry
        {
            Id = "a1",
            Name = "TestApp",
            Path = @"C:\valid\app.exe",
            IsEnabled = true
        };

        var fakeProcess = new FakeProcessStarter();
        var fakeValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForLaunchResult = (true, string.Empty)
        };

        var service = new AppLauncherService(fakeProcess, fakeValidation, new FakeLogger());

        // Act
        var result = await service.LaunchOneAsync(app);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("TestApp", result.AppName);
        Assert.Equal(LaunchErrorCode.None, result.ErrorCode);
    }

    [Fact]
    public async Task LaunchOneAsync_EmptyPath_ReturnsEmptyPathError()
    {
        // Arrange
        var app = new AppEntry
        {
            Id = "a1",
            Name = "NoPath",
            Path = "",
            IsEnabled = true
        };

        var fakeProcess = new FakeProcessStarter();
        var fakeValidation = new FakePathValidationService
        {
            ValidateForLaunchResult = (false, "Path is empty."),
            IsSupportedExtensionResult = false,
            FileExistsResult = false
        };

        var service = new AppLauncherService(fakeProcess, fakeValidation, new FakeLogger());

        // Act
        var result = await service.LaunchOneAsync(app);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(LaunchErrorCode.EmptyPath, result.ErrorCode);
        Assert.Contains("Cannot launch", result.UserMessage);
    }

    [Fact]
    public async Task LaunchOneAsync_UnsupportedExtension_ReturnsUnsupportedFileTypeError()
    {
        // Arrange
        var app = new AppEntry
        {
            Id = "a1",
            Name = "Script",
            Path = @"C:\script.dll",
            IsEnabled = true
        };

        var fakeProcess = new FakeProcessStarter();
        var fakeValidation = new FakePathValidationService
        {
            ValidateForLaunchResult = (false, "Only .exe files are supported."),
            IsSupportedExtensionResult = false,
            FileExistsResult = true
        };

        var service = new AppLauncherService(fakeProcess, fakeValidation, new FakeLogger());

        // Act
        var result = await service.LaunchOneAsync(app);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(LaunchErrorCode.UnsupportedFileType, result.ErrorCode);
    }

    [Fact]
    public async Task LaunchOneAsync_MissingFile_ReturnsFileNotFoundError()
    {
        // Arrange
        var app = new AppEntry
        {
            Id = "a1",
            Name = "Missing",
            Path = @"C:\missing.exe",
            IsEnabled = true
        };

        var fakeProcess = new FakeProcessStarter();
        var fakeValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = false,
            ValidateForLaunchResult = (false, "File does not exist.")
        };

        var service = new AppLauncherService(fakeProcess, fakeValidation, new FakeLogger());

        // Act
        var result = await service.LaunchOneAsync(app);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(LaunchErrorCode.FileNotFound, result.ErrorCode);
    }

    [Fact]
    public async Task LaunchOneAsync_ProcessStartReturnsNull_ReturnsAccessDenied()
    {
        // Arrange
        var app = new AppEntry
        {
            Id = "a1",
            Name = "Blocked",
            Path = @"C:\blocked.exe",
            IsEnabled = true
        };

        // ProcessStarter returns null with AccessDenied error code
        var fakeProcess = new FakeProcessStarter { StartResult = null, ErrorCode = LaunchErrorCode.AccessDenied };
        var fakeValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForLaunchResult = (true, string.Empty)
        };

        var service = new AppLauncherService(fakeProcess, fakeValidation, new FakeLogger());

        // Act
        var result = await service.LaunchOneAsync(app);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(LaunchErrorCode.AccessDenied, result.ErrorCode);
        Assert.Contains("Access denied", result.UserMessage);
    }

    [Fact]
    public async Task LaunchOneAsync_ProcessStartReturnsNull_ReturnsFileNotFound()
    {
        // Arrange
        var app = new AppEntry
        {
            Id = "a1",
            Name = "Missing",
            Path = @"C:\missing.exe",
            IsEnabled = true
        };

        // ProcessStarter returns null with FileNotFound error code
        var fakeProcess = new FakeProcessStarter { StartResult = null, ErrorCode = LaunchErrorCode.FileNotFound };
        var fakeValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForLaunchResult = (true, string.Empty)
        };

        var service = new AppLauncherService(fakeProcess, fakeValidation, new FakeLogger());

        // Act
        var result = await service.LaunchOneAsync(app);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(LaunchErrorCode.FileNotFound, result.ErrorCode);
        Assert.Contains("File not found", result.UserMessage);
    }

    [Fact]
    public async Task LaunchOneAsync_ProcessStartThrows_ReturnsUnknown()
    {
        // Arrange
        var app = new AppEntry
        {
            Id = "a1",
            Name = "Crashy",
            Path = @"C:\crashy.exe",
            IsEnabled = true
        };

        // ProcessStarter returns null with Unknown error code (simulates unexpected exception)
        var fakeProcess = new FakeProcessStarter { StartResult = null, ErrorCode = LaunchErrorCode.Unknown };
        var fakeValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForLaunchResult = (true, string.Empty)
        };

        var service = new AppLauncherService(fakeProcess, fakeValidation, new FakeLogger());

        // Act
        var result = await service.LaunchOneAsync(app);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(LaunchErrorCode.Unknown, result.ErrorCode);
    }

    // ──────────────────────────────────────────────
    //  LaunchGroupAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task LaunchGroupAsync_AllEnabled_StartsWithDelay()
    {
        // Arrange
        var app1 = new AppEntry { Id = "a1", Name = "App1", Path = @"C:\app1.exe", IsEnabled = true };
        var app2 = new AppEntry { Id = "a2", Name = "App2", Path = @"C:\app2.exe", IsEnabled = true };
        var group = new AppGroup
        {
            Id = "g1",
            Name = "TestGroup",
            Apps = new List<AppEntry> { app1, app2 }
        };

        var fakeProcess = new FakeProcessStarter();
        var fakeValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForLaunchResult = (true, string.Empty)
        };

        var service = new AppLauncherService(fakeProcess, fakeValidation, new FakeLogger());

        // Act
        var results = await service.LaunchGroupAsync(group, delayMilliseconds: 10);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.True(results.All(r => r.Success));
    }

    [Fact]
    public async Task LaunchGroupAsync_SkipsDisabledApps()
    {
        // Arrange
        var app1 = new AppEntry { Id = "a1", Name = "App1", Path = @"C:\app1.exe", IsEnabled = true };
        var app2 = new AppEntry { Id = "a2", Name = "App2", Path = @"C:\app2.exe", IsEnabled = false }; // disabled
        var app3 = new AppEntry { Id = "a3", Name = "App3", Path = @"C:\app3.exe", IsEnabled = true };
        var group = new AppGroup
        {
            Id = "g1",
            Name = "TestGroup",
            Apps = new List<AppEntry> { app1, app2, app3 }
        };

        var fakeProcess = new FakeProcessStarter();
        var fakeValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForLaunchResult = (true, string.Empty)
        };

        var service = new AppLauncherService(fakeProcess, fakeValidation, new FakeLogger());

        // Act
        var results = await service.LaunchGroupAsync(group, delayMilliseconds: 0);

        // Assert
        Assert.Equal(2, results.Count); // only 2 enabled apps
        Assert.Equal("App1", results[0].AppName);
        Assert.Equal("App3", results[1].AppName);
    }

    [Fact]
    public async Task LaunchGroupAsync_EmptyGroup_ReturnsEmptyList()
    {
        // Arrange
        var group = new AppGroup
        {
            Id = "g1",
            Name = "EmptyGroup",
            Apps = new List<AppEntry>()
        };

        var service = new AppLauncherService(
            new FakeProcessStarter(),
            new FakePathValidationService(),
            new FakeLogger());

        // Act
        var results = await service.LaunchGroupAsync(group, delayMilliseconds: 100);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task LaunchGroupAsync_AllDisabled_ReturnsEmptyList()
    {
        // Arrange
        var app1 = new AppEntry { Id = "a1", Name = "App1", Path = @"C:\app1.exe", IsEnabled = false };
        var app2 = new AppEntry { Id = "a2", Name = "App2", Path = @"C:\app2.exe", IsEnabled = false };
        var group = new AppGroup
        {
            Id = "g1",
            Name = "TestGroup",
            Apps = new List<AppEntry> { app1, app2 }
        };

        var service = new AppLauncherService(
            new FakeProcessStarter(),
            new FakePathValidationService(),
            new FakeLogger());

        // Act
        var results = await service.LaunchGroupAsync(group, delayMilliseconds: 0);

        // Assert
        Assert.Empty(results);
    }
}

// ──────────────────────────────────────────────
//  Fake Implementations
// ──────────────────────────────────────────────

internal class FakeProcessStarter : IProcessStarter
{
    /// <summary>
    /// The process to return from <see cref="Start"/>.
    /// Defaults to a non-null placeholder.
    /// </summary>
    public Process? StartResult { get; set; } = new Process();
    public App.Models.LaunchErrorCode? ErrorCode { get; set; }

    public (Process? Process, App.Models.LaunchErrorCode? Error) Start(ProcessStartInfo startInfo)
        => (StartResult, ErrorCode);
}
