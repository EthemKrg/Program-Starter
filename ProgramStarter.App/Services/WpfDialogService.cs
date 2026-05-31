using System.Windows;
using ProgramStarter.App.Views.Dialogs;

namespace ProgramStarter.App.Services;

/// <summary>
/// WPF implementation of <see cref="IDialogService"/>.
/// Creates modal dialogs with the main window as owner.
/// Uses a robust owner resolution strategy: prefers the known MainWindow,
/// but falls back to iterating visible windows to find the real MainWindow.
/// This prevents dialogs from becoming standalone windows that can trigger
/// WPF application shutdown when closed.
/// </summary>
internal class WpfDialogService : IDialogService
{
    private readonly IFileDialogService _fileDialogService;

    public WpfDialogService(IFileDialogService fileDialogService)
    {
        _fileDialogService = fileDialogService;
    }

    /// <summary>
    /// Resolves the real application MainWindow for dialog ownership.
    /// </summary>
    private Window? ResolveOwner()
    {
        try
        {
            // Strategy 1: use the explicitly assigned MainWindow
            if (Application.Current?.MainWindow is not null)
                return Application.Current.MainWindow;

            // Strategy 2: scan all open windows to find the real MainWindow
            if (Application.Current is not null)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow mainWindow)
                    {
                        Application.Current.MainWindow = mainWindow;
                        return mainWindow;
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public string? ShowTextInputDialog(string title, string message, string initialValue = "")
    {
        var owner = ResolveOwner();
        var dialog = new TextInputDialog(title, message, initialValue);

        if (owner is not null)
            dialog.Owner = owner;

        var result = dialog.ShowDialog();
        return result == true ? dialog.Result : null;
    }

    public bool ShowConfirmDialog(string title, string message)
    {
        var owner = ResolveOwner();
        var dialog = new ConfirmDialog(title, message);

        if (owner is not null)
            dialog.Owner = owner;

        var result = dialog.ShowDialog();
        return result == true;
    }

    public AppEditResult? ShowAppEditDialog(string currentName, string currentPath)
    {
        var owner = ResolveOwner();
        var dialog = new AppEditDialog(currentName, currentPath, _fileDialogService);

        if (owner is not null)
            dialog.Owner = owner;

        var result = dialog.ShowDialog();
        return result == true ? dialog.Result : null;
    }
}
