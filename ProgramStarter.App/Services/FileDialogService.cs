using Microsoft.Win32;

namespace ProgramStarter.App.Services;

internal class FileDialogService : IFileDialogService
{
    public string? OpenFileDialog(string title, string filter)
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter,
            Multiselect = false,
            CheckFileExists = true
        };

        var result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }
}
