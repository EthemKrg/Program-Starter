using System.Windows;

namespace ProgramStarter.App.Views.Dialogs;

public partial class ConfirmDialog : Window
{
    public bool Result { get; private set; }

    public string TitleText { get; }
    public string Message { get; }
    public string ConfirmText { get; }

    public ConfirmDialog(string title, string message, string confirmText = "Delete")
    {
        InitializeComponent();

        TitleText = title;
        Message = message;
        ConfirmText = confirmText;

        DataContext = this;
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        Result = true;
        DialogResult = true;
        Close();
    }
}
