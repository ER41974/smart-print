using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartPrint.Core.Models;

public partial class PrintJob : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string filePath = string.Empty;

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private FileType type = FileType.Unknown;

    [ObservableProperty]
    private int copies = 1;

    [ObservableProperty]
    private bool isColor = true;

    [ObservableProperty]
    private PrintQuality quality = PrintQuality.Default;

    [ObservableProperty]
    private string? selectedPrinterName;

    [ObservableProperty]
    private PrintJobStatus status = PrintJobStatus.Queued;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    // Not serialized/persisted usually, but useful for runtime logic
    [ObservableProperty]
    private PrinterCapabilities? capabilities;
}
