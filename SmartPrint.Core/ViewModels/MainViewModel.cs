using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;
using SmartPrint.Core.Resources;

namespace SmartPrint.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IQueueService _queueService;
    private readonly IPrinterService _printerService;
    private readonly IPrintEngine _printEngine;
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<Printer> printers = new();

    [ObservableProperty]
    private PrintJob? selectedJob;

    public ObservableCollection<PrintJob> Queue => _queueService.Queue;

    public MainViewModel(
        IQueueService queueService,
        IPrinterService printerService,
        IPrintEngine printEngine,
        ISettingsService settingsService,
        ILocalizationService localizationService)
    {
        _queueService = queueService;
        _printerService = printerService;
        _printEngine = printEngine;
        _settingsService = settingsService;
        _localizationService = localizationService;

        LoadPrinters();
    }

    private void LoadPrinters()
    {
        var list = _printerService.GetPrinters();
        Printers = new ObservableCollection<Printer>(list);
    }

    [RelayCommand]
    private void AddFiles(string[] files)
    {
        if (files != null && files.Length > 0)
        {
            _queueService.AddFiles(files);
        }
    }

    [RelayCommand]
    private void RemoveJob(PrintJob? job)
    {
        if (job != null)
        {
            _queueService.RemoveJob(job);
        }
    }

    [RelayCommand]
    private void ClearQueue()
    {
        _queueService.ClearQueue();
    }

    [RelayCommand]
    private async Task PrintAll()
    {
        var jobsToPrint = Queue.Where(j => j.Status == PrintJobStatus.Queued || j.Status == PrintJobStatus.Error).ToList();

        foreach (var job in jobsToPrint)
        {
             await _printEngine.PrintAsync(job);
        }
    }

    [RelayCommand]
    private void ApplyAllSettingsToSelection(System.Collections.IList? items)
    {
        if (SelectedJob == null || items == null) return;

        foreach (var item in items)
        {
            if (item is PrintJob job && job != SelectedJob)
            {
                job.SelectedPrinterName = SelectedJob.SelectedPrinterName;
                job.Copies = SelectedJob.Copies;
                job.IsColor = SelectedJob.IsColor;
                job.Quality = SelectedJob.Quality;
            }
        }
    }

    [RelayCommand]
    private void ResetDefaults(System.Collections.IList? items)
    {
        if (items == null) return;
        foreach (var item in items)
        {
            if (item is PrintJob job)
            {
                _queueService.ApplyDefaults(job);
            }
        }
    }
}
