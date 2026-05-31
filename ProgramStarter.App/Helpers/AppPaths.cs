using System.IO;

namespace ProgramStarter.App.Helpers;

/// <summary>
/// Provides well-known application paths under %LocalAppData%/ProgramStarter.
/// All paths are computed once at class initialization for consistency.
/// </summary>
public static class AppPaths
{
    private static string? _baseDirectory;

    private static string BaseDirectory => _baseDirectory ??= ComputeBaseDirectory();

    /// <summary>
    /// The root application data directory: %LocalAppData%/ProgramStarter
    /// </summary>
    public static string AppDataDirectory => BaseDirectory;

    /// <summary>
    /// Full path to the config file: %LocalAppData%/ProgramStarter/config.json
    /// </summary>
    public static string ConfigFilePath => Path.Combine(BaseDirectory, Constants.ConfigFileName);

    /// <summary>
    /// Full path to the logs directory: %LocalAppData%/ProgramStarter/logs
    /// </summary>
    public static string LogsDirectory => Path.Combine(BaseDirectory, Constants.LogsDirectory);

    /// <summary>
    /// Full path to the current log file: %LocalAppData%/ProgramStarter/logs/app.log
    /// </summary>
    public static string LogFilePath => Path.Combine(LogsDirectory, Constants.LogFileName);

    /// <summary>
    /// Ensures that the app data directory and logs directory exist.
    /// Creates them if they don't already exist.
    /// </summary>
    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(BaseDirectory);
        Directory.CreateDirectory(LogsDirectory);
    }

    private static string ComputeBaseDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Constants.ConfigDirectory);
    }

    /// <summary>
    /// Redirects AppPaths to use the specified root directory instead of
    /// the real %LocalAppData%/ProgramStarter path. Intended for testing.
    /// Call <see cref="ResetRoot"/> to return to the real path.
    /// </summary>
    internal static void SetTestModeRoot(string rootPath)
    {
        _baseDirectory = rootPath;
    }

    /// <summary>
    /// Resets AppPaths to use the real %LocalAppData%/ProgramStarter path.
    /// </summary>
    internal static void ResetRoot()
    {
        _baseDirectory = null;
    }
}
