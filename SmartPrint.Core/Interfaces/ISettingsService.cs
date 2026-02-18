using SmartPrint.Core.Models;

namespace SmartPrint.Core.Interfaces;

public interface ISettingsService
{
    PrintSettings Settings { get; }
    void Load();
    void Save();
}
