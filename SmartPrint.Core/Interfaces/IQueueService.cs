using System.Collections.ObjectModel;
using SmartPrint.Core.Models;

namespace SmartPrint.Core.Interfaces;

public interface IQueueService
{
    ObservableCollection<PrintJob> Queue { get; }
    void AddFiles(string[] paths);
    void RemoveJob(PrintJob job);
    void ClearQueue();
    void ApplyDefaults(PrintJob job);
}
