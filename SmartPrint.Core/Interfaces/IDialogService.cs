using System.Threading.Tasks;

namespace SmartPrint.Core.Interfaces;

public interface IDialogService
{
    Task<string[]> OpenFilesAsync();
    Task<string> OpenFolderAsync();
    void ShowError(string message);
}
