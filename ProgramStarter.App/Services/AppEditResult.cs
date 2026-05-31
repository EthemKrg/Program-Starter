namespace ProgramStarter.App.Services;

/// <summary>
/// Result from the app edit dialog.
/// Returns null if the user cancelled, otherwise contains the confirmed name and path.
/// </summary>
public record AppEditResult(string Name, string Path);
