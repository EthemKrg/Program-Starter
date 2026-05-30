using System.Collections.ObjectModel;
using ProgramStarter.App.Models;

namespace ProgramStarter.App.ViewModels;

public class MainViewModel : BaseViewModel
{
    private AppGroup? _selectedGroup;
    private string _statusMessage = string.Empty;
    private bool _hasGroups;
    private bool _isLaunching;

    public ObservableCollection<AppGroup> Groups { get; } = new();

    public AppGroup? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetProperty(ref _selectedGroup, value))
            {
                OnPropertyChanged(nameof(HasSelectedGroup));
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

    public MainViewModel()
    {
        // Phase 0: shell state only. No service calls, no CRUD yet.
        // Groups collection starts empty -> HasGroups = false triggers empty state.
        UpdateHasGroups();
    }

    public void UpdateHasGroups()
    {
        HasGroups = Groups.Count > 0;
    }
}
