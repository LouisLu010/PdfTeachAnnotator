# PdfTeachAnnotator

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?style=flat-square&logo=windows)
![AI-Assisted](https://img.shields.io/badge/AI-Codex%20%2B%20GPT--5.6-10A37F?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

A Windows desktop application for classroom presentations, lesson preparation, and PDF annotation. It supports handwriting, highlighting, a laser pointer, offline OCR, and automatic annotation saving.

[Features](#features) | [Quick Start](#quick-start) | [Usage](#usage) | [AI-Assisted Development](#ai-assisted-development) | [Tech Stack](#tech-stack) | [Roadmap](#roadmap)

</div>

---

## Features

### PDF Reading and Annotation

- **PDF rendering and display**
  - Local PDF rendering powered by `Docnet.Core` and PDFium.
  - Multi-page documents, continuous scrolling, and a dynamic page indicator.
  - On-demand loading for visible pages to reduce resource usage in long documents.
  - Zoom levels from 50% to 400%, with toolbar controls and `Ctrl + Mouse Wheel`.

- **Handwriting tools**
  - Pen, highlighter, laser pointer, and eraser.
  - Twelve preset colors for common classroom annotation needs.
  - Five sizes each for the pen, laser pointer, highlighter, and eraser.
  - Laser strokes disappear automatically after a short delay.
  - Undo and redo support for correcting mistakes quickly.

- **Annotation management**
  - Annotations are stored in a sidecar file at `{pdfPath}.annotations`.
  - The original PDF is never modified.
  - Existing annotations are restored automatically when a PDF is reopened.
  - Manual saving and automatic saving when closing a file or the application.
  - Slide-to-confirm interaction for clearing all annotations.

### Interface Experience

- **Modern UI**
  - WPF interface inspired by Fluent Design.
  - Dark and light themes.
  - A collapsible sidebar that preserves icon navigation when collapsed.
  - Transition animations for sidebar and page changes.

- **Clear information architecture**
  - Home: open or drop a PDF and access recent files.
  - PDF editor: read, zoom, annotate, and save.
  - Toolbox: run OCR text recognition.
  - Settings: configure the theme, auto-save, recent files, default pen, and OCR information.
  - About: view the project summary and core features.

- **Classroom-friendly interaction**
  - Large touch targets for interactive whiteboards and touchscreens.
  - Drag-and-drop PDF opening.
  - Recent files for quick access to frequently used course materials.
  - A dark theme for reducing visual distraction in dim classrooms.

### Offline OCR

- Local OCR powered by `Tesseract 5.2.0`.
- Simplified Chinese and English recognition with `chi_sim + eng`.
- Whole-document recognition with page-level progress reporting.
- One-click copying of recognized text.
- PDFs and OCR results are never uploaded.

> The `main` branch uses Tesseract only. An experimental PaddleOCR implementation remains on the `feature/paddleocr` branch for separate validation and performance testing.

---

## Quick Start

### System Requirements

- **Operating system:** Windows 10/11 x64.
- **Memory:** 4 GB or more recommended.
- **Display:** 1920x1080 or higher recommended.
- **Runtime:** Release packages are self-contained for `win-x64`, so most users do not need to install the .NET Runtime separately.

### Option 1: Download a Prebuilt Release

1. Open the [Releases](https://github.com/LouisLu010/PdfTeachAnnotator/releases) page.
2. Download the latest `PdfTeachAnnotator-Release.zip`.
3. Extract it to any directory.
4. Double-click `PdfTeachAnnotator.lnk`, or run the included launch script.

> OCR requires the `tessdata` directory in the release package. The project currently includes `chi_sim.traineddata` and `eng.traineddata`.

### Option 2: Build from Source

```bash
git clone https://github.com/LouisLu010/PdfTeachAnnotator.git
cd PdfTeachAnnotator

dotnet build -c Release
dotnet run --project PdfTeachAnnotator.csproj
```

---

## Usage

### 1. Open a PDF

- Select **Open PDF File** on the home page.
- Drag a PDF file into the application window.
- Select a document from the recent files list.
- Press `Ctrl + O`.

The application currently supports `.pdf` files only.

### 2. Choose an Annotation Tool

- **Pen:** Write or draw standard annotations.
- **Highlighter:** Add semi-transparent emphasis.
- **Laser pointer:** Point at content during a presentation; strokes disappear automatically.
- **Eraser:** Remove existing annotations.

Use the toolbar to select the color, stroke size, and eraser size.

### 3. Save and Restore Annotations

- Press `Ctrl + S` to save the current annotations.
- When auto-save is enabled, annotations are saved when the file or application closes.
- Annotations are written to a `.annotations` file beside the PDF.
- The original PDF is not modified or overwritten.

### 4. Run OCR

1. Open a PDF.
2. Select **Toolbox** in the sidebar.
3. Select **Start Recognition** on the OCR panel.
4. Wait for processing to finish, then copy the recognized text.

Recognition accuracy depends on page clarity, font quality, scan resolution, and layout complexity.

### 5. Keyboard Shortcuts

| Shortcut | Action |
| --- | --- |
| `Ctrl + O` | Open a PDF |
| `Ctrl + S` | Save annotations |
| `Ctrl + Z` | Undo |
| `Ctrl + Y` | Redo |
| `Ctrl + Shift + Z` | Redo |
| `Ctrl + Mouse Wheel` | Zoom in or out |

---

## Project Highlights

### Designed for Teaching

PdfTeachAnnotator is not a general-purpose PDF editor. It focuses on classroom presentation and lesson preparation, minimizing the interactions required to write, highlight, erase, undo, and save.

### Preserves the Original PDF

Annotations are stored in a separate `.annotations` file. Teachers can mark up course materials freely while retaining a clean copy of the original document.

### Local and Offline

PDF rendering, annotation storage, and OCR all happen locally. The application is suitable for school intranets, offline classrooms, and privacy-sensitive teaching environments.

### Built for Large Displays and Touch

Touch-friendly controls, theme switching, a collapsible sidebar, and responsive feedback make the application suitable for interactive whiteboards, projectors, and Windows tablets.

---

## AI-Assisted Development

> **Codex and GPT-5.6 were used as development collaborators for PdfTeachAnnotator.**
>
> They accelerated exploration and iteration. The author retained responsibility for product decisions, code review, testing, and release quality.

| Tool | How it was used |
| --- | --- |
| **Codex** | Explored the codebase, traced WPF and MVVM flows, assisted with implementation and refactoring, investigated annotation and OCR behavior, checked builds, and improved project documentation. |
| **GPT-5.6** | Helped analyze classroom workflows, refine requirements, compare technical approaches, break features into implementation steps, evaluate UX tradeoffs, and shape project explanations. |

Examples of AI-assisted work included:

- Turning classroom needs into focused features such as the temporary laser pointer, touch-friendly controls, and sidecar annotation storage.
- Reasoning about visible-page rendering, zoom-aware tools, undo and redo state, and asynchronous OCR progress.
- Reviewing the separation between models, view models, services, and WPF views.
- Drafting and refining the README, project story, roadmap, and release-facing descriptions.

Codex and GPT-5.6 were used during development only. PdfTeachAnnotator does not call a hosted language model at runtime; PDF rendering, annotations, and OCR remain local to the user's computer.

---

## Tech Stack

### Core Technologies

- **Application framework:** `.NET 8` / `net8.0-windows`.
- **UI framework:** WPF.
- **Architecture:** MVVM, WPF Data Binding, `ICommand`, and `RelayCommand`.
- **PDF rendering:** `Docnet.Core 2.6.0` with native PDFium rendering.
- **Annotation system:** WPF `InkCanvas`, `StrokeCollection`, and `DrawingAttributes`.
- **OCR engine:** `Tesseract 5.2.0`.
- **Serialization:** `System.Text.Json`.
- **Release target:** Self-contained Windows x64.

### Project Structure

```text
PdfTeachAnnotator/
|-- Models/                    # Settings, pages, annotations, and recent-file models
|   |-- AnnotationFile.cs
|   |-- AppSettings.cs
|   |-- PageModel.cs
|   `-- RecentFile.cs
|-- ViewModels/                # MVVM view models and commands
|   |-- MainViewModel.cs
|   |-- ToolbarViewModel.cs
|   |-- ViewModelBase.cs
|   `-- RelayCommand.cs
|-- Services/                  # PDF, annotation, and OCR services
|   |-- AnnotationFileService.cs
|   |-- IOcrService.cs
|   |-- OcrServiceFactory.cs
|   |-- PdfRenderService.cs
|   `-- TesseractOcrService.cs
|-- Converters/
|   `-- Converters.cs
|-- Resources/                 # Colors, buttons, and theme resources
|   |-- Buttons.xaml
|   |-- Colors.xaml
|   `-- Theme.xaml
|-- tessdata/                  # Tesseract language data
|   |-- chi_sim.traineddata
|   `-- eng.traineddata
|-- MainWindow.xaml            # Main window UI
|-- MainWindow.xaml.cs         # Main window interaction logic
|-- App.xaml                   # Application resources
`-- PdfTeachAnnotator.csproj   # Project file
```

---

## Roadmap

### Completed

- [x] PDF opening, rendering, and continuous scrolling.
- [x] Pen, highlighter, laser pointer, and eraser.
- [x] Color and tool-size presets.
- [x] Annotation saving and restoration.
- [x] Undo and redo.
- [x] Recent files.
- [x] Dark and light themes.
- [x] Collapsible sidebar and animations.
- [x] Page transition animations.
- [x] Dynamic page indicators.
- [x] Offline Tesseract OCR.

### Planned

- [ ] Text annotations.
- [ ] Shape tools such as rectangles, circles, and arrows.
- [ ] Exporting PDFs with annotations.
- [ ] Annotation search.
- [ ] Layer-based annotation management.
- [ ] PaddleOCR branch validation and performance evaluation.

---

## Contributing

Issues, suggestions, and pull requests are welcome.

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/your-feature`.
3. Commit your changes: `git commit -m 'feat: add your feature'`.
4. Push the branch: `git push origin feature/your-feature`.
5. Open a pull request.

Report bugs or suggest features through [GitHub Issues](https://github.com/LouisLu010/PdfTeachAnnotator/issues).

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

## Author

**Hongjun Lu**

- GitHub: [@LouisLu010](https://github.com/LouisLu010)

---

## Acknowledgements

- [Docnet.Core](https://github.com/GowenGit/docnet) - .NET wrapper for PDFium rendering.
- [PDFium](https://pdfium.googlesource.com/pdfium/) - PDF rendering engine.
- [Tesseract OCR](https://github.com/tesseract-ocr/tesseract) - Local OCR engine.
- [.NET](https://dotnet.microsoft.com/) - Application platform.
- [LINUX DO](https://linux.do/) - A community for developers and technology enthusiasts.
- **Codex and GPT-5.6** - AI collaborators used for development, analysis, and documentation.

---

<div align="center">

**If this project helps you, please consider giving it a Star!**

Made with care by Hongjun Lu

</div>
