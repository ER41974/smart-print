using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartPrint.Core.Models;

public partial class PrintSettings : ObservableObject
{
    [ObservableProperty]
    private string defaultPrinter = string.Empty;

    [ObservableProperty]
    private int defaultCopies = 1;

    [ObservableProperty]
    private bool defaultColor = true;

    [ObservableProperty]
    private PrintQuality defaultQuality = PrintQuality.Default;

    [ObservableProperty]
    private string language = "en";
}
