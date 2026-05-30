using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

public interface IAppLauncherService
{
    Task<LaunchResult> LaunchOneAsync(AppEntry app);
    Task<List<LaunchResult>> LaunchGroupAsync(AppGroup group, int delayMilliseconds);
}
