using System.Windows;
using System.Windows.Input;
using ProgramStarter.App.Services;

namespace ProgramStarter.App.Views.Dialogs;

public partial class AppEditDialog : Window
{
    private readonly IFileDialogService _fileDialogService;

    public AppEditResult? Result { get; private set; }

    public string AppName { get; set; }
    public string AppPath { get; set; }

    public AppEditDialog(string currentName, string currentPath, IFileDialogService fileDialogService)
    {
        _fileDialogService = fileDialogService;

        InitializeComponent();

        AppName = currentName;
        AppPath = currentPath;

        NameTextBox.Text = currentName;
        PathTextBox.Text = currentPath;

        NameTextBox.SelectAll();

        DataContext = this;
    }

    private void OnBrowseClick(object sender, RoutedEventArgs e)
    {
        var selectedPath = _fileDialogService.OpenFileDialog(
            "Select an application",
            "Executable files (*.exe)|*.exe");

        if (selectedPath is not null)
        {
            AppPath = selectedPath;
            PathTextBox.Text = selectedPath;
        }
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text?.Trim() ?? string.Empty;
        var path = PathTextBox.Text?.Trim() ?? string.Empty;

        Result = new AppEditResult(name, path);
        DialogResult = true;
        Close();
    }

    private void OnNameKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OnOkClick(sender, e);
        }
    }

    private void OnPathKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OnOkClick(sender, e);
        }
    }
}
