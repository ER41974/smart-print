using System;
using System.Drawing.Printing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using PdfiumViewer;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;

// Resolve ambiguity for Image
using NativeImage = System.Drawing.Image;
using WpfImage = System.Windows.Controls.Image;
// Resolve ambiguity for PrintJobStatus (conflicts with System.Printing.PrintJobStatus)
using PrintJobStatus = SmartPrint.Core.Models.PrintJobStatus;

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
                printDocument.DefaultPageSettings.Landscape = job.Orientation == PrintOrientation.Landscape;

                // Force orientation on each page via QueryPageSettings event
                printDocument.QueryPageSettings += (s, e) =>
                {
                    e.PageSettings.Landscape = job.Orientation == PrintOrientation.Landscape;
                };

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
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using var server = new LocalPrintServer();
                    var queue = server.GetPrintQueue(job.SelectedPrinterName);

                    // Clone du ticket pour éviter de corrompre le profil de l'imprimante
                    var defaultTicket = queue.UserPrintTicket ?? queue.DefaultPrintTicket;
                    var ticket = defaultTicket.Clone();

                    ticket.PageOrientation = job.Orientation == PrintOrientation.Landscape
                        ? PageOrientation.Landscape
                        : PageOrientation.Portrait;
                    ticket.CopyCount = job.Copies;
                    ticket.OutputColor = job.IsColor ? OutputColor.Color : OutputColor.Monochrome;

                    var caps = queue.GetPrintCapabilities(ticket);

                    double pageWidth = caps.OrientedPageMediaWidth ?? 816;
                    double pageHeight = caps.OrientedPageMediaHeight ?? 1056;

                    // Forcer l'inversion des dimensions selon l'orientation désirée
                    if (job.Orientation == PrintOrientation.Landscape && pageWidth < pageHeight)
                    {
                        (pageWidth, pageHeight) = (pageHeight, pageWidth);
                    }
                    else if (job.Orientation == PrintOrientation.Portrait && pageWidth > pageHeight)
                    {
                        (pageWidth, pageHeight) = (pageHeight, pageWidth);
                    }

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(job.FilePath);
                    bitmap.EndInit();
                    bitmap.Freeze();

                    var fixedDoc = new FixedDocument();
                    var fixedPage = new FixedPage
                    {
                        Width = pageWidth,
                        Height = pageHeight
                    };

                    fixedDoc.DocumentPaginator.PageSize = new System.Windows.Size(pageWidth, pageHeight);

                    var image = new System.Windows.Controls.Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.Uniform,
                        Width = pageWidth,
                        Height = pageHeight
                    };

                    fixedPage.Children.Add(image);

                    var pageContent = new PageContent();
                    ((IAddChild)pageContent).AddChild(fixedPage);
                    fixedDoc.Pages.Add(pageContent);

                    var writer = PrintQueue.CreateXpsDocumentWriter(queue);
                    writer.Write(fixedDoc.DocumentPaginator, ticket);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (error != null)
            {
                throw new Exception($"Image Print Error: {error.Message}", error);
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
            
            // Refined Strategy: Visible=true, Minimize, DisplayAlerts=0
            wordApp.Visible = true;
            try { wordApp.WindowState = 2; } catch { } // wdWindowStateMinimize = 2
            try { wordApp.DisplayAlerts = 0; } catch { } // wdAlertsNone = 0

            // Store original printer to restore later (good practice)
            try { originalPrinter = wordApp.ActivePrinter; } catch { }

            // Set printer BEFORE opening document if possible, or try setting it on the app
            // Note: Setting ActivePrinter changes the system default in some Word versions.
            bool printerSet = false;
            try
            {
                wordApp.ActivePrinter = job.SelectedPrinterName;
                printerSet = true;
            }
            catch
            {
                // Fallback to default, but we should warn or just proceed if user accepts default.
                // For now, we proceed as the job has a selected printer which might match default.
            }

            // Open document
            // ReadOnly: true, Visible: true (since app is visible), AddToRecentFiles: false
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
                Visible: true
            );

            // Set Orientation
            try
            {
                // wdOrientPortrait = 0, wdOrientLandscape = 1
                doc.PageSetup.Orientation = job.Orientation == PrintOrientation.Landscape ? 1 : 0;
            }
            catch { }

            // Activate to ensure it's the active window for printing focus
            try { doc.Activate(); } catch { }
            
            // Short delay for stability
            System.Threading.Thread.Sleep(500);

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
            string suggestion = SmartPrint.Core.Resources.Strings.WordPrintSuggestion;
            string printerName = job.SelectedPrinterName ?? "Default";
            string fileName = Path.GetFileName(job.FilePath);
            string message = string.Format(SmartPrint.Core.Resources.Strings.WordPrintFailed, $"{ex.Message} (File: {fileName}, Printer: {printerName}). {suggestion}");
            throw new Exception(message, ex);
        }
        finally
        {
            // Reset alerts
            if (wordApp != null)
            {
                 try { wordApp.DisplayAlerts = -1; } catch { } // wdAlertsAll = -1
            }

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
