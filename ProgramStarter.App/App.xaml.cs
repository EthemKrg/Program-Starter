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
        // Stub service implementations (replaced by real implementations in later phases)
        services.AddSingleton<IConfigService, StubConfigService>();               // TODO: Replace with JsonConfigService in Phase 1
        services.AddSingleton<IConfigMigrationService, StubConfigMigrationService>(); // TODO: Replace with ConfigMigrationService in Phase 1
        services.AddSingleton<IAppLogger, StubAppLogger>();                       // TODO: Replace with FileAppLogger in Phase 1
        services.AddSingleton<IFileDialogService, StubFileDialogService>();       // TODO: Replace with FileDialogService in Phase 3
        services.AddSingleton<IPathValidationService, StubPathValidationService>(); // TODO: Replace with PathValidationService in Phase 3
        services.AddSingleton<IProcessStarter, StubProcessStarter>();             // TODO: Replace with ProcessStarter in Phase 4
        services.AddSingleton<IAppLauncherService, StubAppLauncherService>();     // TODO: Replace with AppLauncherService in Phase 4

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
