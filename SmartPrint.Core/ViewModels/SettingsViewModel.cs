using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;

namespace SmartPrint.Core.ViewModels;

public class LanguageItem
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _localizationService;
    private readonly IPrinterService _printerService;

    [ObservableProperty]
    private PrintSettings tempSettings;

    [ObservableProperty]
    private ObservableCollection<Printer> printers = new();

    public List<LanguageItem> Languages { get; } = new()
    {
        new LanguageItem { Code = "en", Name = "English" },
        new LanguageItem { Code = "fr", Name = "Français" }
    };

    public List<double> UiScales { get; } = new() { 1.0, 1.1, 1.25, 1.5 };

    public SettingsViewModel(ISettingsService settingsService, ILocalizationService localizationService, IPrinterService printerService)
    {
        _settingsService = settingsService;
        _localizationService = localizationService;
        _printerService = printerService;

        var current = _settingsService.Settings;
        TempSettings = new PrintSettings
        {
            DefaultPrinter = current.DefaultPrinter,
            DefaultCopies = current.DefaultCopies,
            DefaultColor = current.DefaultColor,
            DefaultQuality = current.DefaultQuality,
            Language = current.Language,
            UiScale = current.UiScale
        };

        LoadPrinters();
    }

    private void LoadPrinters()
    {
        var list = _printerService.GetPrinters();
        Printers = new ObservableCollection<Printer>(list);
    }

    [RelayCommand]
    private void Save()
    {
        var current = _settingsService.Settings;
        current.DefaultPrinter = TempSettings.DefaultPrinter;
        current.DefaultCopies = TempSettings.DefaultCopies;
        current.DefaultColor = TempSettings.DefaultColor;
        current.DefaultQuality = TempSettings.DefaultQuality;
        current.Language = TempSettings.Language;
        current.UiScale = TempSettings.UiScale;

        _settingsService.Save();
        _localizationService.SetLanguage(current.Language);

        // Signal view to close via Message or Action?
        // Since I'm using code-behind to open dialog, I should use event or callback.
        OnRequestClose?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        OnRequestClose?.Invoke();
    }

    public delegate void CloseHandler();
    public event CloseHandler? OnRequestClose;
}
