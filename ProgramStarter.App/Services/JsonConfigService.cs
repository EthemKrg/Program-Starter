using System.IO;
using System.Text.Json;
using ProgramStarter.App.Helpers;
using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

/// <summary>
/// JSON-based config service with atomic save, corrupted config backup,
/// future schema protection, and data normalization.
/// </summary>
internal class JsonConfigService : IConfigService
{
    private const string TempFileSuffix = ".tmp";
    private const string BackupFilePrefix = "config.corrupted_";
    private const string BackupFileSuffix = ".json";

    private static string TempFilePath => AppPaths.ConfigFilePath + TempFileSuffix;

    private readonly IConfigMigrationService _migrationService;
    private readonly IAppLogger _logger;

    public JsonConfigService(IConfigMigrationService migrationService, IAppLogger logger)
    {
        _migrationService = migrationService;
        _logger = logger;
    }

    /// <summary>
    /// Loads config from disk. If missing, creates default and saves it.
    /// If corrupted, backs up the corrupted file and creates default.
    /// If future schema version, does not overwrite; returns temporary empty settings.
    /// </summary>
    public AppSettings Load()
    {
        AppPaths.EnsureDirectoriesExist();

        // If no config exists, return default and save immediately
        if (!File.Exists(AppPaths.ConfigFilePath))
        {
            _logger.Info("No config file found. Creating default config.");
            var defaults = CreateDefaultSettings();
            SaveInternal(defaults);
            return defaults;
        }

        // Try to read and parse the config
        try
        {
            var rawJson = File.ReadAllText(AppPaths.ConfigFilePath);
            using var doc = JsonDocument.Parse(rawJson);
            return _migrationService.Migrate(doc);
        }
        catch (UnsupportedSchemaException ex)
        {
            // Future schema: do not overwrite, return empty in-memory settings
            _logger.Warning($"Config has unsupported schema version {ex.ConfigSchemaVersion}. " +
                            "The config was not modified. Returning temporary empty settings.");
            return CreateDefaultSettings();
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or IOException)
        {
            // Corrupted config: back it up and create fresh default
            _logger.Error("Config file is corrupted. Backing up and creating new default.", ex);
            BackupCorruptedConfig();
            var defaults = CreateDefaultSettings();
            SaveInternal(defaults);
            return defaults;
        }
    }

    /// <summary>
    /// Atomically saves settings to disk.
    /// Writes to a temp file first, then uses File.Replace (if config exists) or File.Move (if new).
    /// </summary>
    public void Save(AppSettings settings)
    {
        AppPaths.EnsureDirectoriesExist();
        SaveInternal(settings);
    }

    private void SaveInternal(AppSettings settings)
    {
        var serialized = JsonSerializer.Serialize(settings, JsonOptions.Default);
        var tempPath = TempFilePath;

        try
        {
            // Step 1: write to temp file
            File.WriteAllText(tempPath, serialized);

            // Step 2: atomically replace or move
            if (File.Exists(AppPaths.ConfigFilePath))
            {
                File.Replace(tempPath, AppPaths.ConfigFilePath, null);
            }
            else
            {
                File.Move(tempPath, AppPaths.ConfigFilePath);
            }

            _logger.Info("Config saved successfully.");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to save config.", ex);
            throw;
        }
        finally
        {
            // Clean up stale temp file if it still exists
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); }
                catch { /* best-effort cleanup */ }
            }
        }
    }

    private static AppSettings CreateDefaultSettings()
    {
        return new AppSettings
        {
            SchemaVersion = Constants.SupportedSchemaVersion,
            Groups = new List<AppGroup>(),
            LastSelectedGroupId = null,
            Theme = "Dark",
            DefaultDelayMilliseconds = Constants.DefaultDelayMilliseconds
        };
    }

    private void BackupCorruptedConfig()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupName = $"{BackupFilePrefix}{timestamp}{BackupFileSuffix}";
        var backupPath = Path.Combine(AppPaths.AppDataDirectory, backupName);

        try
        {
            File.Copy(AppPaths.ConfigFilePath, backupPath, overwrite: false);
            _logger.Info($"Corrupted config backed up to: {backupName}");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to back up corrupted config file.", ex);
        }
    }
}
