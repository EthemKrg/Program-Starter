using ProgramStarter.App.Helpers;
using ProgramStarter.App.Models;
using ProgramStarter.App.Services;
using ProgramStarter.App.ViewModels;

namespace ProgramStarter.Tests.GroupManagement;

/// <summary>
/// Tests for MainViewModel group CRUD operations.
/// Uses fake service implementations to avoid file I/O and UI dialogs.
/// </summary>
public class MainViewModelTests : IDisposable
{
    private readonly string _testRoot;

    public MainViewModelTests()
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
    //  Constructor / Initialization
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_WithMultipleGroups_RestoresLastSelectedGroup()
    {
        // Arrange
        var groupA = new AppGroup { Id = "id-a", Name = "A" };
        var groupB = new AppGroup { Id = "id-b", Name = "B" };

        var settings = new AppSettings
        {
            Groups = new List<AppGroup> { groupA, groupB },
            LastSelectedGroupId = "id-b"
        };

        var fakeConfig = new FakeConfigService(settings);

        // Act
        var vm = CreateViewModel(fakeConfig);

        // Assert
        Assert.Same(groupB, vm.SelectedGroup);
        Assert.True(vm.HasSelectedGroup);
    }

    [Fact]
    public void Constructor_WithGroupsAndNoMatchingLastSelectedId_SelectsNull()
    {
        // Arrange
        var settings = new AppSettings
        {
            Groups = new List<AppGroup>
            {
                new() { Id = "id-a", Name = "A" }
            },
            LastSelectedGroupId = "nonexistent-id"
        };

        var fakeConfig = new FakeConfigService(settings);

        // Act
        var vm = CreateViewModel(fakeConfig);

        // Assert
        Assert.Null(vm.SelectedGroup);
        Assert.False(vm.HasSelectedGroup);
    }

    [Fact]
    public void Constructor_WithNoGroups_HasGroupsIsFalse()
    {
        // Arrange
        var settings = new AppSettings
        {
            Groups = new List<AppGroup>(),
            Theme = "Light",
            DefaultDelayMilliseconds = 2000
        };

        var fakeConfig = new FakeConfigService(settings);

        // Act
        var vm = CreateViewModel(fakeConfig);

        // Assert
        Assert.False(vm.HasGroups);
        Assert.Empty(vm.Groups);
        Assert.Null(vm.SelectedGroup);
    }

    // ──────────────────────────────────────────────
    //  Add Group
    // ──────────────────────────────────────────────

    [Fact]
    public void AddGroupCommand_WithValidName_CreatesGroupAndSelectsIt()
    {
        // Arrange
        var fakeDialog = new FakeDialogService { NextTextInputResult = "MyGroup" };
        var vm = CreateViewModelWithEmptyConfig(fakeDialog);

        // Act
        vm.AddGroupCommand.Execute(null);

        // Assert
        Assert.Single(vm.Groups);
        Assert.Equal("MyGroup", vm.Groups[0].Name);
        Assert.Same(vm.Groups[0], vm.SelectedGroup);
        Assert.True(vm.HasGroups);
    }

    [Fact]
    public void AddGroupCommand_WithDuplicateName_DoesNotAddGroup()
    {
        // Arrange
        var existingGroup = new AppGroup { Id = "existing", Name = "Games" };
        var settings = new AppSettings
        {
            Groups = new List<AppGroup> { existingGroup }
        };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = "Games" };
        var vm = CreateViewModel(fakeConfig, fakeDialog);

        // Act
        vm.AddGroupCommand.Execute(null);

        // Assert
        Assert.Single(vm.Groups);
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void AddGroupCommand_WhenCancelled_DoesNotCreateGroup()
    {
        // Arrange
        var fakeDialog = new FakeDialogService { NextTextInputResult = null }; // cancelled
        var vm = CreateViewModelWithEmptyConfig(fakeDialog);

        // Act
        vm.AddGroupCommand.Execute(null);

        // Assert
        Assert.Empty(vm.Groups);
        Assert.False(vm.HasGroups);
    }

    // ──────────────────────────────────────────────
    //  Rename Group
    // ──────────────────────────────────────────────

    [Fact]
    public void RenameGroupCommand_WithValidName_RenamesGroup()
    {
        // Arrange
        var group = new AppGroup { Id = "group1", Name = "OldName" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = "NewName" };
        var vm = CreateViewModel(fakeConfig, fakeDialog);

        // Act
        vm.RenameGroupCommand.Execute(group);

        // Assert
        Assert.Equal("NewName", group.Name);
        Assert.Single(vm.Groups);
    }

    [Fact]
    public void RenameGroupCommand_WithDuplicateName_DoesNotRename()
    {
        // Arrange
        var groupA = new AppGroup { Id = "a", Name = "Games" };
        var groupB = new AppGroup { Id = "b", Name = "Work" };
        var settings = new AppSettings { Groups = new List<AppGroup> { groupA, groupB } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = "Work" }; // duplicate
        var vm = CreateViewModel(fakeConfig, fakeDialog);

        // Act
        vm.RenameGroupCommand.Execute(groupA); // try renaming "Games" to "Work"

        // Assert
        Assert.Equal("Games", groupA.Name); // unchanged
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void RenameGroupCommand_WhenCancelled_DoesNotRename()
    {
        // Arrange
        var group = new AppGroup { Id = "g1", Name = "Original" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextTextInputResult = null }; // cancelled
        var vm = CreateViewModel(fakeConfig, fakeDialog);

        // Act
        vm.RenameGroupCommand.Execute(group);

        // Assert
        Assert.Equal("Original", group.Name);
    }

    // ──────────────────────────────────────────────
    //  Delete Group
    // ──────────────────────────────────────────────

    [Fact]
    public void DeleteGroupCommand_WithConfirmation_RemovesGroup()
    {
        // Arrange
        var group = new AppGroup { Id = "g1", Name = "ToDelete" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextConfirmResult = true };
        var vm = CreateViewModel(fakeConfig, fakeDialog);

        // Act
        vm.DeleteGroupCommand.Execute(group);

        // Assert
        Assert.Empty(vm.Groups);
        Assert.False(vm.HasGroups);
        Assert.Null(vm.SelectedGroup);
    }

    [Fact]
    public void DeleteGroupCommand_DeletedLastGroup_SelectsNull()
    {
        // Arrange
        var group = new AppGroup { Id = "g1", Name = "Solo" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextConfirmResult = true };
        var vm = CreateViewModel(fakeConfig, fakeDialog);

        // Pre-condition: select it first so the delete logic sees it as selected
        vm.SelectedGroup = group;

        // Act
        vm.DeleteGroupCommand.Execute(group);

        // Assert
        Assert.Empty(vm.Groups);
        Assert.Null(vm.SelectedGroup);
        Assert.False(vm.HasSelectedGroup);
    }

    [Fact]
    public void DeleteGroupCommand_DeletedMiddleGroup_SelectsNextGroup()
    {
        // Arrange
        var groupA = new AppGroup { Id = "a", Name = "A" };
        var groupB = new AppGroup { Id = "b", Name = "B" };
        var groupC = new AppGroup { Id = "c", Name = "C" };
        var settings = new AppSettings
        {
            Groups = new List<AppGroup> { groupA, groupB, groupC },
            LastSelectedGroupId = "b" // middle group selected
        };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextConfirmResult = true };
        var vm = CreateViewModel(fakeConfig, fakeDialog);

        // Pre-condition
        Assert.Same(groupB, vm.SelectedGroup);

        // Act
        vm.DeleteGroupCommand.Execute(groupB);

        // Assert
        Assert.Equal(2, vm.Groups.Count);
        Assert.Same(groupC, vm.SelectedGroup); // next group selected
    }

    [Fact]
    public void DeleteGroupCommand_WhenCancelled_DoesNotDelete()
    {
        // Arrange
        var group = new AppGroup { Id = "g1", Name = "Safe" };
        var settings = new AppSettings { Groups = new List<AppGroup> { group } };
        var fakeConfig = new FakeConfigService(settings);
        var fakeDialog = new FakeDialogService { NextConfirmResult = false }; // cancelled
        var vm = CreateViewModel(fakeConfig, fakeDialog);

        // Act
        vm.DeleteGroupCommand.Execute(group);

        // Assert
        Assert.Single(vm.Groups);
    }

    // ──────────────────────────────────────────────
    //  Save Config - Preserves Cached Values
    // ──────────────────────────────────────────────

    [Fact]
    public void SaveConfig_PreservesCachedThemeAndDelay()
    {
        // Arrange
        var settings = new AppSettings
        {
            Groups = new List<AppGroup>
            {
                new() { Id = "g1", Name = "Work" }
            },
            Theme = "Light",
            DefaultDelayMilliseconds = 5000
        };
        var fakeConfig = new FakeConfigService(settings);
        var vm = CreateViewModel(fakeConfig);

        // Act - add a group, which triggers SaveConfig
        vm.AddGroupCommand.Execute(null);

        // Assert - check what was saved
        var saved = fakeConfig.LastSavedSettings;
        Assert.NotNull(saved);
        Assert.Equal("Light", saved!.Theme);
        Assert.Equal(5000, saved.DefaultDelayMilliseconds);
    }

    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    private static MainViewModel CreateViewModel(
        FakeConfigService? config = null,
        FakeDialogService? dialog = null,
        FakeFileDialogService? fileDialog = null,
        FakePathValidationService? pathValidation = null,
        FakeLogger? logger = null)
    {
        var settings = config?.LoadedSettings ?? new AppSettings
        {
            Groups = new List<AppGroup>(),
            Theme = "Dark",
            DefaultDelayMilliseconds = 1000
        };
        var fakeConfig = config ?? new FakeConfigService(settings);
        return new MainViewModel(
            fakeConfig,
            dialog ?? new FakeDialogService(),
            fileDialog ?? new FakeFileDialogService(),
            pathValidation ?? new FakePathValidationService(),
            logger ?? new FakeLogger());
    }

    private static MainViewModel CreateViewModelWithEmptyConfig(FakeDialogService? fakeDialog = null)
    {
        var settings = new AppSettings
        {
            Groups = new List<AppGroup>(),
            Theme = "Dark",
            DefaultDelayMilliseconds = 1000
        };
        var fakeConfig = new FakeConfigService(settings);
        return new MainViewModel(
            fakeConfig,
            fakeDialog ?? new FakeDialogService(),
            new FakeFileDialogService(),
            new FakePathValidationService(),
            new FakeLogger());
    }
}

// ──────────────────────────────────────────────
//  Fake Service Implementations
// ──────────────────────────────────────────────

internal class FakeDialogService : IDialogService
{
    /// <summary>
    /// The value to return from the next ShowTextInputDialog call.
    /// Set to null to simulate cancellation.
    /// </summary>
    public string? NextTextInputResult { get; set; } = string.Empty;

    /// <summary>
    /// The value to return from the next ShowConfirmDialog call.
    /// </summary>
    public bool NextConfirmResult { get; set; } = true;

    /// <summary>
    /// The value to return from the next ShowAppEditDialog call.
    /// Set to null to simulate cancellation.
    /// </summary>
    public AppEditResult? NextAppEditResult { get; set; } = new AppEditResult("TestApp", @"C:\Test\app.exe");

    public string? ShowTextInputDialog(string title, string message, string initialValue = "")
        => NextTextInputResult;

    public bool ShowConfirmDialog(string title, string message)
        => NextConfirmResult;

    public AppEditResult? ShowAppEditDialog(string currentName, string currentPath)
        => NextAppEditResult;
}

internal class FakeFileDialogService : IFileDialogService
{
    /// <summary>
    /// The path to return from OpenFileDialog. Set to null to simulate cancellation.
    /// </summary>
    public string? NextPath { get; set; } = @"C:\Test\app.exe";

    public string? OpenFileDialog(string title, string filter) => NextPath;
}

internal class FakePathValidationService : IPathValidationService
{
    public bool IsValidAppPathResult { get; set; } = true;
    public bool IsSupportedExtensionResult { get; set; } = true;
    public bool FileExistsResult { get; set; } = true;
    public (bool IsValid, string ErrorMessage) ValidateForLaunchResult { get; set; } = (true, string.Empty);
    public (bool IsValid, string ErrorMessage) ValidateForAddResult { get; set; } = (true, string.Empty);

    public bool IsValidAppPath(string? path) => IsValidAppPathResult;
    public bool IsSupportedExtension(string? path) => IsSupportedExtensionResult;
    public bool FileExists(string? path) => FileExistsResult;
    public (bool IsValid, string ErrorMessage) ValidateForLaunch(string? path) => ValidateForLaunchResult;
    public (bool IsValid, string ErrorMessage) ValidateForAdd(string path, string appName, AppGroup group, AppEntry? excludeApp = null) => ValidateForAddResult;
}

internal class FakeConfigService : IConfigService
{
    private readonly AppSettings _initialSettings;

    /// <summary>
    /// The last settings object passed to <see cref="Save"/>.
    /// </summary>
    public AppSettings? LastSavedSettings { get; private set; }

    /// <summary>
    /// The settings this service was initialized with.
    /// </summary>
    public AppSettings LoadedSettings => _initialSettings;

    public FakeConfigService(AppSettings initialSettings)
    {
        _initialSettings = initialSettings;
    }

    public AppSettings Load() => _initialSettings;

    public void Save(AppSettings settings)
    {
        LastSavedSettings = settings;
    }
}

internal class FakeLogger : IAppLogger
{
    public List<string> InfoMessages { get; } = new();
    public List<string> WarningMessages { get; } = new();
    public List<(string Message, Exception? Exception)> ErrorMessages { get; } = new();

    public void Info(string message) => InfoMessages.Add(message);
    public void Warning(string message) => WarningMessages.Add(message);
    public void Error(string message, Exception? exception = null) => ErrorMessages.Add((message, exception));
}
