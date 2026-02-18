namespace SmartPrint.Core.Models;

public class Printer
{
    public string Name { get; set; } = string.Empty;
    public PrinterCapabilities Capabilities { get; set; } = new PrinterCapabilities();

    public override string ToString()
    {
        return Name;
    }
}
