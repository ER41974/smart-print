using System.Threading.Tasks;
using SmartPrint.Core.Models;

namespace SmartPrint.Core.Interfaces;

public interface IPrintEngine
{
    Task PrintAsync(PrintJob job);
    Task CancelAsync(PrintJob job);
}
