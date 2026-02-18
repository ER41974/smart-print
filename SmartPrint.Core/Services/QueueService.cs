using System.Collections.ObjectModel;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;

namespace SmartPrint.Core.Services;

public class QueueService : IQueueService
{
    private readonly ISettingsService _settingsService;
    private readonly IFileTypeService _fileTypeService;
    private readonly ObservableCollection<PrintJob> _queue = new();

    public ObservableCollection<PrintJob> Queue => _queue;

    public QueueService(ISettingsService settingsService, IFileTypeService fileTypeService)
    {
        _settingsService = settingsService;
        _fileTypeService = fileTypeService;
    }

    public void AddFiles(string[] paths)
    {
        foreach (var path in paths)
        {
            var fileType = _fileTypeService.GetFileType(path);
            var job = new PrintJob
            {
                FilePath = path,
                FileName = Path.GetFileName(path),
                Type = fileType,
                SelectedPrinterName = _settingsService.Settings.DefaultPrinter,
                Copies = _settingsService.Settings.DefaultCopies,
                IsColor = _settingsService.Settings.DefaultColor,
                Quality = _settingsService.Settings.DefaultQuality
            };

            Queue.Add(job);
        }
    }

    public void RemoveJob(PrintJob job)
    {
        if (Queue.Contains(job))
        {
            Queue.Remove(job);
        }
    }

    public void ClearQueue()
    {
        Queue.Clear();
    }

    public void ApplyDefaults(PrintJob job)
    {
        job.SelectedPrinterName = _settingsService.Settings.DefaultPrinter;
        job.Copies = _settingsService.Settings.DefaultCopies;
        job.IsColor = _settingsService.Settings.DefaultColor;
        job.Quality = _settingsService.Settings.DefaultQuality;
    }
}
