using ProgramStarter.App.Helpers;
using ProgramStarter.Tests.GroupManagement;
using ProgramStarter.App.Models;
using ProgramStarter.App.Services;
using ProgramStarter.App.ViewModels;

namespace ProgramStarter.Tests.AppManagement;

/// <summary>
/// Focused tests for MainViewModel app CRUD operations and launch commands.
/// </summary>
public class MainViewModelAppTests : IDisposable
{
    private readonly string _testRoot;

    public MainViewModelAppTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "ProgramStarter_Tests_" + Guid.NewGuid().ToString("N"));
        AppPaths.SetTestModeRoot(_testRoot);
    }

    public void Dispose()
    {
        AppPaths.ResetRoot();

        if (Directory.Exists(_testRoot))
        {
            try { Directory.Delete(_testRoot, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }

    // ──────────────────────────────────────────────
    //  Add App
    // ──────────────────────────────────────────────

    [Fact]
    public void AddApp_WithValidExe_AddsToGroup()
    {
        // Arrange
        var group = new AppGroup { Id = "g1", Name = "Work" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = "Slack" };
        var fakeFileDialog = new FakeFileDialogService { NextPath = @"C:\Apps\slack.exe" };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, fakeFileDialog, fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        // Act
        vm.AddAppCommand.Execute(null);

        // Assert
        Assert.Single(group.Apps);
        Assert.Equal("Slack", group.Apps[0].Name);
        Assert.Equal(@"C:\Apps\slack.exe", group.Apps[0].Path);
        Assert.Single(vm.SelectedGroupApps);
        Assert.True(vm.SelectedGroupApps[0].IsPathValid);
    }

    [Fact]
    public void AddApp_CancelledFilePicker_NoChange()
    {
        // Arrange
        var group = new AppGroup { Id = "g1", Name = "Work" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeFileDialog = new FakeFileDialogService { NextPath = null }; // cancelled
        var vm = new MainViewModel(fakeConfig, new FakeDialogService(), fakeFileDialog, new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        // Act
        vm.AddAppCommand.Execute(null);

        // Assert
        Assert.Empty(group.Apps);
    }

    [Fact]
    public void AddApp_CancelledNameDialog_NoChange()
    {
        // Arrange
        var group = new AppGroup { Id = "g1", Name = "Work" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = null }; // cancelled on name dialog
        var fakeFileDialog = new FakeFileDialogService { NextPath = @"C:\Apps\slack.exe" };
        var vm = new MainViewModel(fakeConfig, fakeDialog, fakeFileDialog, new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        // Act
        vm.AddAppCommand.Execute(null);

        // Assert
        Assert.Empty(group.Apps);
    }

    [Fact]
    public void AddApp_DuplicateName_Rejected()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "Slack", Path = @"C:\slack.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = "Slack" };
        var fakeFileDialog = new FakeFileDialogService { NextPath = @"C:\new\slack.exe" };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForAddResult = (false, "An app named \"Slack\" already exists in this group.")
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, fakeFileDialog, fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        // Act
        vm.AddAppCommand.Execute(null);

        // Assert
        Assert.Single(group.Apps); // unchanged
        Assert.Contains("already exists", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddApp_DuplicatePath_Rejected()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "Slack", Path = @"C:\slack.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = "NewApp" };
        var fakeFileDialog = new FakeFileDialogService { NextPath = @"C:\slack.exe" };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForAddResult = (false, "This app path already exists in this group.")
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, fakeFileDialog, fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        // Act
        vm.AddAppCommand.Execute(null);

        // Assert
        Assert.Single(group.Apps); // unchanged
        Assert.Contains("already exists", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddApp_UnsupportedExtension_Rejected()
    {
        // Arrange
        var group = new AppGroup { Id = "g1", Name = "Work" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = "Test" };
        var fakeFileDialog = new FakeFileDialogService { NextPath = @"C:\test.dll" };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = false,
            FileExistsResult = true,
            ValidateForAddResult = (false, "Only .exe files are supported.")
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, fakeFileDialog, fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        // Act
        vm.AddAppCommand.Execute(null);

        // Assert
        Assert.Empty(group.Apps);
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void AddApp_MissingFile_Rejected()
    {
        // Arrange
        var group = new AppGroup { Id = "g1", Name = "Work" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = "Test" };
        var fakeFileDialog = new FakeFileDialogService { NextPath = @"C:\missing.exe" };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = false,
            ValidateForAddResult = (false, "File does not exist.")
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, fakeFileDialog, fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        // Act
        vm.AddAppCommand.Execute(null);

        // Assert
        Assert.Empty(group.Apps);
        Assert.NotEmpty(vm.StatusMessage);
    }

    // ──────────────────────────────────────────────
    //  Edit App - Name
    // ──────────────────────────────────────────────

    [Fact]
    public void EditApp_WithValidName_UpdatesName()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "OldName", Path = @"C:\app.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService
        {
            NextAppEditResult = new AppEditResult("NewName", @"C:\app.exe")
        };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, new FakeFileDialogService(), fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First();

        // Act
        vm.EditAppCommand.Execute(appVm);

        // Assert
        Assert.Equal("NewName", app.Name);
        Assert.Equal("NewName", appVm.Name);
    }

    [Fact]
    public void EditApp_WithDuplicateName_DoesNotUpdate()
    {
        // Arrange
        var appA = new AppEntry { Id = "a1", Name = "Slack", Path = @"C:\slack.exe" };
        var appB = new AppEntry { Id = "a2", Name = "Discord", Path = @"C:\discord.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { appA, appB } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService
        {
            NextAppEditResult = new AppEditResult("Discord", @"C:\slack.exe") // name is duplicate, path unchanged
        };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForAddResult = (false, "An app named \"Discord\" already exists in this group.")
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, new FakeFileDialogService(), fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First(a => a.Id == "a1");

        // Act
        vm.EditAppCommand.Execute(appVm);

        // Assert
        Assert.Equal("Slack", appA.Name); // unchanged
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void EditApp_CancelledDialog_DoesNotUpdate()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "Original", Path = @"C:\app.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextAppEditResult = null }; // cancelled
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, new FakeFileDialogService(), fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First();

        // Act
        vm.EditAppCommand.Execute(appVm);

        // Assert
        Assert.Equal("Original", app.Name);
        Assert.Equal(@"C:\app.exe", app.Path);
    }

    // ──────────────────────────────────────────────
    //  Edit App - Path
    // ──────────────────────────────────────────────

    [Fact]
    public void EditApp_WithPathChange_UpdatesPath()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "Slack", Path = @"C:\old\slack.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService
        {
            NextAppEditResult = new AppEditResult("Slack", @"C:\new\slack.exe")
        };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, new FakeFileDialogService(), fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First();

        // Act
        vm.EditAppCommand.Execute(appVm);

        // Assert
        Assert.Equal(@"C:\new\slack.exe", app.Path);
        Assert.Equal(@"C:\new\slack.exe", appVm.Path);
    }

    [Fact]
    public void EditApp_WithDuplicatePath_DoesNotUpdate()
    {
        // Arrange
        var appA = new AppEntry { Id = "a1", Name = "Slack", Path = @"C:\slack.exe" };
        var appB = new AppEntry { Id = "a2", Name = "Discord", Path = @"C:\discord.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { appA, appB } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService
        {
            NextAppEditResult = new AppEditResult("Slack", @"C:\discord.exe") // path is duplicate
        };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = true,
            ValidateForAddResult = (false, "This app path already exists in this group.")
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, new FakeFileDialogService(), fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First(a => a.Id == "a1");

        // Act
        vm.EditAppCommand.Execute(appVm);

        // Assert
        Assert.Equal(@"C:\slack.exe", appA.Path); // unchanged
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void EditApp_WithUnsupportedExtension_DoesNotUpdate()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "Script", Path = @"C:\script.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService
        {
            NextAppEditResult = new AppEditResult("Script", @"C:\script.dll")
        };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = false, // .dll is not supported
            FileExistsResult = true,
            ValidateForAddResult = (false, "Only .exe files are supported.")
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, new FakeFileDialogService(), fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First();

        // Act
        vm.EditAppCommand.Execute(appVm);

        // Assert
        Assert.Equal(@"C:\script.exe", app.Path); // unchanged
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void EditApp_WithMissingFile_DoesNotUpdate()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "Script", Path = @"C:\script.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService
        {
            NextAppEditResult = new AppEditResult("Script", @"C:\missing.exe")
        };
        var fakePathValidation = new FakePathValidationService
        {
            IsSupportedExtensionResult = true,
            FileExistsResult = false,
            ValidateForAddResult = (false, "File does not exist.")
        };
        var vm = new MainViewModel(fakeConfig, fakeDialog, new FakeFileDialogService(), fakePathValidation, new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First();

        // Act
        vm.EditAppCommand.Execute(appVm);

        // Assert
        Assert.Equal(@"C:\script.exe", app.Path); // unchanged
        Assert.NotEmpty(vm.StatusMessage);
    }

    // ──────────────────────────────────────────────
    //  Remove App
    // ──────────────────────────────────────────────

    [Fact]
    public void RemoveApp_Confirmed_RemovesApp()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "ToRemove", Path = @"C:\app.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextConfirmResult = true };
        var vm = new MainViewModel(fakeConfig, fakeDialog, new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First();

        // Act
        vm.RemoveAppCommand.Execute(appVm);

        // Assert
        Assert.Empty(group.Apps);
        Assert.Empty(vm.SelectedGroupApps);
    }

    [Fact]
    public void RemoveApp_Cancelled_DoesNotRemove()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "Keep", Path = @"C:\app.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextConfirmResult = false }; // cancelled
        var vm = new MainViewModel(fakeConfig, fakeDialog, new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(new FakeAppLauncherService());
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First();

        // Act
        vm.RemoveAppCommand.Execute(appVm);

        // Assert
        Assert.Single(group.Apps);
        Assert.Single(vm.SelectedGroupApps);
    }

    // ──────────────────────────────────────────────
    //  Launch Commands
    // ──────────────────────────────────────────────

    [Fact]
    public void LaunchAppCommand_WithValidApp_ReturnsSuccessStatus()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "TestApp", Path = @"C:\test.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeLauncher = new FakeAppLauncherService
        {
            LaunchOneResult = new LaunchResult
            {
                Success = true,
                AppName = "TestApp",
                Path = @"C:\test.exe",
                ErrorCode = LaunchErrorCode.None,
                UserMessage = "Launch requested for \"TestApp\"."
            }
        };
        var vm = new MainViewModel(fakeConfig, new FakeDialogService(), new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(fakeLauncher);
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First();

        // Act
        vm.LaunchAppCommand.Execute(appVm);

        // Assert
        Assert.Contains("Launch requested", vm.StatusMessage);
    }

    [Fact]
    public void LaunchAppCommand_WithFailedLaunch_ShowsErrorStatus()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "BadApp", Path = @"C:\bad.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeLauncher = new FakeAppLauncherService
        {
            LaunchOneResult = new LaunchResult
            {
                Success = false,
                AppName = "BadApp",
                Path = @"C:\bad.exe",
                ErrorCode = LaunchErrorCode.FileNotFound,
                UserMessage = "Cannot launch \"BadApp\". File does not exist."
            }
        };
        var vm = new MainViewModel(fakeConfig, new FakeDialogService(), new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(fakeLauncher);
        vm.SelectedGroup = group;

        var appVm = vm.SelectedGroupApps.First();

        // Act
        vm.LaunchAppCommand.Execute(appVm);

        // Assert
        Assert.Contains("Cannot launch", vm.StatusMessage);
    }

    [Fact]
    public void LaunchAppCommand_WhenIsLaunching_DoesNotExecute()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "TestApp", Path = @"C:\test.exe" };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeLauncher = new FakeAppLauncherService
        {
            LaunchOneResult = new LaunchResult
            {
                Success = true,
                AppName = "TestApp",
                Path = @"C:\test.exe",
                UserMessage = "Launch requested for \"TestApp\"."
            }
        };
        var vm = new MainViewModel(fakeConfig, new FakeDialogService(), new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(fakeLauncher);
        vm.SelectedGroup = group;

        // Mark as already launching (simulate in-progress state)
        vm.IsLaunching = true;

        var appVm = vm.SelectedGroupApps.First();

        // Act - LaunchAppCommand should be disabled while IsLaunching is true
        var canLaunch = vm.LaunchAppCommand.CanExecute(appVm);

        // Assert
        Assert.False(canLaunch);
    }

    // ──────────────────────────────────────────────
    //  Launch Group Command Tests
    // ──────────────────────────────────────────────

    [Fact]
    public void LaunchGroupCommand_AllSuccess_ShowsLaunchRequestedStatus()
    {
        // Arrange
        var app1 = new AppEntry { Id = "a1", Name = "App1", Path = @"C:\app1.exe", IsEnabled = true };
        var app2 = new AppEntry { Id = "a2", Name = "App2", Path = @"C:\app2.exe", IsEnabled = true };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app1, app2 } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group }, DefaultDelayMilliseconds = 0 };
        var fakeConfig = new FakeConfigService(settings);
        var fakeLauncher = new FakeAppLauncherService
        {
            LaunchGroupResults = new List<LaunchResult>
            {
                new() { Success = true, AppName = "App1", UserMessage = "Launch requested for \"App1\"." },
                new() { Success = true, AppName = "App2", UserMessage = "Launch requested for \"App2\"." }
            }
        };
        var vm = new MainViewModel(fakeConfig, new FakeDialogService(), new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(fakeLauncher);
        vm.SelectedGroup = group;

        // Act
        vm.LaunchGroupCommand.Execute(null);

        // Assert
        Assert.Equal("Launch requested for 2 app(s).", vm.StatusMessage);
    }

    [Fact]
    public void LaunchGroupCommand_MixedResults_ShowsPartialStatus()
    {
        // Arrange
        var app1 = new AppEntry { Id = "a1", Name = "App1", Path = @"C:\app1.exe", IsEnabled = true };
        var app2 = new AppEntry { Id = "a2", Name = "App2", Path = @"C:\app2.exe", IsEnabled = true };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app1, app2 } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group }, DefaultDelayMilliseconds = 0 };
        var fakeConfig = new FakeConfigService(settings);
        var fakeLauncher = new FakeAppLauncherService
        {
            LaunchGroupResults = new List<LaunchResult>
            {
                new() { Success = true, AppName = "App1", UserMessage = "Launch requested for \"App1\"." },
                new() { Success = false, AppName = "App2", Path = @"C:\app2.exe", ErrorCode = LaunchErrorCode.FileNotFound, UserMessage = "Cannot launch \"App2\". File does not exist." }
            }
        };
        var vm = new MainViewModel(fakeConfig, new FakeDialogService(), new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(fakeLauncher);
        vm.SelectedGroup = group;

        // Act
        vm.LaunchGroupCommand.Execute(null);

        // Assert
        Assert.Equal("Launch requested for 1 app(s). 1 failed.", vm.StatusMessage);
    }

    [Fact]
    public void LaunchGroupCommand_AllFailed_ShowsCouldNotLaunchStatus()
    {
        // Arrange
        var app1 = new AppEntry { Id = "a1", Name = "App1", Path = @"C:\app1.exe", IsEnabled = true };
        var app2 = new AppEntry { Id = "a2", Name = "App2", Path = @"C:\app2.exe", IsEnabled = true };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app1, app2 } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group }, DefaultDelayMilliseconds = 0 };
        var fakeConfig = new FakeConfigService(settings);
        var fakeLauncher = new FakeAppLauncherService
        {
            LaunchGroupResults = new List<LaunchResult>
            {
                new() { Success = false, AppName = "App1", ErrorCode = LaunchErrorCode.FileNotFound, UserMessage = "Cannot launch \"App1\". File does not exist." },
                new() { Success = false, AppName = "App2", ErrorCode = LaunchErrorCode.FileNotFound, UserMessage = "Cannot launch \"App2\". File does not exist." }
            }
        };
        var vm = new MainViewModel(fakeConfig, new FakeDialogService(), new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(fakeLauncher);
        vm.SelectedGroup = group;

        // Act
        vm.LaunchGroupCommand.Execute(null);

        // Assert
        Assert.Equal("Could not launch any apps. Please check the app paths.", vm.StatusMessage);
    }

    [Fact]
    public void LaunchGroupCommand_AllDisabled_ShowsEmptyStatus()
    {
        // Arrange
        var app1 = new AppEntry { Id = "a1", Name = "App1", Path = @"C:\app1.exe", IsEnabled = false };
        var app2 = new AppEntry { Id = "a2", Name = "App2", Path = @"C:\app2.exe", IsEnabled = false };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app1, app2 } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group }, DefaultDelayMilliseconds = 0 };
        var fakeConfig = new FakeConfigService(settings);
        var fakeLauncher = new FakeAppLauncherService
        {
            LaunchGroupResults = new List<LaunchResult>() // empty - no enabled apps
        };
        var vm = new MainViewModel(fakeConfig, new FakeDialogService(), new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(fakeLauncher);
        vm.SelectedGroup = group;

        // Act
        vm.LaunchGroupCommand.Execute(null);

        // Assert
        Assert.Equal("No enabled apps to launch in this group.", vm.StatusMessage);
    }

    [Fact]
    public void LaunchGroupCommand_IsLaunchingResetsAfterFailure()
    {
        // Arrange
        var app = new AppEntry { Id = "a1", Name = "App1", Path = @"C:\app1.exe", IsEnabled = true };
        var group = new AppGroup { Id = "g1", Name = "Work", Apps = new List<AppEntry> { app } };
        var settings = new AppSettings { Groups = new List<AppGroup> { group }, DefaultDelayMilliseconds = 0 };
        var fakeConfig = new FakeConfigService(settings);
        var fakeLauncher = new FakeAppLauncherService
        {
            LaunchGroupResults = new List<LaunchResult>
            {
                new() { Success = false, AppName = "App1", ErrorCode = LaunchErrorCode.FileNotFound, UserMessage = "Cannot launch \"App1\". File does not exist." }
            }
        };
        var vm = new MainViewModel(fakeConfig, new FakeDialogService(), new FakeFileDialogService(), new FakePathValidationService(), new FakeLogger());
        vm.SetAppLauncherService(fakeLauncher);
        vm.SelectedGroup = group;

        // Act
        vm.LaunchGroupCommand.Execute(null);

        // Assert - IsLaunching must be false after command completes
        Assert.False(vm.IsLaunching);
    }
}
