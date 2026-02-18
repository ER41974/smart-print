using SmartPrint.Core.Models;

namespace SmartPrint.Core.Interfaces;

public interface IFileTypeService
{
    FileType GetFileType(string path);
}
