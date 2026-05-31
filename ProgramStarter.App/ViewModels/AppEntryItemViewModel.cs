using ProgramStarter.App.Models;
using ProgramStarter.App.Services;

namespace ProgramStarter.App.ViewModels;

/// <summary>
/// Thin wrapper around AppEntry that adds display-oriented properties.
/// No config save, file dialog, or process launching logic.
/// </summary>
public class AppEntryItemViewModel : BaseViewModel
{
    private readonly IPathValidationService _pathValidationService;

    public AppEntry Model { get; }

    public string Id => Model.Id;

    public string Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name != value)
            {
                Model.Name = value;
                OnPropertyChanged();
            }
        }
    }

    public string Path
    {
        get => Model.Path;
        set
        {
            if (Model.Path != value)
            {
                Model.Path = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPathValid));
            }
        }
    }

    public bool IsEnabled
    {
        get => Model.IsEnabled;
        set
        {
            if (Model.IsEnabled != value)
            {
                Model.IsEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsPathValid => _pathValidationService.IsValidAppPath(Path);

    public AppEntryItemViewModel(AppEntry model, IPathValidationService pathValidationService)
    {
        Model = model;
        _pathValidationService = pathValidationService;
    }
}
