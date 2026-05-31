using System.Collections.ObjectModel;
using ProgramStarter.App.Models;
using ProgramStarter.App.Services;

namespace ProgramStarter.App.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IAppLogger _logger;
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

    public MainViewModel(IConfigService configService, IAppLogger logger)
    {
        _logger = logger;
        _logger.Info("MainViewModel initializing.");

        var settings = configService.Load();

        // Populate groups from loaded config into the observable collection
        if (settings.Groups.Count > 0)
        {
            foreach (var group in settings.Groups)
            {
                Groups.Add(group);
            }
        }

        UpdateHasGroups();
        _logger.Info($"MainViewModel initialized with {Groups.Count} group(s).");
    }

    public void UpdateHasGroups()
    {
        HasGroups = Groups.Count > 0;
    }
}
