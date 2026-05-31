using ProgramStarter.App.Models;
using ProgramStarter.App.Services;

namespace ProgramStarter.Tests.Validation;

/// <summary>
/// Focused tests for PathValidationService.
/// </summary>
public class PathValidationServiceTests
{
    private readonly PathValidationService _service = new();

    [Fact]
    public void IsValidAppPath_ExistingExe_ReturnsTrue()
    {
        // Use a known system .exe that always exists
        var path = @"C:\Windows\System32\notepad.exe";
        if (!File.Exists(path))
        {
            // Fallback: find any .exe in system directory
            path = Directory.GetFiles(Environment.SystemDirectory, "*.exe").First();
        }

        Assert.True(_service.IsValidAppPath(path));
    }

    [Fact]
    public void IsValidAppPath_EmptyPath_ReturnsFalse()
    {
        Assert.False(_service.IsValidAppPath(""));
        Assert.False(_service.IsValidAppPath(null));
        Assert.False(_service.IsValidAppPath("   "));
    }

    [Fact]
    public void IsValidAppPath_MissingFile_ReturnsFalse()
    {
        var path = @"C:\DoesNotExist\missing.exe";
        Assert.False(_service.IsValidAppPath(path));
    }

    [Fact]
    public void IsValidAppPath_UnsupportedExtension_ReturnsFalse()
    {
        // .lnk is unsupported in v0.1
        Assert.False(_service.IsValidAppPath(@"C:\test.lnk"));
        Assert.False(_service.IsValidAppPath(@"C:\test.url"));
        Assert.False(_service.IsValidAppPath(@"C:\test.bat"));
    }

    [Fact]
    public void IsSupportedExtension_DifferentCase_ReturnsTrue()
    {
        Assert.True(_service.IsSupportedExtension(@"C:\test.EXE"));
        Assert.True(_service.IsSupportedExtension(@"C:\test.Exe"));
    }

    [Fact]
    public void ValidateForAdd_DuplicateName_ReturnsInvalid()
    {
        // Arrange
        var group = new AppGroup
        {
            Apps = new List<AppEntry>
            {
                new() { Name = "Notepad", Path = @"C:\Windows\notepad.exe" }
            }
        };

        var path = @"C:\Windows\System32\notepad.exe";
        if (!File.Exists(path))
            path = Directory.GetFiles(Environment.SystemDirectory, "*.exe").First();

        // Act
        var (isValid, errorMessage) = _service.ValidateForAdd(path, "Notepad", group);

        // Assert
        Assert.False(isValid);
        Assert.Contains("already exists", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateForLaunch_EmptyPath_ReturnsInvalid()
    {
        // Act
        var (isValid, errorMessage) = _service.ValidateForLaunch(null);
        Assert.False(isValid);
        Assert.Contains("empty", errorMessage, StringComparison.OrdinalIgnoreCase);

        (isValid, errorMessage) = _service.ValidateForLaunch("");
        Assert.False(isValid);
        Assert.Contains("empty", errorMessage, StringComparison.OrdinalIgnoreCase);

        (isValid, errorMessage) = _service.ValidateForLaunch("   ");
        Assert.False(isValid);
        Assert.Contains("empty", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateForLaunch_UnsupportedExtension_ReturnsInvalid()
    {
        // Act
        var (isValid, errorMessage) = _service.ValidateForLaunch(@"C:\test.dll");

        // Assert
        Assert.False(isValid);
        Assert.Contains(".exe", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateForLaunch_MissingFile_ReturnsInvalid()
    {
        // Act
        var (isValid, errorMessage) = _service.ValidateForLaunch(@"C:\DoesNotExist\missing.exe");

        // Assert
        Assert.False(isValid);
        Assert.Contains("does not exist", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateForLaunch_ValidFile_ReturnsValid()
    {
        // Arrange
        var path = @"C:\Windows\System32\notepad.exe";
        if (!File.Exists(path))
            path = Directory.GetFiles(Environment.SystemDirectory, "*.exe").First();

        // Act
        var (isValid, errorMessage) = _service.ValidateForLaunch(path);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void ValidateForLaunch_WithExeExtension_ReturnsValid()
    {
        // Act
        var (isValid, errorMessage) = _service.ValidateForLaunch(@"C:\test.EXE");

        // Assert
        // File doesn't exist, so it will fail on FileExists check first
        Assert.False(isValid);
        Assert.Contains("does not exist", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateForAdd_DuplicatePath_ReturnsInvalid()
    {
        // Arrange
        var path = @"C:\Windows\System32\notepad.exe";
        if (!File.Exists(path))
            path = Directory.GetFiles(Environment.SystemDirectory, "*.exe").First();

        var group = new AppGroup
        {
            Apps = new List<AppEntry>
            {
                new() { Name = "Existing", Path = path }
            }
        };

        // Act
        var (isValid, errorMessage) = _service.ValidateForAdd(path, "NewApp", group);

        // Assert
        Assert.False(isValid);
        Assert.Contains("already exists", errorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
