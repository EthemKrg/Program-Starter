using ProgramStarter.App.ViewModels;
using System.Windows;

namespace ProgramStarter.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
