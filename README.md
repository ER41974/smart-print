# Smart Print

A fast queue ("tray/bac") for printing with per-document options.

## Features
- **Drag & Drop** files/folders into the queue.
- **Per-Document Settings**: Copies, Color/BW, Quality, Printer.
- **Bulk Edit**: Apply settings to multiple files at once.
- **Multi-Language**: English and French (Français).
- **Supported Formats**:
  - PDF
  - Images (PNG, JPG, JPEG, TIFF)
  - Office (DOCX, XLSX, PPTX) *Requires Microsoft Office installed*

## Build & Run
### Prerequisites
- .NET 8 SDK
- Windows 10/11 (for WPF and Printing APIs)

### Instructions
1. Clone the repository.
2. Open `SmartPrint.sln` in Visual Studio 2022.
3. Build and Run `SmartPrint.App`.

## Localization
Go to Settings -> Language to switch between English and Français.
The app may need a restart to fully apply the language change.

## Driver Limitations
- **Color/BW**: Only available if the printer driver exposes this capability via standard Windows APIs.
- **Quality**: Only Draft/Normal/High if supported. Otherwise "Default".
