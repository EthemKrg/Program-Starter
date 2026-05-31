using System.Windows;
using ProgramStarter.App.Views.Dialogs;

namespace ProgramStarter.App.Services;

/// <summary>
/// WPF implementation of <see cref="IDialogService"/>.
/// Creates modal dialogs with the main window as owner.
/// </summary>
internal class WpfDialogService : IDialogService
{
    private Window? ResolveOwner()
    {
        try
        {
            return Application.Current?.MainWindow;
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
}
