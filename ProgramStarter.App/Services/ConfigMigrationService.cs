using System.Text.Json;
using ProgramStarter.App.Helpers;
using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

/// <summary>
/// Exception thrown when the loaded config has a schema version higher than the app supports.
/// </summary>
public class UnsupportedSchemaException : InvalidOperationException
{
    public int ConfigSchemaVersion { get; }
    public int SupportedSchemaVersion { get; }

    public UnsupportedSchemaException(int configVersion, int supportedVersion)
        : base($"Config schema version {configVersion} is not supported. " +
               $"This app supports schema version {supportedVersion}.")
    {
        ConfigSchemaVersion = configVersion;
        SupportedSchemaVersion = supportedVersion;
    }
}

/// <summary>
/// Handles migration and deserialization of raw JSON config documents.
/// In v0.1 only schema version 1 is supported.
/// </summary>
internal class ConfigMigrationService : IConfigMigrationService
{
    /// <summary>
    /// Deserializes and migrates the raw config JSON document to <see cref="AppSettings"/>.
    /// </summary>
    /// <param name="rawConfig">The raw JSON document to migrate.</param>
    /// <returns>Deserialized and normalized <see cref="AppSettings"/>.</returns>
    /// <exception cref="UnsupportedSchemaException">
    /// Thrown if the schema version is higher than the app supports.
    /// </exception>
    public AppSettings Migrate(JsonDocument rawConfig)
    {
        var schemaVersion = GetSchemaVersion(rawConfig);

        // Future schema: reject and do not overwrite
        if (schemaVersion > Constants.SupportedSchemaVersion)
        {
            throw new UnsupportedSchemaException(
                schemaVersion, Constants.SupportedSchemaVersion);
        }

        // Schema version 1 (or missing, treated as 1): standard deserialization
        var settings = JsonSerializer.Deserialize<AppSettings>(
            rawConfig.RootElement.GetRawText(), JsonOptions.Default);

        settings ??= new AppSettings();

        // Normalize according to roadmap Section 13.8
        Normalize(settings);

        return settings;
    }

    private static void Normalize(AppSettings settings)
    {
        // Ensure Groups is never null
        settings.Groups ??= new List<AppGroup>();

        // Normalize DefaultDelayMilliseconds
        if (settings.DefaultDelayMilliseconds < 0)
        {
            settings.DefaultDelayMilliseconds = Constants.DefaultDelayMilliseconds;
        }

        // Normalize Theme
        if (string.IsNullOrWhiteSpace(settings.Theme))
        {
            settings.Theme = "Dark";
        }

        // Build set of valid group IDs for LastSelectedGroupId validation
        var validGroupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in settings.Groups)
        {
            // Normalize group ID
            if (string.IsNullOrWhiteSpace(group.Id))
            {
                group.Id = Guid.NewGuid().ToString();
            }

            validGroupIds.Add(group.Id);

            // Trim group name
            group.Name = group.Name?.Trim() ?? string.Empty;

            // Default group name if empty
            if (group.Name.Length == 0)
            {
                group.Name = "New Group";
            }

            // Ensure group Apps is never null
            group.Apps ??= new List<AppEntry>();

            foreach (var app in group.Apps)
            {
                // Normalize app ID
                if (string.IsNullOrWhiteSpace(app.Id))
                {
                    app.Id = Guid.NewGuid().ToString();
                }

                // Trim app name
                app.Name = app.Name?.Trim() ?? string.Empty;

                // Default app name if empty
                if (app.Name.Length == 0)
                {
                    app.Name = "New App";
                }
            }
        }

        // Clear orphaned LastSelectedGroupId
        if (settings.LastSelectedGroupId is not null &&
            !validGroupIds.Contains(settings.LastSelectedGroupId))
        {
            settings.LastSelectedGroupId = null;
        }
    }

    private static int GetSchemaVersion(JsonDocument rawConfig)
    {
        if (rawConfig.RootElement.TryGetProperty("schemaVersion", out var versionElement))
        {
            return versionElement.GetInt32();
        }

        // Also check camelCase property name
        if (rawConfig.RootElement.TryGetProperty("SchemaVersion", out var pascalElement))
        {
            return pascalElement.GetInt32();
        }

        // Missing schema version: treat as version 1
        return 1;
    }
}
