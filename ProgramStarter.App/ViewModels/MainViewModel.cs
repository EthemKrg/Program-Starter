using System.Collections.ObjectModel;
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

    public AppGroup? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetProperty(ref _selectedGroup, value))
            {
                OnPropertyChanged(nameof(HasSelectedGroup));
                if (!_isLoading)
                {
                    SaveConfig();
                }
            }
        }
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

    public MainViewModel(IConfigService configService, IDialogService dialogService, IAppLogger logger)
    {
        _configService = configService;
        _dialogService = dialogService;
        _logger = logger;

        AddGroupCommand = new RelayCommand(ExecuteAddGroup);
        RenameGroupCommand = new RelayCommand(ExecuteRenameGroup);
        DeleteGroupCommand = new RelayCommand(ExecuteDeleteGroup);

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
