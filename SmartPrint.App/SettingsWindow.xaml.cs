using System.Windows;
using SmartPrint.Core.ViewModels;

namespace SmartPrint.App;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            if (DataContext is SettingsViewModel vm)
            {
                vm.OnRequestClose += () => this.DialogResult = true;
            }
        };
    }
}
