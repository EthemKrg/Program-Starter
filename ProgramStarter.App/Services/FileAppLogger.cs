using System.IO;
using ProgramStarter.App.Helpers;

namespace ProgramStarter.App.Services;

/// <summary>
/// Simple file-based logger that writes timestamped log entries to a log file.
/// Supports basic log rotation: when the file exceeds 1 MB, it is renamed with a
/// timestamp suffix and a new file is started. Keeps the latest 5 rotated files.
/// </summary>
internal class FileAppLogger : IAppLogger
{
    private readonly object _lock = new();

    public void Info(string message)
    {
        WriteLog("INFO", message, null);
    }

    public void Warning(string message)
    {
        WriteLog("WARN", message, null);
    }

    public void Error(string message, Exception? exception = null)
    {
        WriteLog("ERROR", message, exception);
    }

    private void WriteLog(string level, string message, Exception? exception)
    {
        lock (_lock)
        {
            try
            {
                AppPaths.EnsureDirectoriesExist();
                RotateIfNeeded();

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logLine = exception is not null
                    ? $"[{timestamp}] [{level}] {message}{Environment.NewLine}{exception}"
                    : $"[{timestamp}] [{level}] {message}";

                File.AppendAllText(AppPaths.LogFilePath, logLine + Environment.NewLine);
            }
            catch
            {
                // Logging must never crash the application
            }
        }
    }

    private void RotateIfNeeded()
    {
        var logFile = AppPaths.LogFilePath;

        if (!File.Exists(logFile))
            return;

        var fileInfo = new FileInfo(logFile);
        if (fileInfo.Length < Constants.MaxLogSizeBytes)
            return;

        // Rename current log with timestamp
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var rotatedName = $"app_{timestamp}.log";
        var rotatedPath = Path.Combine(AppPaths.LogsDirectory, rotatedName);

        try
        {
            // Use Move (atomic on same volume) instead of Copy+Delete
            File.Move(logFile, rotatedPath, overwrite: false);
        }
        catch
        {
            // If rotation fails, continue writing to current file
            return;
        }

        // Clean up old rotated logs beyond retention limit
        CleanupOldRotatedFiles();
    }

    private void CleanupOldRotatedFiles()
    {
        try
        {
            var directory = new DirectoryInfo(AppPaths.LogsDirectory);
            if (!directory.Exists)
                return;

            var rotatedFiles = directory
                .GetFiles("app_*.log")
                .OrderByDescending(f => f.LastWriteTime)
                .Skip(Constants.MaxRotatedLogFiles)
                .ToList();

            foreach (var file in rotatedFiles)
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // Best-effort cleanup
                }
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }
}
