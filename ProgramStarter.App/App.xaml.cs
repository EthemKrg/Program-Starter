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
        // Phase 1 - Real implementations
        services.AddSingleton<IConfigMigrationService, ConfigMigrationService>();
        services.AddSingleton<IAppLogger, FileAppLogger>();
        services.AddSingleton<IConfigService, JsonConfigService>();

        // Phase 2 - Real implementations
        services.AddSingleton<IDialogService, WpfDialogService>();

        // Phase 3 stub implementations (replaced by real implementations in Phase 3)
        services.AddSingleton<IFileDialogService, StubFileDialogService>();       // TODO: Replace with FileDialogService in Phase 3
        services.AddSingleton<IPathValidationService, StubPathValidationService>(); // TODO: Replace with PathValidationService in Phase 3

        // Phase 4 stub implementations (replaced by real implementations in Phase 4)
        services.AddSingleton<IProcessStarter, StubProcessStarter>();             // TODO: Replace with ProcessStarter in Phase 4
        services.AddSingleton<IAppLauncherService, StubAppLauncherService>();     // TODO: Replace with AppLauncherService in Phase 4

        // ViewModels
        services.AddSingleton<MainViewModel>();

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
