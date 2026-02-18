using System;
using System.IO;
using System.Text.Json;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;

namespace SmartPrint.Core.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private PrintSettings _settings = new();

    public PrintSettings Settings => _settings;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "SmartPrint");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "settings.json");
    }

    public void Load()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<PrintSettings>(json);
                if (settings != null)
                {
                    // Copy properties to existing instance to maintain reference
                    _settings.DefaultPrinter = settings.DefaultPrinter;
                    _settings.DefaultCopies = settings.DefaultCopies;
                    _settings.DefaultColor = settings.DefaultColor;
                    _settings.DefaultQuality = settings.DefaultQuality;
                    _settings.Language = settings.Language;
                    _settings.UiScale = settings.UiScale != 0 ? settings.UiScale : 1.0;
                }
            }
            catch (Exception)
            {
                // Log or ignore corrupt settings
            }
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception)
        {
            // Log error
        }
    }
}
