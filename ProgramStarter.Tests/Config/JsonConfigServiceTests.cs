using System.Text.Json;
using ProgramStarter.App.Helpers;
using ProgramStarter.App.Models;
using ProgramStarter.App.Services;

namespace ProgramStarter.Tests.Config;

/// <summary>
/// Focused tests for JsonConfigService covering:
/// - Missing config creates default and saves it
/// - Save/load round-trip preserves data
/// - Corrupted config is backed up and replaced with default
/// - Future schema version is not overwritten
/// - Null groups are normalized to empty list
/// - Full normalization: null Apps, empty IDs, blank names, negative delay, empty theme, orphaned group ref
/// - Save writes valid camelCase JSON
/// - Logger is called on key events
/// </summary>
public class JsonConfigServiceTests : IDisposable
{
    private readonly string _testRoot;

    public JsonConfigServiceTests()
    {
        // Each test gets its own isolated temp directory
        _testRoot = Path.Combine(Path.GetTempPath(), "ProgramStarter_Tests_" + Guid.NewGuid().ToString("N"));
        AppPaths.SetTestModeRoot(_testRoot);
    }

    public void Dispose()
    {
        AppPaths.ResetRoot();

        // Clean up test directory
        if (Directory.Exists(_testRoot))
        {
            try { Directory.Delete(_testRoot, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }

    [Fact]
    public void Load_WhenConfigMissing_CreatesDefaultAndSaves()
    {
        // Arrange
        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert - default settings
        Assert.NotNull(settings);
        Assert.Equal(1, settings.SchemaVersion);
        Assert.Empty(settings.Groups);
        Assert.Null(settings.LastSelectedGroupId);
        Assert.Equal("Dark", settings.Theme);
        Assert.Equal(1000, settings.DefaultDelayMilliseconds);

        // Assert - config file was created
        Assert.True(File.Exists(AppPaths.ConfigFilePath));

        // Assert - logger was called
        Assert.Contains(logger.Messages, m => m.Contains("No config file found"));
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesData()
    {
        // Arrange
        using var logger = new TestLogger();
        var service = CreateService(logger);

        var group = new AppGroup
        {
            Id = "test-group-id",
            Name = "Test Group",
            Apps = new List<AppEntry>
            {
                new()
                {
                    Id = "test-app-id",
                    Name = "Test App",
                    Path = @"C:\Program Files\Test\test.exe",
                    IsEnabled = true
                }
            }
        };

        var settings = new AppSettings
        {
            SchemaVersion = 1,
            Groups = new List<AppGroup> { group },
            LastSelectedGroupId = "test-group-id",
            Theme = "Dark",
            DefaultDelayMilliseconds = 500
        };

        // Act
        service.Save(settings);
        var loaded = service.Load();

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(1, loaded.SchemaVersion);
        Assert.Single(loaded.Groups);
        Assert.Equal("Test Group", loaded.Groups[0].Name);
        Assert.Single(loaded.Groups[0].Apps);
        Assert.Equal("Test App", loaded.Groups[0].Apps[0].Name);
        Assert.Equal(@"C:\Program Files\Test\test.exe", loaded.Groups[0].Apps[0].Path);
        Assert.True(loaded.Groups[0].Apps[0].IsEnabled);
        Assert.Equal("test-group-id", loaded.LastSelectedGroupId);
        Assert.Equal("Dark", loaded.Theme);
        Assert.Equal(500, loaded.DefaultDelayMilliseconds);
    }

    [Fact]
    public void Load_WhenConfigCorrupted_BacksUpAndCreatesDefault()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        File.WriteAllText(AppPaths.ConfigFilePath, "This is not valid JSON {{{");

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert - default settings returned
        Assert.NotNull(settings);
        Assert.Equal(1, settings.SchemaVersion);
        Assert.Empty(settings.Groups);

        // Assert - corrupted file was backed up (some .corrupted_ file exists)
        var backupFiles = Directory.GetFiles(AppPaths.AppDataDirectory, "config.corrupted_*");
        Assert.NotEmpty(backupFiles);

        // Assert - new valid config was written
        Assert.True(File.Exists(AppPaths.ConfigFilePath));
        var reloaded = JsonSerializer.Deserialize<AppSettings>(
            File.ReadAllText(AppPaths.ConfigFilePath), JsonOptions.Default);
        Assert.NotNull(reloaded);

        // Assert - logger was called
        Assert.Contains(logger.Messages, m => m.Contains("Config file is corrupted"));
    }

    [Fact]
    public void Load_WhenFutureSchemaVersion_DoesNotOverwrite()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var futureConfig = new AppSettings
        {
            SchemaVersion = 999,
            Groups = new List<AppGroup>
            {
                new() { Id = "future-group", Name = "Future Group" }
            }
        };
        var json = JsonSerializer.Serialize(futureConfig, JsonOptions.Default);
        File.WriteAllText(AppPaths.ConfigFilePath, json);
        var originalContent = File.ReadAllText(AppPaths.ConfigFilePath);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert - returned empty in-memory settings
        Assert.NotNull(settings);
        Assert.Equal(1, settings.SchemaVersion);
        Assert.Empty(settings.Groups);

        // Assert - original config file was NOT overwritten
        var afterContent = File.ReadAllText(AppPaths.ConfigFilePath);
        Assert.Equal(originalContent, afterContent);
        Assert.Contains("schemaVersion", afterContent);
        Assert.Contains("999", afterContent);

        // Assert - logger warning was called
        Assert.Contains(logger.Messages, m => m.Contains("unsupported schema version"));
    }

    [Fact]
    public void Load_WhenNullGroups_NormalizesToEmpty()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var settingsWithNullGroups = new Dictionary<string, object?>
        {
            ["schemaVersion"] = 1,
            ["groups"] = null,
            ["lastSelectedGroupId"] = null,
            ["theme"] = "Dark",
            ["defaultDelayMilliseconds"] = 1000
        };
        var json = JsonSerializer.Serialize(settingsWithNullGroups, JsonOptions.Default);
        File.WriteAllText(AppPaths.ConfigFilePath, json);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.Groups);
        Assert.Empty(settings.Groups);
    }

    [Fact]
    public void Load_WhenGroupHasNullApps_NormalizesToEmptyList()
    {
        // Arrange: write JSON where "apps" is explicitly null
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var configJson = /*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [
                    {
                        "id": "group-1",
                        "name": "Group With Null Apps",
                        "apps": null
                    }
                ],
                "lastSelectedGroupId": null,
                "theme": "Dark",
                "defaultDelayMilliseconds": 1000
            }
            """;
        File.WriteAllText(AppPaths.ConfigFilePath, configJson);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert
        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.NotNull(settings.Groups[0].Apps);
        Assert.Empty(settings.Groups[0].Apps);
    }

    [Fact]
    public void Load_WhenGroupIdEmpty_GeneratesNewId()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var configJson = /*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [
                    {
                        "id": "",
                        "name": "Nameless Group",
                        "apps": []
                    }
                ],
                "lastSelectedGroupId": null,
                "theme": "Dark",
                "defaultDelayMilliseconds": 1000
            }
            """;
        File.WriteAllText(AppPaths.ConfigFilePath, configJson);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert
        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.False(string.IsNullOrWhiteSpace(settings.Groups[0].Id));
        // Should not be the empty string we wrote; should be a new GUID
        Assert.NotEqual("", settings.Groups[0].Id);
    }

    [Fact]
    public void Load_WhenAppIdEmpty_GeneratesNewId()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var configJson = /*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [
                    {
                        "id": "group-1",
                        "name": "Group",
                        "apps": [
                            {
                                "id": "",
                                "name": "App With Empty Id",
                                "path": "C:\\test.exe",
                                "isEnabled": true
                            }
                        ]
                    }
                ],
                "lastSelectedGroupId": null,
                "theme": "Dark",
                "defaultDelayMilliseconds": 1000
            }
            """;
        File.WriteAllText(AppPaths.ConfigFilePath, configJson);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert
        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.Single(settings.Groups[0].Apps);
        Assert.False(string.IsNullOrWhiteSpace(settings.Groups[0].Apps[0].Id));
        Assert.NotEqual("", settings.Groups[0].Apps[0].Id);
    }

    [Fact]
    public void Load_WhenGroupNameBlank_NormalizesToNewGroup()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var configJson = /*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [
                    {
                        "id": "group-1",
                        "name": "   ",
                        "apps": []
                    }
                ],
                "lastSelectedGroupId": null,
                "theme": "Dark",
                "defaultDelayMilliseconds": 1000
            }
            """;
        File.WriteAllText(AppPaths.ConfigFilePath, configJson);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert
        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.Equal("New Group", settings.Groups[0].Name);
    }

    [Fact]
    public void Load_WhenAppNameBlank_NormalizesToNewApp()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var configJson = /*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [
                    {
                        "id": "group-1",
                        "name": "Group",
                        "apps": [
                            {
                                "id": "app-1",
                                "name": "",
                                "path": "C:\\test.exe",
                                "isEnabled": true
                            }
                        ]
                    }
                ],
                "lastSelectedGroupId": null,
                "theme": "Dark",
                "defaultDelayMilliseconds": 1000
            }
            """;
        File.WriteAllText(AppPaths.ConfigFilePath, configJson);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert
        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.Single(settings.Groups[0].Apps);
        Assert.Equal("New App", settings.Groups[0].Apps[0].Name);
    }

    [Fact]
    public void Load_WhenDefaultDelayNegative_NormalizesTo1000()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var configJson = /*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [],
                "lastSelectedGroupId": null,
                "theme": "Dark",
                "defaultDelayMilliseconds": -500
            }
            """;
        File.WriteAllText(AppPaths.ConfigFilePath, configJson);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(1000, settings.DefaultDelayMilliseconds);
    }

    [Fact]
    public void Load_WhenThemeEmpty_NormalizesToDark()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var configJson = /*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [],
                "lastSelectedGroupId": null,
                "theme": "",
                "defaultDelayMilliseconds": 1000
            }
            """;
        File.WriteAllText(AppPaths.ConfigFilePath, configJson);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("Dark", settings.Theme);
    }

    [Fact]
    public void Load_WhenLastSelectedGroupIdOrphaned_ClearsToNull()
    {
        // Arrange
        Directory.CreateDirectory(AppPaths.AppDataDirectory);
        var configJson = /*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [
                    {
                        "id": "group-a",
                        "name": "Group A",
                        "apps": []
                    }
                ],
                "lastSelectedGroupId": "non-existent-group",
                "theme": "Dark",
                "defaultDelayMilliseconds": 1000
            }
            """;
        File.WriteAllText(AppPaths.ConfigFilePath, configJson);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        // Act
        var settings = service.Load();

        // Assert
        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.Null(settings.LastSelectedGroupId);
    }

    [Fact]
    public void Save_WritesValidCamelCaseJson()
    {
        // Arrange
        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = new AppSettings
        {
            SchemaVersion = 1,
            Groups = new List<AppGroup>(),
            LastSelectedGroupId = null,
            Theme = "Dark",
            DefaultDelayMilliseconds = 1000
        };

        // Act
        service.Save(settings);

        // Assert - file exists and is valid camelCase JSON
        Assert.True(File.Exists(AppPaths.ConfigFilePath));
        var rawJson = File.ReadAllText(AppPaths.ConfigFilePath);
        using var doc = JsonDocument.Parse(rawJson);

        // Verify camelCase property names
        Assert.True(doc.RootElement.TryGetProperty("schemaVersion", out _));
        Assert.True(doc.RootElement.TryGetProperty("groups", out _));
        Assert.True(doc.RootElement.TryGetProperty("lastSelectedGroupId", out _));
        Assert.True(doc.RootElement.TryGetProperty("theme", out _));
        Assert.True(doc.RootElement.TryGetProperty("defaultDelayMilliseconds", out _));
    }

    private static JsonConfigService CreateService(TestLogger logger)
    {
        return new JsonConfigService(
            new ConfigMigrationService(),
            logger);
    }
}

/// <summary>
/// In-memory logger implementation for testing.
/// Captures log messages so tests can verify logger was called.
/// </summary>
internal class TestLogger : IAppLogger, IDisposable
{
    private readonly List<string> _messages = new();
    private readonly object _lock = new();

    public IReadOnlyList<string> Messages
    {
        get { lock (_lock) return _messages.ToList(); }
    }

    public void Info(string message)
    {
        lock (_lock) _messages.Add($"[INFO] {message}");
    }

    public void Warning(string message)
    {
        lock (_lock) _messages.Add($"[WARN] {message}");
    }

    public void Error(string message, Exception? exception = null)
    {
        lock (_lock) _messages.Add($"[ERROR] {message}");
    }

    public void Dispose()
    {
        _messages.Clear();
    }
}
