using System.Windows;
using System.Windows.Input;

namespace ProgramStarter.App.Views.Dialogs;

public partial class TextInputDialog : Window
{
    public string? Result { get; private set; }

    public string TitleText
    {
        get => Title;
        set => Title = value;
    }

    public string Message { get; }
    public string InputText { get; set; }

    public TextInputDialog(string title, string message, string initialValue = "")
    {
        InitializeComponent();

        Title = title;
        Message = message;
        InputText = initialValue;
        InputTextBox.Text = initialValue;
        InputTextBox.SelectAll();

        DataContext = this;
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        var text = InputTextBox.Text?.Trim() ?? string.Empty;

        if (text.Length == 0)
            return;

        Result = text;
        DialogResult = true;
        Close();
    }

    private void OnInputKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OnOkClick(sender, e);
        }
    }
}
