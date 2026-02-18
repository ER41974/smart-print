using System.Collections.Generic;
using SmartPrint.Core.Models;

namespace SmartPrint.Core.Interfaces;

public interface IPrinterService
{
    IEnumerable<Printer> GetPrinters();
    PrinterCapabilities GetCapabilities(string printerName);
    void RefreshPrinters();
}
