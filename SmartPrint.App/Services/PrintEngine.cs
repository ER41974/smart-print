using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Threading.Tasks;
using PdfiumViewer;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;

namespace SmartPrint.App.Services;

public class PrintEngine : IPrintEngine
{
    private readonly IPrinterService _printerService;

    public PrintEngine(IPrinterService printerService)
    {
        _printerService = printerService;
    }

    public async Task PrintAsync(PrintJob job)
    {
        job.Status = PrintJobStatus.Printing;
        job.StatusMessage = string.Empty;

        try
        {
            switch (job.Type)
            {
                case FileType.Pdf:
                    await PrintPdf(job);
                    break;
                case FileType.Image:
                    await PrintImage(job);
                    break;
                case FileType.Office:
                    await PrintOffice(job);
                    break;
                default:
                    throw new NotSupportedException("File type not supported.");
            }
            job.Status = PrintJobStatus.Completed;
        }
        catch (Exception ex)
        {
            job.Status = PrintJobStatus.Error;
            job.StatusMessage = ex.Message;
        }
    }

    public Task CancelAsync(PrintJob job)
    {
        return Task.CompletedTask;
    }

    private async Task PrintPdf(PrintJob job)
    {
        await Task.Run(() =>
        {
            try
            {
                using var document = PdfDocument.Load(job.FilePath);
                using var printDocument = document.CreatePrintDocument();

                printDocument.PrinterSettings.PrinterName = job.SelectedPrinterName;
                printDocument.PrinterSettings.Copies = (short)job.Copies;

                if (job.IsColor)
                {
                    printDocument.DefaultPageSettings.Color = true;
                }
                else
                {
                    printDocument.DefaultPageSettings.Color = false;
                }

                printDocument.Print();
            }
            catch (Exception ex)
            {
                throw new Exception($"PDF Print Error: {ex.Message}", ex);
            }
        });
    }

    private async Task PrintImage(PrintJob job)
    {
        await Task.Run(() =>
        {
            try
            {
                using var pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = job.SelectedPrinterName;
                pd.PrinterSettings.Copies = (short)job.Copies;
                pd.DefaultPageSettings.Color = job.IsColor;

                using var img = Image.FromFile(job.FilePath);

                pd.PrintPage += (s, e) =>
                {
                    var m = e.MarginBounds;

                    if (m.Width > 0 && m.Height > 0)
                    {
                         float ratio = Math.Min((float)m.Width / img.Width, (float)m.Height / img.Height);
                         int width = (int)(img.Width * ratio);
                         int height = (int)(img.Height * ratio);

                         int x = m.Left + (m.Width - width) / 2;
                         int y = m.Top + (m.Height - height) / 2;

                         e.Graphics.DrawImage(img, x, y, width, height);
                    }
                    else
                    {
                        e.Graphics.DrawImage(img, 0, 0);
                    }
                    e.HasMorePages = false;
                };

                pd.Print();
            }
            catch (Exception ex)
            {
                throw new Exception($"Image Print Error: {ex.Message}", ex);
            }
        });
    }

    private async Task PrintOffice(PrintJob job)
    {
        await Task.Run(() =>
        {
            dynamic app = null;
            try
            {
                string ext = Path.GetExtension(job.FilePath).ToLowerInvariant();
                string progId = ext switch
                {
                    ".docx" or ".doc" => "Word.Application",
                    ".xlsx" or ".xls" => "Excel.Application",
                    ".pptx" or ".ppt" => "PowerPoint.Application",
                    _ => null
                };

                if (progId == null) throw new NotSupportedException("Unknown Office format.");

                Type officeType = Type.GetTypeFromProgID(progId);
                if (officeType == null) throw new NotSupportedException("Office not installed.");

                app = Activator.CreateInstance(officeType);
                app.Visible = false;

                if (progId.Contains("Word"))
                {
                    dynamic doc = app.Documents.Open(job.FilePath, ReadOnly: true, Visible: false);
                    app.ActivePrinter = job.SelectedPrinterName;

                    // Wait for background print to finish or disable it
                    doc.PrintOut(Background: false);

                    doc.Close(SaveChanges: false);
                }
                else if (progId.Contains("Excel"))
                {
                    dynamic wb = app.Workbooks.Open(job.FilePath, ReadOnly: true);
                    // app.ActivePrinter = ...; // Skip for now
                    wb.PrintOut();
                    wb.Close(SaveChanges: false);
                }
                else if (progId.Contains("PowerPoint"))
                {
                    dynamic pres = app.Presentations.Open(job.FilePath, ReadOnly: -1 /*msoTrue*/, Untitled: 0 /*msoFalse*/, WithWindow: 0 /*msoFalse*/);
                    pres.PrintOptions.ActivePrinter = job.SelectedPrinterName;
                    pres.PrintOut();
                    pres.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Office Print Error: {ex.Message}", ex);
            }
            finally
            {
                if (app != null)
                {
                    try { app.Quit(); } catch {}
                    try { System.Runtime.InteropServices.Marshal.ReleaseComObject(app); } catch {}
                }
            }
        });
    }
}
