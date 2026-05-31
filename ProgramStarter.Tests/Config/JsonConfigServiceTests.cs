using System.Text.Json;
using ProgramStarter.App.Helpers;
using ProgramStarter.App.Models;
using ProgramStarter.App.Services;

namespace ProgramStarter.Tests.Config;

[CollectionDefinition("ConfigTests", DisableParallelization = true)]
public sealed class ConfigTestsCollection
{
}

/// <summary>
/// Focused tests for JsonConfigService...
/// </summary>
[Collection("ConfigTests")]
public class JsonConfigServiceTests : IDisposable
{
    private readonly string _testRoot;

    public JsonConfigServiceTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "ProgramStarter_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testRoot);
        AppPaths.SetTestModeRoot(_testRoot);
    }

    public void Dispose()
    {
        AppPaths.ResetRoot();

        if (Directory.Exists(_testRoot))
        {
            try { Directory.Delete(_testRoot, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }

    [Fact]
    public void Load_WhenConfigMissing_CreatesDefaultAndSaves()
    {
        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Equal(1, settings.SchemaVersion);
        Assert.Empty(settings.Groups);
        Assert.Null(settings.LastSelectedGroupId);
        Assert.Equal("Dark", settings.Theme);
        Assert.Equal(1000, settings.DefaultDelayMilliseconds);
        Assert.True(File.Exists(AppPaths.ConfigFilePath));
        Assert.Contains(logger.Messages, m => m.Contains("No config file found"));
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesData()
    {
        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = new AppSettings
        {
            SchemaVersion = 1,
            Groups = new List<AppGroup>
            {
                new()
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
                }
            },
            LastSelectedGroupId = "test-group-id",
            Theme = "Dark",
            DefaultDelayMilliseconds = 500
        };

        service.Save(settings);
        var loaded = service.Load();

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
        WriteConfigJson("This is not valid JSON {{{");

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Equal(1, settings.SchemaVersion);
        Assert.Empty(settings.Groups);
        Assert.NotEmpty(Directory.GetFiles(AppPaths.AppDataDirectory, "config.corrupted_*"));
        Assert.True(File.Exists(AppPaths.ConfigFilePath));
        Assert.NotNull(JsonSerializer.Deserialize<AppSettings>(ReadConfigJson(), JsonOptions.Default));
        Assert.Contains(logger.Messages, m => m.Contains("Config file is corrupted"));
    }

    [Fact]
    public void Load_WhenFutureSchemaVersion_DoesNotOverwrite()
    {
        var futureConfig = new AppSettings
        {
            SchemaVersion = 999,
            Groups = new List<AppGroup>
            {
                new() { Id = "future-group", Name = "Future Group" }
            }
        };
        WriteConfigJson(JsonSerializer.Serialize(futureConfig, JsonOptions.Default));
        var originalContent = ReadConfigJson();

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Equal(1, settings.SchemaVersion);
        Assert.Empty(settings.Groups);
        Assert.Equal(originalContent, ReadConfigJson());
        Assert.Contains("schemaVersion", ReadConfigJson());
        Assert.Contains("999", ReadConfigJson());
        Assert.Contains(logger.Messages, m => m.Contains("unsupported schema version"));
    }

    [Fact]
    public void Load_WhenNullGroups_NormalizesToEmpty()
    {
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["schemaVersion"] = 1,
            ["groups"] = null,
            ["lastSelectedGroupId"] = null,
            ["theme"] = "Dark",
            ["defaultDelayMilliseconds"] = 1000
        }, JsonOptions.Default);
        WriteConfigJson(json);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.NotNull(settings.Groups);
        Assert.Empty(settings.Groups);
    }

    [Fact]
    public void Load_WhenGroupHasNullApps_NormalizesToEmptyList()
    {
        WriteConfigJson(/*lang=json,strict*/ """
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
            """);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.NotNull(settings.Groups[0].Apps);
        Assert.Empty(settings.Groups[0].Apps);
    }

    [Fact]
    public void Load_WhenGroupIdEmpty_GeneratesNewId()
    {
        WriteConfigJson(/*lang=json,strict*/ """
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
            """);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.False(string.IsNullOrWhiteSpace(settings.Groups[0].Id));
        Assert.NotEqual("", settings.Groups[0].Id);
    }

    [Fact]
    public void Load_WhenAppIdEmpty_GeneratesNewId()
    {
        WriteConfigJson(/*lang=json,strict*/ """
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
            """);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.Single(settings.Groups[0].Apps);
        Assert.False(string.IsNullOrWhiteSpace(settings.Groups[0].Apps[0].Id));
        Assert.NotEqual("", settings.Groups[0].Apps[0].Id);
    }

    [Fact]
    public void Load_WhenGroupNameBlank_NormalizesToNewGroup()
    {
        WriteConfigJson(/*lang=json,strict*/ """
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
            """);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.Equal("New Group", settings.Groups[0].Name);
    }

    [Fact]
    public void Load_WhenAppNameBlank_NormalizesToNewApp()
    {
        WriteConfigJson(/*lang=json,strict*/ """
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
            """);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.Single(settings.Groups[0].Apps);
        Assert.Equal("New App", settings.Groups[0].Apps[0].Name);
    }

    [Fact]
    public void Load_WhenDefaultDelayNegative_NormalizesTo1000()
    {
        WriteConfigJson(/*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [],
                "lastSelectedGroupId": null,
                "theme": "Dark",
                "defaultDelayMilliseconds": -500
            }
            """);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Equal(1000, settings.DefaultDelayMilliseconds);
    }

    [Fact]
    public void Load_WhenThemeEmpty_NormalizesToDark()
    {
        WriteConfigJson(/*lang=json,strict*/ """
            {
                "schemaVersion": 1,
                "groups": [],
                "lastSelectedGroupId": null,
                "theme": "",
                "defaultDelayMilliseconds": 1000
            }
            """);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Equal("Dark", settings.Theme);
    }

    [Fact]
    public void Load_WhenLastSelectedGroupIdOrphaned_ClearsToNull()
    {
        WriteConfigJson(/*lang=json,strict*/ """
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
            """);

        using var logger = new TestLogger();
        var service = CreateService(logger);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Single(settings.Groups);
        Assert.Null(settings.LastSelectedGroupId);
    }

    [Fact]
    public void Save_WritesValidCamelCaseJson()
    {
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

        service.Save(settings);

        Assert.True(File.Exists(AppPaths.ConfigFilePath));
        using var doc = JsonDocument.Parse(ReadConfigJson());

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

    private static void WriteConfigJson(string json)
    {
        EnsureConfigDirectoryExists();
        File.WriteAllText(AppPaths.ConfigFilePath, json);
    }

    private static string ReadConfigJson()
    {
        return File.ReadAllText(AppPaths.ConfigFilePath);
    }

    private static void EnsureConfigDirectoryExists()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(AppPaths.ConfigFilePath)!);
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
