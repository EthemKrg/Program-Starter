namespace ProgramStarter.App.Services;

public interface IFileDialogService
{
    string? OpenFileDialog(string title, string filter);
}
