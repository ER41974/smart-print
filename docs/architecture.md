# Smart Print Architecture

## Overview
Smart Print is a WPF application using .NET 8 and the MVVM pattern (CommunityToolkit.Mvvm).

## Projects
- **SmartPrint.App**: The WPF application. Contains Views, ViewModels, and platform-specific implementations.
- **SmartPrint.Core**: The core library. Contains Models, Interfaces, and platform-agnostic Services.
- **SmartPrint.Tests**: xUnit test project for SmartPrint.Core.

## Service Boundary
- `IQueueService`: Manages the print queue (adding/removing jobs).
- `IPrinterService`: Handles printer enumeration and capability detection.
- `IPrintEngine`: Dispatches print jobs to the selected printer.
- `IFileTypeService`: Detects file types and validation.
- `ISettingsService`: Persists user preferences and queue defaults.
- `ILocalizationService`: Manages culture switching and resource access.

## Localization
We use standard `.resx` files for localization:
- `Strings.resx`: Default (English).
- `Strings.fr.resx`: French.
The `LocalizationService` switches `Thread.CurrentThread.CurrentUICulture` and notifies the UI to update or restart.

## Printer Capabilities
Printer capabilities (Color/BW, Quality) are detected using `System.Printing` (WPF).
- Capabilities are mapped to a `PrinterCapabilities` model in Core.
- Fallback logic: If a capability is not supported, the UI control is disabled or hidden.

## Printing Pipeline
- **PDF**: Uses `PdfiumViewer` to render PDF pages to images, then prints using `System.Drawing.Printing.PrintDocument`.
- **Images**: Uses `System.Drawing.Printing` directly.
- **Office**: Uses COM Interop (Word/Excel/PowerPoint) to print documents.
