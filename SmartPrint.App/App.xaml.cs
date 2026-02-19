using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SmartPrint.App.Services;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Services;
using SmartPrint.Core.ViewModels;

namespace SmartPrint.App;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; }

    public new static App Current => (App)Application.Current;

    public App()
    {
        Services = ConfigureServices();
        // InitializeComponent(); // Not strictly needed if no InitializeComponent method generated or if using OnStartup
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Core Services
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IFileTypeService, FileTypeService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();

        // IQueueService needs to be Singleton because it holds the queue state
        services.AddSingleton<IQueueService, QueueService>();

        // Platform Services
        services.AddSingleton<IPrinterService, PrinterService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddTransient<IPrintEngine, PrintEngine>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        this.DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"An unhandled exception occurred: {args.Exception.Message}\n\n{args.Exception.StackTrace}",
                "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
            Current.Shutdown();
        };

        var settings = Services.GetRequiredService<ISettingsService>();
        settings.Load();

        var localization = Services.GetRequiredService<ILocalizationService>();
        localization.SetLanguage(settings.Settings.Language);

        var mainWindow = new MainWindow();
        mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }
}
