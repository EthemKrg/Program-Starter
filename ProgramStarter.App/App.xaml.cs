using Microsoft.Extensions.DependencyInjection;
using ProgramStarter.App.Services;
using ProgramStarter.App.ViewModels;
using System.Windows;

namespace ProgramStarter.App;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Service interfaces (implementations added in later phases)
        services.AddSingleton<IConfigService, StubConfigService>();
        services.AddSingleton<IConfigMigrationService, StubConfigMigrationService>();
        services.AddSingleton<IAppLauncherService, StubAppLauncherService>();
        services.AddSingleton<IProcessStarter, StubProcessStarter>();
        services.AddSingleton<IFileDialogService, StubFileDialogService>();
        services.AddSingleton<IPathValidationService, StubPathValidationService>();
        services.AddSingleton<IAppLogger, StubAppLogger>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

#region Stub Service Implementations (Phase 0 placeholder)

internal class StubConfigService : IConfigService
{
    public Models.AppSettings Load() => new();
    public void Save(Models.AppSettings settings) { }
}

internal class StubConfigMigrationService : IConfigMigrationService
{
    public Models.AppSettings Migrate(System.Text.Json.JsonDocument rawConfig) => new();
}

internal class StubAppLauncherService : IAppLauncherService
{
    public Task<Models.LaunchResult> LaunchOneAsync(Models.AppEntry app) =>
        Task.FromResult(new Models.LaunchResult { Success = true, UserMessage = "Launch requested." });

    public Task<List<Models.LaunchResult>> LaunchGroupAsync(Models.AppGroup group, int delayMilliseconds) =>
        Task.FromResult(new List<Models.LaunchResult>());
}

internal class StubProcessStarter : IProcessStarter
{
    public System.Diagnostics.Process? Start(System.Diagnostics.ProcessStartInfo startInfo) => null;
}

internal class StubFileDialogService : IFileDialogService
{
    public string? OpenFileDialog(string title, string filter) => null;
}

internal class StubPathValidationService : IPathValidationService
{
    public bool IsValidAppPath(string? path) => true;
    public bool IsSupportedExtension(string? path) => true;
    public bool FileExists(string? path) => true;
    public (bool IsValid, string ErrorMessage) ValidateForLaunch(string? path) => (true, string.Empty);
    public (bool IsValid, string ErrorMessage) ValidateForAdd(string path, string appName, Models.AppGroup group) => (true, string.Empty);
}

internal class StubAppLogger : IAppLogger
{
    public void Info(string message) { }
    public void Warning(string message) { }
    public void Error(string message, Exception? exception = null) { }
}

#endregion
