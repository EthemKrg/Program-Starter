using System.Diagnostics;
using System.IO;
using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

/// <summary>
/// Real implementation of <see cref="IAppLauncherService"/>.
/// Validates each app path via <see cref="IPathValidationService.ValidateForLaunch"/>,
/// starts the process via <see cref="IProcessStarter.Start"/>, and returns a detailed
/// <see cref="LaunchResult"/> for each attempted launch.
/// </summary>
internal class AppLauncherService : IAppLauncherService
{
    private readonly IProcessStarter _processStarter;
    private readonly IPathValidationService _pathValidationService;
    private readonly IAppLogger _logger;

    public AppLauncherService(
        IProcessStarter processStarter,
        IPathValidationService pathValidationService,
        IAppLogger logger)
    {
        _processStarter = processStarter;
        _pathValidationService = pathValidationService;
        _logger = logger;
    }

    public Task<LaunchResult> LaunchOneAsync(AppEntry app)
    {
        var result = LaunchCore(app);
        return Task.FromResult(result);
    }

    public async Task<List<LaunchResult>> LaunchGroupAsync(AppGroup group, int delayMilliseconds)
    {
        var results = new List<LaunchResult>();

        var enabledApps = group.Apps
            .Where(a => a.IsEnabled)
            .ToList();

        for (int i = 0; i < enabledApps.Count; i++)
        {
            var app = enabledApps[i];
            var result = LaunchCore(app);
            results.Add(result);

            if (i < enabledApps.Count - 1 && delayMilliseconds > 0)
            {
                await Task.Delay(delayMilliseconds);
            }
        }

        return results;
    }

    private LaunchResult LaunchCore(AppEntry app)
    {
        var validationError = ValidateBeforeLaunch(app);
        if (validationError is not null)
        {
            _logger.Warning($"Launch blocked for \"{app.Name}\": {validationError.UserMessage}");
            return validationError;
        }

        try
        {
            var workingDir = Path.GetDirectoryName(app.Path) ?? string.Empty;

            var startInfo = new ProcessStartInfo
            {
                FileName = app.Path,
                WorkingDirectory = workingDir,
                UseShellExecute = true
            };

            var (process, errorCode) = _processStarter.Start(startInfo);

            if (process is null || errorCode is not null)
            {
                var code = errorCode ?? LaunchErrorCode.Unknown;
                var userMessage = code switch
                {
                    LaunchErrorCode.AccessDenied => $"Cannot launch \"{app.Name}\". Access denied.",
                    LaunchErrorCode.FileNotFound => $"Cannot launch \"{app.Name}\". File not found.",
                    LaunchErrorCode.ProcessStartFailed => $"Cannot launch \"{app.Name}\". Process start failed.",
                    _ => $"Cannot launch \"{app.Name}\"."
                };

                var result = new LaunchResult
                {
                    Success = false,
                    AppName = app.Name,
                    Path = app.Path,
                    ErrorCode = code,
                    UserMessage = userMessage,
                    TechnicalMessage = $"ProcessStarter returned error: {code}"
                };
                _logger.Warning($"Launch failed for \"{app.Name}\": {code}");
                return result;
            }

            var successResult = new LaunchResult
            {
                Success = true,
                AppName = app.Name,
                Path = app.Path,
                ErrorCode = LaunchErrorCode.None,
                UserMessage = $"Launch requested for \"{app.Name}\"."
            };
            _logger.Info($"Launch requested for \"{app.Name}\" ({app.Path})");
            return successResult;
        }
        catch (Exception ex)
        {
            var result = new LaunchResult
            {
                Success = false,
                AppName = app.Name,
                Path = app.Path,
                ErrorCode = LaunchErrorCode.Unknown,
                UserMessage = $"Failed to launch \"{app.Name}\".",
                TechnicalMessage = ex.Message
            };
            _logger.Error($"Launch failed for \"{app.Name}\": {ex.Message}", ex);
            return result;
        }
    }

    private LaunchResult? ValidateBeforeLaunch(AppEntry app)
    {
        var (isValid, errorMessage) = _pathValidationService.ValidateForLaunch(app.Path);

        if (!isValid)
        {
            LaunchErrorCode errorCode;

            if (string.IsNullOrWhiteSpace(app.Path))
                errorCode = LaunchErrorCode.EmptyPath;
            else if (!_pathValidationService.IsSupportedExtension(app.Path))
                errorCode = LaunchErrorCode.UnsupportedFileType;
            else
                errorCode = LaunchErrorCode.FileNotFound;

            return new LaunchResult
            {
                Success = false,
                AppName = app.Name,
                Path = app.Path,
                ErrorCode = errorCode,
                UserMessage = $"Cannot launch \"{app.Name}\". {errorMessage}",
                TechnicalMessage = errorMessage
            };
        }

        return null;
    }
}
