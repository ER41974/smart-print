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
            string ext = Path.GetExtension(job.FilePath).ToLowerInvariant();
            if (ext == ".docx" || ext == ".doc")
            {
                PrintWord(job);
            }
            else
            {
                // Fallback for others or implement similar logic later
                PrintLegacyOffice(job);
            }
        });
    }

    private void PrintWord(PrintJob job)
    {
        dynamic wordApp = null;
        dynamic originalPrinter = null;
        dynamic doc = null;

        try
        {
            Type wordType = Type.GetTypeFromProgID("Word.Application");
            if (wordType == null)
            {
                throw new NotSupportedException(SmartPrint.Core.Resources.Strings.OfficeNotInstalled);
            }

            wordApp = Activator.CreateInstance(wordType);
            wordApp.Visible = false;
            // wordApp.ScreenUpdating = false; // Optional, might improve performance

            // Store original printer to restore later (good practice)
            try { originalPrinter = wordApp.ActivePrinter; } catch { }

            // Set printer BEFORE opening document if possible, or try setting it on the app
            // Note: Setting ActivePrinter changes the system default in some Word versions.
            // Safe approach: rely on PrintOut arguments if supported, or set active printer carefully.
            try
            {
                wordApp.ActivePrinter = job.SelectedPrinterName;
            }
            catch
            {
                // If setting printer fails, maybe it's invalid or Word blocks it.
                // We proceed hoping default is okay or user accepts it.
                // But typically this is required.
            }

            // Open document
            // ReadOnly: true, Visible: false, AddToRecentFiles: false
            doc = wordApp.Documents.Open(
                FileName: job.FilePath,
                ConfirmConversions: false,
                ReadOnly: true,
                AddToRecentFiles: false,
                PasswordDocument: "",
                PasswordTemplate: "",
                Revert: false,
                WritePasswordDocument: "",
                WritePasswordTemplate: "",
                Format: 0, // wdOpenFormatAuto
                Encoding: 0,
                Visible: false
            );

            // Double check printer if we can
            // doc.ActiveWindow... NO! Do not use ActiveWindow.

            // Print
            // Background: false IS CRITICAL to wait for completion
            doc.PrintOut(
                Background: false,
                Append: false,
                Range: 0, // wdPrintAllDocument
                Item: 0, // wdPrintDocumentContent
                Copies: job.Copies,
                PageType: 0, // wdPrintAllPages
                PrintToFile: false,
                Collate: true,
                ManualDuplexPrint: false
            );
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format(SmartPrint.Core.Resources.Strings.WordPrintFailed, ex.Message), ex);
        }
        finally
        {
            // Close document
            if (doc != null)
            {
                try
                {
                    // wdDoNotSaveChanges = 0
                    doc.Close(SaveChanges: 0);
                }
                catch { }
                try { System.Runtime.InteropServices.Marshal.FinalReleaseComObject(doc); } catch { }
            }

            // Restore printer (optional)
            if (wordApp != null && originalPrinter != null)
            {
                try { wordApp.ActivePrinter = originalPrinter; } catch { }
            }

            // Quit Word
            if (wordApp != null)
            {
                try
                {
                    // wdDoNotSaveChanges = 0
                    wordApp.Quit(SaveChanges: 0);
                }
                catch { }
                try { System.Runtime.InteropServices.Marshal.FinalReleaseComObject(wordApp); } catch { }
            }

            doc = null;
            wordApp = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private void PrintLegacyOffice(PrintJob job)
    {
         dynamic app = null;
         try
         {
             string ext = Path.GetExtension(job.FilePath).ToLowerInvariant();
             string progId = ext switch
             {
                 ".xlsx" or ".xls" => "Excel.Application",
                 ".pptx" or ".ppt" => "PowerPoint.Application",
                 _ => null
             };

             if (progId == null) throw new NotSupportedException(SmartPrint.Core.Resources.Strings.Unsupported);

             Type officeType = Type.GetTypeFromProgID(progId);
             if (officeType == null) throw new NotSupportedException(SmartPrint.Core.Resources.Strings.OfficeNotInstalled);

             app = Activator.CreateInstance(officeType);
             app.Visible = false;

             if (progId.Contains("Excel"))
             {
                 dynamic wb = app.Workbooks.Open(job.FilePath, ReadOnly: true);
                 wb.PrintOut();
                 wb.Close(SaveChanges: false);
             }
             else if (progId.Contains("PowerPoint"))
             {
                 // ReadOnly: -1 (msoTrue), WithWindow: 0 (msoFalse)
                 dynamic pres = app.Presentations.Open(job.FilePath, ReadOnly: -1, Untitled: 0, WithWindow: 0);
                 pres.PrintOptions.ActivePrinter = job.SelectedPrinterName;
                 pres.PrintOut();
                 pres.Close();
             }
         }
         catch (Exception ex)
         {
             throw new Exception($"Legacy Office Print Error: {ex.Message}", ex);
         }
         finally
         {
             if (app != null)
             {
                 try { app.Quit(); } catch {}
                 try { System.Runtime.InteropServices.Marshal.ReleaseComObject(app); } catch {}
             }
         }
    }
}
