using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Moq;
using SmartPrint.Core.Interfaces;
using SmartPrint.Core.Models;
using SmartPrint.Core.ViewModels;
using Xunit;

namespace SmartPrint.Tests;

public class MainViewModelTests
{
    private readonly Mock<IQueueService> _mockQueue;
    private readonly Mock<IPrinterService> _mockPrinter;
    private readonly Mock<IPrintEngine> _mockEngine;
    private readonly Mock<ISettingsService> _mockSettings;
    private readonly Mock<ILocalizationService> _mockLoc;

    private readonly ObservableCollection<PrintJob> _queue;

    public MainViewModelTests()
    {
        _queue = new ObservableCollection<PrintJob>();
        _mockQueue = new Mock<IQueueService>();
        _mockQueue.Setup(q => q.Queue).Returns(_queue);

        _mockPrinter = new Mock<IPrinterService>();
        _mockPrinter.Setup(p => p.GetPrinters()).Returns(new[] { new Printer { Name = "P1" } });

        _mockEngine = new Mock<IPrintEngine>();
        _mockSettings = new Mock<ISettingsService>();
        _mockLoc = new Mock<ILocalizationService>();
    }

    [Fact]
    public void AddFilesCommand_ShouldCallQueueService()
    {
        var vm = new MainViewModel(_mockQueue.Object, _mockPrinter.Object, _mockEngine.Object, _mockSettings.Object, _mockLoc.Object);
        var files = new[] { "file1.pdf" };

        vm.AddFilesCommand.Execute(files);

        _mockQueue.Verify(q => q.AddFiles(files), Times.Once);
    }

    [Fact]
    public async Task PrintAllCommand_ShouldCallEngineForEachQueuedJob()
    {
        var job1 = new PrintJob { Status = PrintJobStatus.Queued };
        var job2 = new PrintJob { Status = PrintJobStatus.Completed };
        _queue.Add(job1);
        _queue.Add(job2);

        var vm = new MainViewModel(_mockQueue.Object, _mockPrinter.Object, _mockEngine.Object, _mockSettings.Object, _mockLoc.Object);

        await vm.PrintAllCommand.ExecuteAsync(null);

        _mockEngine.Verify(e => e.PrintAsync(job1), Times.Once);
        _mockEngine.Verify(e => e.PrintAsync(job2), Times.Never);
    }

    [Fact]
    public void ApplyAllSettingsToSelection_ShouldCopySettings()
    {
        var vm = new MainViewModel(_mockQueue.Object, _mockPrinter.Object, _mockEngine.Object, _mockSettings.Object, _mockLoc.Object);
        var source = new PrintJob { SelectedPrinterName = "P2", Copies = 5, IsColor = false, Quality = PrintQuality.Draft };
        var target = new PrintJob { SelectedPrinterName = "P1", Copies = 1, IsColor = true, Quality = PrintQuality.Normal };

        vm.SelectedJob = source;
        vm.ApplyAllSettingsToSelectionCommand.Execute(new[] { source, target });

        Assert.Equal("P2", target.SelectedPrinterName);
        Assert.Equal(5, target.Copies);
        Assert.False(target.IsColor);
        Assert.Equal(PrintQuality.Draft, target.Quality);
    }
}
