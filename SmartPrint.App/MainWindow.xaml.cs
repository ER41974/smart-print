using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SmartPrint.Core.ViewModels;

namespace SmartPrint.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (DataContext is MainViewModel vm)
            {
                if (vm.AddFilesCommand.CanExecute(files))
                {
                    vm.AddFilesCommand.Execute(files);
                }
            }
        }
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsVm = App.Current.Services.GetRequiredService<SettingsViewModel>();
        var settingsWin = new SettingsWindow
        {
            DataContext = settingsVm,
            Owner = this
        };
        settingsWin.ShowDialog();
    }
}
