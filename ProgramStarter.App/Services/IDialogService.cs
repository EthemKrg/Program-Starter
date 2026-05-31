namespace ProgramStarter.App.Services;

/// <summary>
/// Abstracts simple modal dialogs for text input and confirmation.
/// Designed for testability: test implementations return canned values.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a modal text input dialog.
    /// Returns the trimmed user input, or null if the user cancelled.
    /// </summary>
    string? ShowTextInputDialog(string title, string message, string initialValue = "");

    /// <summary>
    /// Shows a modal confirmation dialog.
    /// Returns true if the user confirmed, false if cancelled.
    /// </summary>
    bool ShowConfirmDialog(string title, string message);

    /// <summary>
    /// Shows a modal app edit dialog with name and path fields and a Browse button.
    /// Returns an AppEditResult if confirmed, or null if cancelled.
    /// </summary>
    AppEditResult? ShowAppEditDialog(string currentName, string currentPath);
}
