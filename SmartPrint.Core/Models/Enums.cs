namespace SmartPrint.Core.Models;

public enum PrintJobStatus
{
    Queued,
    Printing,
    Completed,
    Error
}

public enum PrintQuality
{
    Default,
    Draft,
    Normal,
    High
}

public enum FileType
{
    Unknown,
    Pdf,
    Image,
    Office
}
