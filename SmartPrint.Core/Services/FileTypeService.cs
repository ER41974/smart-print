using System.IO;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;

namespace SmartPrint.Core.Services;

public class FileTypeService : IFileTypeService
{
    public FileType GetFileType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => FileType.Pdf,
            ".png" or ".jpg" or ".jpeg" or ".tiff" or ".tif" => FileType.Image,
            ".docx" or ".xlsx" or ".pptx" => FileType.Office,
            _ => FileType.Unknown
        };
    }
}
