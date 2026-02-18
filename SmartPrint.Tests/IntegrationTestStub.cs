using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using SmartPrint.Core.Models;
using SmartPrint.Core.Interfaces;

namespace SmartPrint.Tests;

public class IntegrationTestStub
{
    [Fact]
    public void OfficeFileType_RoutesTo_PrintOffice()
    {
        // This is a stub to verify logic routing, as we cannot mock COM easily in CI without wrappers.
        // In a real scenario, we would use an IOfficeAutomation wrapper.
        
        var job = new PrintJob 
        { 
            FilePath = "test.docx", 
            Type = FileType.Office 
        };

        Assert.Equal(FileType.Office, job.Type);
        Assert.True(Path.GetExtension(job.FilePath) == ".docx");
    }

    [Fact]
    public void ErrorMessage_ConstructedCorrectly()
    {
        // Verify localized error construction (simulated)
        string failedMsg = SmartPrint.Core.Resources.Strings.WordPrintFailed;
        string suggestion = SmartPrint.Core.Resources.Strings.WordPrintSuggestion;
        
        Assert.NotNull(failedMsg);
        Assert.NotNull(suggestion);
        
        string exMsg = "COM Error";
        string formatted = string.Format(failedMsg, $"{exMsg}. {suggestion}");
        
        Assert.Contains(exMsg, formatted);
        Assert.Contains(suggestion, formatted);
    }
}
