using Microsoft.Extensions.DependencyInjection;
using ProgramStarter.App.Services;
using ProgramStarter.App.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ProgramStarter.App;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;

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

        // Phase 3 - Real implementations
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<IPathValidationService, PathValidationService>();

        // Phase 4 - Real implementations
        services.AddSingleton<IProcessStarter, ProcessStarter>();
        services.AddSingleton<IAppLauncherService, AppLauncherService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Register error-handling hooks
        Exit += OnAppExit;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        // Explicitly register the real MainWindow for dialog ownership
        Application.Current.MainWindow = mainWindow;

        // Wire up AppLauncherService to MainViewModel (singleton, already resolved by MainWindow)
        var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        var appLauncherService = _serviceProvider.GetRequiredService<IAppLauncherService>();
        viewModel.SetAppLauncherService(appLauncherService);

        mainWindow.Show();

        try
        {
            var logger = _serviceProvider.GetRequiredService<IAppLogger>();
            logger.Info("Application started successfully.");
        }
        catch
        {
            // Best-effort
        }
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ProgramStarter", "logs");
            Directory.CreateDirectory(logDir);
            var crashLog = Path.Combine(logDir, "crash_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            File.WriteAllText(crashLog,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FATAL] Unhandled dispatcher exception{Environment.NewLine}" +
                $"Exception: {e.Exception?.GetType().FullName}{Environment.NewLine}" +
                $"Message: {e.Exception?.Message}{Environment.NewLine}" +
                $"StackTrace: {e.Exception?.StackTrace}{Environment.NewLine}");
        }
        catch
        {
            try
            {
                using var eventLog = new EventLog("Application");
                eventLog.Source = "ProgramStarter";
                eventLog.WriteEntry(
                    $"Unhandled exception: {e.Exception?.GetType().FullName}: {e.Exception?.Message}",
                    EventLogEntryType.Error);
            }
            catch { }
        }
    }

    private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            var ex = e.ExceptionObject as Exception;
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ProgramStarter", "logs");
            Directory.CreateDirectory(logDir);
            var crashLog = Path.Combine(logDir, "crash_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            File.WriteAllText(crashLog,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FATAL] AppDomain.UnhandledException (isTerminating={e.IsTerminating}){Environment.NewLine}" +
                $"Exception: {ex?.GetType().FullName}{Environment.NewLine}" +
                $"Message: {ex?.Message}{Environment.NewLine}" +
                $"StackTrace: {ex?.StackTrace}{Environment.NewLine}");
        }
        catch { }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ProgramStarter", "logs");
            Directory.CreateDirectory(logDir);
            var crashLog = Path.Combine(logDir, "crash_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            File.WriteAllText(crashLog,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FATAL] TaskScheduler.UnobservedTaskException{Environment.NewLine}" +
                $"Exception: {e.Exception?.GetType().FullName}{Environment.NewLine}" +
                $"Message: {e.Exception?.Message}{Environment.NewLine}" +
                $"StackTrace: {e.Exception?.StackTrace}{Environment.NewLine}");
        }
        catch { }
    }

    private void OnAppExit(object sender, ExitEventArgs e)
    {
        try
        {
            var logger = _serviceProvider?.GetService<IAppLogger>();
            if (logger is not null)
                logger.Info($"Application exiting. ApplicationExitCode={e.ApplicationExitCode}");
        }
        catch { }
    }
}
