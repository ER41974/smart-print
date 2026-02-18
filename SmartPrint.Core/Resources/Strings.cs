using System.Resources;
using System.Globalization;

namespace SmartPrint.Core.Resources;

public class Strings
{
    private static readonly ResourceManager _resourceManager =
        new ResourceManager("SmartPrint.Core.Resources.Strings", typeof(Strings).Assembly);

    public static string Get(string key)
    {
        return _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }

    public static string AppTitle => Get("AppTitle");
    public static string AddFiles => Get("AddFiles");
    public static string AddFolder => Get("AddFolder");
    public static string DragDropHint => Get("DragDropHint");
    public static string Settings => Get("Settings");
    public static string Language => Get("Language");
    public static string Printer => Get("Printer");
    public static string Copies => Get("Copies");
    public static string Color => Get("Color");
    public static string Quality => Get("Quality");
    public static string Status => Get("Status");
    public static string Apply => Get("Apply");
    public static string Cancel => Get("Cancel");
    public static string Print => Get("Print");
    public static string Remove => Get("Remove");
    public static string ClearQueue => Get("ClearQueue");
    public static string Default => Get("Default");
    public static string Draft => Get("Draft");
    public static string Normal => Get("Normal");
    public static string High => Get("High");
    public static string Queued => Get("Queued");
    public static string Printing => Get("Printing");
    public static string Completed => Get("Completed");
    public static string Error => Get("Error");
    public static string OpenLocation => Get("OpenLocation");
    public static string Unsupported => Get("Unsupported");
    public static string WordPrintFailed => Get("WordPrintFailed");
    public static string WordPrintSuggestion => Get("WordPrintSuggestion");
    public static string OfficeNotInstalled => Get("OfficeNotInstalled");
}
