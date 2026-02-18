using System.Globalization;
using System.Threading;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Resources;

namespace SmartPrint.Core.Services;

public class LocalizationService : ILocalizationService
{
    public void SetLanguage(string cultureCode)
    {
        try
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        catch
        {
            // Fallback to English if invalid
            var culture = new CultureInfo("en");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }

    public string GetString(string key)
    {
        return Strings.Get(key);
    }
}
