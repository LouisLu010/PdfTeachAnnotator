using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using PdfTeachAnnotator.Models;
using PdfTeachAnnotator.Services;

namespace PdfTeachAnnotator.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly PdfRenderService _pdfService = new();
    private readonly AnnotationFileService _annotationService = new();
    private string? _currentPdfPath;
    private int _viewportWidth = 800;
    private double _zoomLevel = 1.0;

    public ObservableCollection<PageModel> Pages { get; } = new();
    public ToolbarViewModel Toolbar { get; } = new();

    public string? CurrentPdfPath
    {
        get => _currentPdfPath;
        private set => SetField(ref _currentPdfPath, value);
    }

    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            if (SetField(ref _zoomLevel, Math.Clamp(value, 0.5, 4.0)))
                OnPropertyChanged(nameof(ZoomPercent));
        }
    }

    public string ZoomPercent => $"{ZoomLevel * 100:F0}%";

    public int ViewportWidth
    {
        get => _viewportWidth;
        set
        {
            if (SetField(ref _viewportWidth, value) && _currentPdfPath != null)
                ReloadPages();
        }
    }

    public ICommand OpenFileCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ZoomResetCommand { get; }
    public ICommand SaveCommand { get; }

    public MainViewModel()
    {
        OpenFileCommand = new RelayCommand(OpenFile);
        ZoomInCommand = new RelayCommand(() => ZoomLevel += 0.1);
        ZoomOutCommand = new RelayCommand(() => ZoomLevel -= 0.1);
        ZoomResetCommand = new RelayCommand(() => ZoomLevel = 1.0);
        SaveCommand = new RelayCommand(SaveAnnotations);
        Toolbar.ClearAllRequested += ClearAllStrokes;
    }

    private void OpenFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            Title = "Open PDF"
        };

        if (dialog.ShowDialog() == true)
            LoadPdf(dialog.FileName);
    }

    public void LoadPdf(string path)
    {
        _pdfService.OpenDocument(path);
        CurrentPdfPath = path;
        ReloadPages();
        LoadAnnotations(path);
    }

    private void ReloadPages()
    {
        Pages.Clear();
        int targetWidth = Math.Max(400, _viewportWidth - 40);
        var rendered = _pdfService.RenderAllPages(targetWidth);
        for (int i = 0; i < rendered.Count; i++)
        {
            Pages.Add(new PageModel { PageIndex = i, Image = rendered[i] });
        }
    }

    private void LoadAnnotations(string pdfPath)
    {
        var annotations = _annotationService.LoadAnnotations(pdfPath);
        if (annotations == null) return;

        foreach (var page in Pages)
        {
            if (annotations.TryGetValue(page.PageIndex, out var strokes))
            {
                page.Strokes.Clear();
                foreach (var stroke in strokes)
                    page.Strokes.Add(stroke);
            }
        }
    }

    private void SaveAnnotations()
    {
        if (_currentPdfPath == null) return;

        var pageStrokes = new Dictionary<int, System.Windows.Ink.StrokeCollection>();
        foreach (var page in Pages)
        {
            if (page.Strokes.Count > 0)
                pageStrokes[page.PageIndex] = page.Strokes;
        }

        _annotationService.SaveAnnotations(_currentPdfPath, pageStrokes);
    }

    private void ClearAllStrokes()
    {
        foreach (var page in Pages)
            page.Strokes.Clear();
    }

    public void Dispose()
    {
        _pdfService.Dispose();
    }
}
