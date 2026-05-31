using ProgramStarter.App.Models;
using ProgramStarter.App.Services;
using System.Windows.Media;

namespace ProgramStarter.App.ViewModels;

/// <summary>
/// Thin wrapper around AppEntry that adds display-oriented properties.
/// Provides a lazily-resolved icon source from the executable path.
/// </summary>
public class AppEntryItemViewModel : BaseViewModel
{
    private readonly IPathValidationService _pathValidationService;
    private ImageSource? _cachedIconSource;
    private bool _iconResolved;

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
                // Invalidate cached icon when path changes
                InvalidateIcon();
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

    /// <summary>
    /// Gets the icon source resolved from the executable path.
    /// Falls back to null if extraction fails (XAML fallback handles the Kaldforge image).
    /// Resolved once and cached for the lifetime of this ViewModel instance.
    /// </summary>
    public ImageSource? IconSource
    {
        get
        {
            if (!_iconResolved)
            {
                _cachedIconSource = IconExtractionService.GetIcon(Path);
                _iconResolved = true;
            }
            return _cachedIconSource;
        }
    }

    public AppEntryItemViewModel(AppEntry model, IPathValidationService pathValidationService)
    {
        Model = model;
        _pathValidationService = pathValidationService;
    }

    /// <summary>
    /// Resets the icon cache so it will be resolved again on next access.
    /// </summary>
    public void InvalidateIcon()
    {
        _cachedIconSource = null;
        _iconResolved = false;
        IconExtractionService.InvalidatePath(Path);
        OnPropertyChanged(nameof(IconSource));
    }
}
