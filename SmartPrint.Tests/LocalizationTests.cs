using System.Globalization;
using Xunit;
using SmartPrint.Core.Resources;
using SmartPrint.Core.Services;

namespace SmartPrint.Tests;

public class LocalizationTests
{
    [Fact]
    public void DefaultLanguageIsEnglish()
    {
        var service = new LocalizationService();
        service.SetLanguage("en");

        Assert.Equal("Smart Print", Strings.AppTitle);
        Assert.Equal("Add Files", Strings.AddFiles);
    }

    [Fact]
    public void CanSwitchToFrench()
    {
        var service = new LocalizationService();
        service.SetLanguage("fr");

        // This might fail if resources are not correctly embedded or found
        // But let's try.
        Assert.Equal("Ajouter fichiers", Strings.AddFiles);
    }
}
