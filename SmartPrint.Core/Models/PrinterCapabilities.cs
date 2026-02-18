using System.Collections.Generic;

namespace SmartPrint.Core.Models;

public class PrinterCapabilities
{
    public bool SupportsColor { get; set; }
    public List<PrintQuality> SupportedQualities { get; set; } = new List<PrintQuality>();

    public bool SupportsQuality(PrintQuality quality)
    {
        return SupportedQualities.Contains(quality);
    }
}
