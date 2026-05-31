using ProgramStarter.App.Models;
using System.Diagnostics;

namespace ProgramStarter.App.Services;

/// <summary>
/// Phase 0 stub implementations for service interfaces.
/// Replaced by real implementations in Phase 4.
/// </summary>

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
