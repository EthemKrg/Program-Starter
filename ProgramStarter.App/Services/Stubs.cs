using ProgramStarter.App.Models;
using System.Diagnostics;

namespace ProgramStarter.App.Services;

/// <summary>
/// Phase 0 stub implementations for service interfaces.
/// Replaced by real implementations in Phases 3 and 4.
/// </summary>

// TODO: Replace with real implementation in Phase 3
internal class StubFileDialogService : IFileDialogService
{
    public string? OpenFileDialog(string title, string filter) => null;
}

// TODO: Replace with real implementation in Phase 3
internal class StubPathValidationService : IPathValidationService
{
    public bool IsValidAppPath(string? path) => true;
    public bool IsSupportedExtension(string? path) => true;
    public bool FileExists(string? path) => true;
    public (bool IsValid, string ErrorMessage) ValidateForLaunch(string? path) => (true, string.Empty);
    public (bool IsValid, string ErrorMessage) ValidateForAdd(string path, string appName, AppGroup group) => (true, string.Empty);
}

// TODO: Replace with real implementation in Phase 4
internal class StubProcessStarter : IProcessStarter
{
    public Process? Start(ProcessStartInfo startInfo) => null;
}

// TODO: Replace with real implementation in Phase 4
internal class StubAppLauncherService : IAppLauncherService
{
    public Task<LaunchResult> LaunchOneAsync(AppEntry app) =>
        Task.FromResult(new LaunchResult { Success = true, UserMessage = "Launch requested." });

    public Task<List<LaunchResult>> LaunchGroupAsync(AppGroup group, int delayMilliseconds) =>
        Task.FromResult(new List<LaunchResult>());
}
