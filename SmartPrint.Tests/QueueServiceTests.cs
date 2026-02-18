using System.Linq;
using Moq;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;
using SmartPrint.Core.Services;
using Xunit;

namespace SmartPrint.Tests;

public class QueueServiceTests
{
    private readonly Mock<ISettingsService> _mockSettings;
    private readonly Mock<IFileTypeService> _mockFileType;
    private readonly QueueService _service;

    public QueueServiceTests()
    {
        _mockSettings = new Mock<ISettingsService>();
        _mockSettings.Setup(s => s.Settings).Returns(new PrintSettings
        {
            DefaultPrinter = "TestPrinter",
            DefaultCopies = 2,
            DefaultColor = false,
            DefaultQuality = PrintQuality.Draft
        });

        _mockFileType = new Mock<IFileTypeService>();
        _mockFileType.Setup(f => f.GetFileType(It.IsAny<string>())).Returns(FileType.Pdf);

        _service = new QueueService(_mockSettings.Object, _mockFileType.Object);
    }

    [Fact]
    public void AddFiles_ShouldAddJobToQueue()
    {
        _service.AddFiles(new[] { "test.pdf" });

        Assert.Single(_service.Queue);
        var job = _service.Queue.First();
        Assert.Equal("test.pdf", job.FilePath);
        Assert.Equal(FileType.Pdf, job.Type);
    }

    [Fact]
    public void AddFiles_ShouldApplyDefaults()
    {
        _service.AddFiles(new[] { "test.pdf" });

        var job = _service.Queue.First();
        Assert.Equal("TestPrinter", job.SelectedPrinterName);
        Assert.Equal(2, job.Copies);
        Assert.False(job.IsColor);
        Assert.Equal(PrintQuality.Draft, job.Quality);
    }

    [Fact]
    public void RemoveJob_ShouldRemoveFromQueue()
    {
        _service.AddFiles(new[] { "test.pdf" });
        var job = _service.Queue.First();

        _service.RemoveJob(job);
        Assert.Empty(_service.Queue);
    }

    [Fact]
    public void ClearQueue_ShouldRemoveAllJobs()
    {
        _service.AddFiles(new[] { "test1.pdf", "test2.pdf" });

        _service.ClearQueue();
        Assert.Empty(_service.Queue);
    }
}
