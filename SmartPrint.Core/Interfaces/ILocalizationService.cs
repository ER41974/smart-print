namespace SmartPrint.Core.Interfaces;

public interface ILocalizationService
{
    void SetLanguage(string cultureCode);
    string GetString(string key);
}
