using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using ProgramStarter.App.Commands;
using ProgramStarter.App.Helpers;
using ProgramStarter.App.Models;
using ProgramStarter.App.Services;

namespace ProgramStarter.App.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IConfigService _configService;
    private readonly IDialogService _dialogService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IPathValidationService _pathValidationService;
    private readonly IAppLogger _logger;

    private bool _isLoading;
    private AppGroup? _selectedGroup;
    private string _statusMessage = string.Empty;
    private bool _hasGroups;
    private bool _isLaunching;

    // Cached values from loaded config to avoid overwriting hand-edited settings
    private string _cachedTheme = "Dark";
    private int _cachedDelay = Constants.DefaultDelayMilliseconds;

    public ObservableCollection<AppGroup> Groups { get; } = new();

    public int SelectedGroupAppCount => SelectedGroup?.Apps.Count ?? 0;

    public AppGroup? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetProperty(ref _selectedGroup, value))
            {
                OnPropertyChanged(nameof(HasSelectedGroup));
                OnPropertyChanged(nameof(SelectedGroupAppCount));
                PopulateSelectedGroupApps();
                if (!_isLoading)
                {
                    SaveConfig();
                }
            }
        }
    }

    private ObservableCollection<AppEntryItemViewModel> _selectedGroupApps = new();
    public ObservableCollection<AppEntryItemViewModel> SelectedGroupApps
    {
        get => _selectedGroupApps;
        set => SetProperty(ref _selectedGroupApps, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool HasGroups
    {
        get => _hasGroups;
        set => SetProperty(ref _hasGroups, value);
    }

    public bool HasSelectedGroup => SelectedGroup is not null;

    public bool IsLaunching
    {
        get => _isLaunching;
        set => SetProperty(ref _isLaunching, value);
    }

    // Group CRUD commands
    public ICommand AddGroupCommand { get; }
    public ICommand RenameGroupCommand { get; }
    public ICommand DeleteGroupCommand { get; }

    // App CRUD commands
    public ICommand AddAppCommand { get; }
    public ICommand EditAppCommand { get; }
    public ICommand RemoveAppCommand { get; }

    public MainViewModel(
        IConfigService configService,
        IDialogService dialogService,
        IFileDialogService fileDialogService,
        IPathValidationService pathValidationService,
        IAppLogger logger)
    {
        _configService = configService;
        _dialogService = dialogService;
        _fileDialogService = fileDialogService;
        _pathValidationService = pathValidationService;
        _logger = logger;

        AddGroupCommand = new RelayCommand(ExecuteAddGroup);
        RenameGroupCommand = new RelayCommand(ExecuteRenameGroup);
        DeleteGroupCommand = new RelayCommand(ExecuteDeleteGroup);

        AddAppCommand = new RelayCommand(ExecuteAddApp, _ => HasSelectedGroup);
        EditAppCommand = new RelayCommand(ExecuteEditApp);
        RemoveAppCommand = new RelayCommand(ExecuteRemoveApp);

        _isLoading = true;
        _logger.Info("MainViewModel initializing.");

        try
        {
            var settings = configService.Load();

            // Cache non-group settings to preserve hand-edited values
            _cachedTheme = settings.Theme;
            _cachedDelay = settings.DefaultDelayMilliseconds;

            // Populate groups from loaded config
            if (settings.Groups.Count > 0)
            {
                foreach (var group in settings.Groups)
                {
                    Groups.Add(group);
                }
            }

            UpdateHasGroups();

            // Restore last selected group if it still exists
            if (settings.LastSelectedGroupId is not null)
            {
                SelectedGroup = Groups.FirstOrDefault(g =>
                    string.Equals(g.Id, settings.LastSelectedGroupId, StringComparison.OrdinalIgnoreCase));
            }

            _logger.Info($"MainViewModel initialized with {Groups.Count} group(s).");
        }
        finally
        {
            _isLoading = false;
        }
    }

    public void UpdateHasGroups()
    {
        HasGroups = Groups.Count > 0;
    }

    private void PopulateSelectedGroupApps()
    {
        SelectedGroupApps.Clear();
        if (SelectedGroup is null)
            return;

        foreach (var app in SelectedGroup.Apps)
        {
            SelectedGroupApps.Add(new AppEntryItemViewModel(app, _pathValidationService));
        }
    }

    // ──────────────────────────────────────────────
    //  Add Group
    // ──────────────────────────────────────────────

    private void ExecuteAddGroup(object? parameter)
    {
        var name = _dialogService.ShowTextInputDialog(
            "New Group",
            "Enter a name for the new group.");

        if (name is null)
            return;

        if (IsGroupNameDuplicate(name, excludeGroup: null))
        {
            StatusMessage = $"A group named \"{name}\" already exists.";
            _logger.Warning($"Attempted to add duplicate group name: {name}");
            return;
        }

        var group = new AppGroup
        {
            Id = Guid.NewGuid().ToString(),
            Name = name.Trim()
        };

        Groups.Add(group);
        SelectedGroup = group;
        UpdateHasGroups();
        SaveConfig();

        StatusMessage = $"Group \"{group.Name}\" created.";
        _logger.Info($"Group created: {group.Name} ({group.Id})");
    }

    // ──────────────────────────────────────────────
    //  Rename Group
    // ──────────────────────────────────────────────

    private void ExecuteRenameGroup(object? parameter)
    {
        if (parameter is not AppGroup group)
            return;

        var newName = _dialogService.ShowTextInputDialog(
            "Rename Group",
            "Enter a new name for this group.",
            group.Name);

        if (newName is null)
            return;

        // Normalize
        newName = newName.Trim();
        if (newName.Length == 0)
            newName = "New Group";

        if (IsGroupNameDuplicate(newName, excludeGroup: group))
        {
            StatusMessage = $"A group named \"{newName}\" already exists.";
            _logger.Warning($"Attempted to rename group to duplicate name: {newName}");
            return;
        }

        var oldName = group.Name;
        group.Name = newName;

        // Refresh UI binding for the group item
        var index = Groups.IndexOf(group);
        if (index >= 0)
        {
            Groups.RemoveAt(index);
            Groups.Insert(index, group);
        }

        SaveConfig();
        StatusMessage = $"Group renamed from \"{oldName}\" to \"{newName}\".";
        _logger.Info($"Group renamed: \"{oldName}\" -> \"{newName}\" ({group.Id})");
    }

    // ──────────────────────────────────────────────
    //  Delete Group
    // ──────────────────────────────────────────────

    private void ExecuteDeleteGroup(object? parameter)
    {
        if (parameter is not AppGroup group)
            return;

        var confirmed = _dialogService.ShowConfirmDialog(
            "Delete Group",
            $"Delete \"{group.Name}\" group?\nThis will remove all apps inside this group from Program Starter.");

        if (!confirmed)
            return;

        var deletedIndex = Groups.IndexOf(group);
        var wasSelected = ReferenceEquals(_selectedGroup, group);

        Groups.Remove(group);
        UpdateHasGroups();

        // Select next group if the deleted group was selected
        if (wasSelected)
        {
            SelectNextGroupAfterDelete(deletedIndex);
        }

        SaveConfig();
        StatusMessage = $"Group \"{group.Name}\" deleted.";
        _logger.Info($"Group deleted: {group.Name} ({group.Id})");
    }

    // ──────────────────────────────────────────────
    //  Add App
    // ──────────────────────────────────────────────

    private void ExecuteAddApp(object? parameter)
    {
        if (SelectedGroup is null)
            return;

        var selectedPath = _fileDialogService.OpenFileDialog(
            "Select an application",
            "Executable files (*.exe)|*.exe");

        if (selectedPath is null)
            return;

        var autoName = Path.GetFileNameWithoutExtension(selectedPath) ?? "New App";

        var confirmedName = _dialogService.ShowTextInputDialog(
            "Add App",
            "Confirm or edit the app name.",
            autoName);

        if (confirmedName is null)
            return;

        var trimmedName = confirmedName.Trim();
        var (isValid, errorMessage) = _pathValidationService.ValidateForAdd(selectedPath, trimmedName, SelectedGroup);

        if (!isValid)
        {
            StatusMessage = errorMessage;
            _logger.Warning($"Add app validation failed: {errorMessage}");
            return;
        }

        var entry = new AppEntry
        {
            Id = Guid.NewGuid().ToString(),
            Name = trimmedName,
            Path = selectedPath,
            IsEnabled = true
        };

        SelectedGroup.Apps.Add(entry);
        OnPropertyChanged(nameof(SelectedGroupAppCount));
        PopulateSelectedGroupApps();
        SaveConfig();

        StatusMessage = $"App \"{trimmedName}\" added.";
        _logger.Info($"App added: {trimmedName} ({entry.Id}) to group {SelectedGroup.Name}");
    }

    // ──────────────────────────────────────────────
    //  Edit App
    // ──────────────────────────────────────────────

    private void ExecuteEditApp(object? parameter)
    {
        if (parameter is not AppEntryItemViewModel appVm)
            return;

        if (SelectedGroup is null)
            return;

        var result = _dialogService.ShowAppEditDialog(appVm.Name, appVm.Path);

        if (result is null)
            return;

        var trimmedName = result.Name.Trim();
        var trimmedPath = result.Path.Trim();

        var (isValid, errorMessage) = _pathValidationService.ValidateForAdd(trimmedPath, trimmedName, SelectedGroup, excludeApp: appVm.Model);

        if (!isValid)
        {
            StatusMessage = errorMessage;
            _logger.Warning($"Edit app validation failed: {errorMessage}");
            return;
        }

        appVm.Name = trimmedName;
        appVm.Path = trimmedPath;
        PopulateSelectedGroupApps();
        SaveConfig();

        StatusMessage = $"App \"{trimmedName}\" updated.";
        _logger.Info($"App updated: \"{trimmedName}\" ({appVm.Id})");
    }

    // ──────────────────────────────────────────────
    //  Remove App
    // ──────────────────────────────────────────────

    private void ExecuteRemoveApp(object? parameter)
    {
        if (parameter is not AppEntryItemViewModel appVm)
            return;

        if (SelectedGroup is null)
            return;

        var confirmed = _dialogService.ShowConfirmDialog(
            "Remove App",
            $"Remove \"{appVm.Name}\" from this group?\nThis will not uninstall the application from your computer.");

        if (!confirmed)
            return;

        SelectedGroup.Apps.Remove(appVm.Model);
        OnPropertyChanged(nameof(SelectedGroupAppCount));
        PopulateSelectedGroupApps();
        SaveConfig();

        StatusMessage = $"App \"{appVm.Name}\" removed.";
        _logger.Info($"App removed: {appVm.Name} ({appVm.Id}) from group {SelectedGroup.Name}");
    }

    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    private bool IsGroupNameDuplicate(string name, AppGroup? excludeGroup)
    {
        var trimmed = name.Trim();
        return Groups.Any(g =>
            !ReferenceEquals(g, excludeGroup) &&
            string.Equals(g.Name, trimmed, StringComparison.OrdinalIgnoreCase));
    }

    private void SelectNextGroupAfterDelete(int deletedIndex)
    {
        if (Groups.Count == 0)
        {
            SelectedGroup = null;
            return;
        }

        // Select the group at the same index, or the last group if deleted was last
        var nextIndex = Math.Min(deletedIndex, Groups.Count - 1);
        SelectedGroup = Groups[nextIndex];
    }

    private void SaveConfig()
    {
        // Don't save during constructor initialization
        if (_isLoading)
            return;

        try
        {
            var settings = new AppSettings
            {
                SchemaVersion = Constants.SupportedSchemaVersion,
                Groups = Groups.ToList(),
                LastSelectedGroupId = SelectedGroup?.Id,
                Theme = _cachedTheme,
                DefaultDelayMilliseconds = _cachedDelay
            };

            _configService.Save(settings);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to save config after group change.", ex);
            StatusMessage = "Failed to save changes. Check logs for details.";
        }
    }
}
