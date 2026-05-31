using System.Text.Json;

namespace ProgramStarter.App.Helpers;

/// <summary>
/// Provides shared <see cref="JsonSerializerOptions"/> for config serialization.
/// Uses camelCase, indented output, and lenient reading (skip comments, allow trailing commas).
/// </summary>
public static class JsonOptions
{
    /// <summary>
    /// Read-write options: camelCase naming, indented output, skip comments, allow trailing commas.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}
