using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using SmartPrint.Core.Interfaces;

namespace SmartPrint.App.Services;

public class DialogService : IDialogService
{
    public Task<string[]> OpenFilesAsync()
    {
        var dlg = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "All Supported|*.pdf;*.png;*.jpg;*.jpeg;*.tiff;*.tif;*.docx;*.doc;*.xlsx;*.xls;*.pptx;*.ppt|PDF|*.pdf|Images|*.png;*.jpg;*.jpeg;*.tiff;*.tif|Office|*.docx;*.doc;*.xlsx;*.xls;*.pptx;*.ppt|All Files|*.*"
        };

        if (dlg.ShowDialog() == true)
        {
            return Task.FromResult(dlg.FileNames);
        }
        return Task.FromResult(Array.Empty<string>());
    }

    public Task<string> OpenFolderAsync()
    {
        using var dlg = new System.Windows.Forms.FolderBrowserDialog();
        // Requires UseWindowsForms=true in csproj
        var result = dlg.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
        {
            return Task.FromResult(dlg.SelectedPath);
        }
        return Task.FromResult(string.Empty);
    }

    public void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
