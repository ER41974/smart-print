using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;

namespace SmartPrint.App.Services;

public class PrinterService : IPrinterService
{
    public IEnumerable<Printer> GetPrinters()
    {
        var printers = new List<Printer>();
        try
        {
            // Use LocalPrintServer to get printers
            using var server = new LocalPrintServer();
            var queues = server.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });

            foreach (var queue in queues)
            {
                var printer = new Printer
                {
                    Name = queue.Name,
                    Capabilities = GetCapabilitiesInternal(queue)
                };
                printers.Add(printer);
            }
        }
        catch (Exception)
        {
            // On Linux or error, return empty list or mock
        }
        return printers;
    }

    public PrinterCapabilities GetCapabilities(string printerName)
    {
        try
        {
            using var server = new LocalPrintServer();
            var queue = server.GetPrintQueue(printerName);
            return GetCapabilitiesInternal(queue);
        }
        catch
        {
            return new PrinterCapabilities();
        }
    }

    public void RefreshPrinters()
    {
        // No-op for now
    }

    private PrinterCapabilities GetCapabilitiesInternal(PrintQueue queue)
    {
        var caps = new PrinterCapabilities();
        try
        {
            var printCaps = queue.GetPrintCapabilities();

            // Color support
            if (printCaps.OutputColorCapability.Contains(OutputColor.Color))
            {
                caps.SupportsColor = true;
            }

            // Quality support - Default is always there
            caps.SupportedQualities.Add(PrintQuality.Default);

            // Checking for Draft/Normal/High is complex with just System.Printing
            // Often mapped to PageResolution or specific driver features.
            // For MVP, we stick to Default to ensure reliability.
            // If we wanted to support it, we'd need to check PageResolutionCapability
            // and try to map low/med/high DPI to qualities.
        }
        catch
        {
            caps.SupportedQualities.Add(PrintQuality.Default);
        }
        return caps;
    }
}
