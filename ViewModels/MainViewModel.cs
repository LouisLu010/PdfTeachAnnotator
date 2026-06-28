using System.Collections.ObjectModel;
using System.Windows.Ink;
using System.Windows.Input;
using Microsoft.Win32;
using PdfTeachAnnotator.Models;
using PdfTeachAnnotator.Services;

namespace PdfTeachAnnotator.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private const int PageBufferSize = 2;
    private const int PageVerticalMargin = 20;

    private readonly PdfRenderService _pdfService = new();
    private readonly AnnotationFileService _annotationService = new();
    private IOcrService? _ocrService;
    private readonly Stack<IAnnotationCommand> _undoStack = new();
    private readonly Stack<IAnnotationCommand> _redoStack = new();
    private bool _isApplyingHistory;
    private string? _currentPdfPath;
    private int _viewportWidth = 800;
    private int _targetPageWidth = 760;
    private int _loadedStartIndex = -1;
    private int _loadedEndIndex = -1;
    private int _firstVisiblePageIndex;
    private double _lastVerticalOffset = 0;
    private double _lastViewportHeight = 0;
    private double _zoomLevel = 1.0;
    private string _currentView = "Home";
    private string _previousView = "Home";
    private bool _isOcrRunning;
    private int _ocrCurrentPage;
    private int _ocrTotalPages;
    private string _ocrResult = string.Empty;

    public ObservableCollection<PageModel> Pages { get; } = new();
    public ToolbarViewModel Toolbar { get; } = new();
    public AppSettings Settings { get; } = AppSettings.Load();
    public ObservableCollection<RecentFile> RecentFiles { get; } = new();
    public string[] OcrEngines { get; } = [OcrEngineNames.Tesseract, OcrEngineNames.IronOcr, OcrEngineNames.PaddleOcr];

    public string SelectedOcrEngine
    {
        get => Settings.OcrEngine;
        set
        {
            if (Settings.OcrEngine == value) return;
            Settings.OcrEngine = value;
            Settings.Save();
            ResetOcrService();
            OnPropertyChanged();
            OnPropertyChanged(nameof(OcrEngineDescription));
        }
    }

    public string OcrEngineDescription => SelectedOcrEngine switch
    {
        OcrEngineNames.IronOcr => "商业 OCR 引擎，中文识别可尝试，可能显示授权水印。",
        OcrEngineNames.PaddleOcr => "高准确率中文 OCR，性能要求较高；当前为预留选项，需配置 PP-OCR 模型后启用。",
        _ => "默认本地 Tesseract OCR，离线可用，资源占用较低。"
    };

    public string CurrentView
    {
        get => _currentView;
        set
        {
            if (SetField(ref _currentView, value))
            {
                OnPropertyChanged(nameof(IsHomeView));
                OnPropertyChanged(nameof(IsPdfView));
                OnPropertyChanged(nameof(IsSettingsView));
                OnPropertyChanged(nameof(IsAboutView));
                OnPropertyChanged(nameof(IsToolboxView));
            }
        }
    }

    public string PreviousView
    {
        get => _previousView;
        set => SetField(ref _previousView, value);
    }

    public bool IsHomeView => CurrentView == "Home";
    public bool IsPdfView => CurrentView == "Pdf";
    public bool IsSettingsView => CurrentView == "Settings";
    public bool IsAboutView => CurrentView == "About";
    public bool IsToolboxView => CurrentView == "Toolbox";
    public bool HasRecentFiles => RecentFiles.Count == 0;

    public string? CurrentPdfPath
    {
        get => _currentPdfPath;
        private set
        {
            if (SetField(ref _currentPdfPath, value))
            {
                OnPropertyChanged(nameof(HasCurrentPdf));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool HasCurrentPdf => !string.IsNullOrEmpty(_currentPdfPath);

    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            if (SetField(ref _zoomLevel, Math.Clamp(value, 0.5, 4.0)))
            {
                OnPropertyChanged(nameof(ZoomPercent));
                UpdateVisiblePages(_lastVerticalOffset, _lastViewportHeight);
            }
        }
    }

    public string ZoomPercent => $"{ZoomLevel * 100:F0}%";

    public int FirstVisiblePageIndex
    {
        get => _firstVisiblePageIndex;
        private set => SetField(ref _firstVisiblePageIndex, value);
    }

    public string PageIndicatorText =>
        Pages.Count == 0 ? string.Empty : $"第 {FirstVisiblePageIndex + 1} 页 / 共 {Pages.Count} 页";

    public int ViewportWidth
    {
        get => _viewportWidth;
        set
        {
            if (SetField(ref _viewportWidth, value) && _currentPdfPath != null)
                ReloadPages();
        }
    }

    public bool IsSidebarCollapsed
    {
        get => Settings.IsSidebarCollapsed;
        set
        {
            if (Settings.IsSidebarCollapsed == value) return;
            Settings.IsSidebarCollapsed = value;
            Settings.Save();
            OnPropertyChanged();
        }
    }

    public bool IsOcrRunning
    {
        get => _isOcrRunning;
        set
        {
            if (SetField(ref _isOcrRunning, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public int OcrCurrentPage
    {
        get => _ocrCurrentPage;
        set => SetField(ref _ocrCurrentPage, value);
    }

    public int OcrTotalPages
    {
        get => _ocrTotalPages;
        set => SetField(ref _ocrTotalPages, value);
    }

    public string OcrResult
    {
        get => _ocrResult;
        set
        {
            if (SetField(ref _ocrResult, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public string OcrProgressText => _ocrTotalPages > 0 ? $"正在识别：{_ocrCurrentPage} / {_ocrTotalPages}" : "准备识别...";

    public bool IsDarkMode
    {
        get => string.Equals(Settings.Theme, "Dark", StringComparison.OrdinalIgnoreCase);
        set
        {
            var theme = value ? "Dark" : "Light";
            if (Settings.Theme == theme) return;
            Settings.Theme = theme;
            Settings.Save();
            OnPropertyChanged();
        }
    }

    public ICommand OpenFileCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ZoomResetCommand { get; }
    public ICommand SaveCommand { get; }
    private readonly RelayCommand _undoCommand;
    private readonly RelayCommand _redoCommand;

    public ICommand UndoCommand => _undoCommand;
    public ICommand RedoCommand => _redoCommand;
    public ICommand ShowHomeCommand { get; }
    public ICommand ShowToolboxCommand { get; }
    public ICommand ShowSettingsCommand { get; }
    public ICommand ShowAboutCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand StartOcrCommand { get; }
    public ICommand CopyOcrResultCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ToggleSidebarCommand { get; }

    public MainViewModel()
    {
        OpenFileCommand = new RelayCommand(OpenFile);
        ZoomInCommand = new RelayCommand(() => ZoomLevel += 0.1);
        ZoomOutCommand = new RelayCommand(() => ZoomLevel -= 0.1);
        ZoomResetCommand = new RelayCommand(() => ZoomLevel = 1.0);
        SaveCommand = new RelayCommand(SaveAnnotations);
        _undoCommand = new RelayCommand(Undo, () => _undoStack.Count > 0);
        _redoCommand = new RelayCommand(Redo, () => _redoStack.Count > 0);
        ShowHomeCommand = new RelayCommand(() =>
        {
            PreviousView = CurrentView;
            CurrentView = "Home";
        });
        ShowToolboxCommand = new RelayCommand(() =>
        {
            PreviousView = CurrentView;
            CurrentView = "Toolbox";
        });
        ShowSettingsCommand = new RelayCommand(() =>
        {
            PreviousView = CurrentView;
            CurrentView = "Settings";
        });
        ShowAboutCommand = new RelayCommand(() =>
        {
            PreviousView = CurrentView;
            CurrentView = "About";
        });
        GoBackCommand = new RelayCommand(() => CurrentView = PreviousView);
        StartOcrCommand = new RelayCommand(async () => await StartOcrAsync(), () => !string.IsNullOrEmpty(_currentPdfPath) && !_isOcrRunning);
        CopyOcrResultCommand = new RelayCommand(CopyOcrResult, () => !string.IsNullOrEmpty(_ocrResult));
        ToggleThemeCommand = new RelayCommand(() => IsDarkMode = !IsDarkMode);
        ToggleSidebarCommand = new RelayCommand(() => IsSidebarCollapsed = !IsSidebarCollapsed);
        Toolbar.ClearAllRequested += ClearAllStrokes;

        LoadRecentFiles();
        if (Settings.ShowWelcomeScreen)
            CurrentView = "Home";
    }

    public void LoadRecentFiles()
    {
        RecentFiles.Clear();
        foreach (var file in Settings.RecentFiles)
            RecentFiles.Add(file);
        OnPropertyChanged(nameof(HasRecentFiles));
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
        ClearHistory();
        ReloadPages();
        LoadAnnotations(path);

        // Add to recent files
        var hasAnnotations = _annotationService.LoadAnnotations(path) != null;
        Settings.AddRecentFile(path, _pdfService.PageCount, hasAnnotations);
        LoadRecentFiles();

        // Switch to PDF view
        CurrentView = "Pdf";
    }

    public void RegisterStrokeChange(
        int pageIndex,
        StrokeCollection added,
        StrokeCollection removed,
        IReadOnlyList<Stroke>? previousStrokes = null)
    {
        if (_isApplyingHistory)
            return;

        if (added.Count == 0 && removed.Count == 0)
            return;

        var page = Pages.FirstOrDefault(p => p.PageIndex == pageIndex);
        if (page == null)
            return;

        PushCommand(new StrokeChangeCommand(pageIndex, GetPageStrokes, page.Strokes, added, removed, previousStrokes));
    }

    private StrokeCollection? GetPageStrokes(int pageIndex) =>
        Pages.FirstOrDefault(page => page.PageIndex == pageIndex)?.Strokes;

    private void Undo()
    {
        if (_undoStack.Count == 0)
            return;

        var command = _undoStack.Pop();
        try
        {
            _isApplyingHistory = true;
            command.Undo();
            _redoStack.Push(command);
        }
        finally
        {
            _isApplyingHistory = false;
            RaiseHistoryChanged();
        }
    }

    private void Redo()
    {
        if (_redoStack.Count == 0)
            return;

        var command = _redoStack.Pop();
        try
        {
            _isApplyingHistory = true;
            command.Redo();
            _undoStack.Push(command);
        }
        finally
        {
            _isApplyingHistory = false;
            RaiseHistoryChanged();
        }
    }

    private void PushCommand(IAnnotationCommand command)
    {
        _undoStack.Push(command);
        _redoStack.Clear();
        RaiseHistoryChanged();
    }

    private void ClearHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        RaiseHistoryChanged();
    }

    private void RaiseHistoryChanged()
    {
        CommandManager.InvalidateRequerySuggested();
        _undoCommand.RaiseCanExecuteChanged();
        _redoCommand.RaiseCanExecuteChanged();
    }

    private void ReloadPages()
    {
        // Save existing strokes before clearing
        var existingStrokes = new Dictionary<int, StrokeCollection>();
        foreach (var page in Pages)
        {
            if (page.Strokes.Count > 0)
            {
                existingStrokes[page.PageIndex] = new StrokeCollection(page.Strokes);
            }
        }

        Pages.Clear();
        _loadedStartIndex = -1;
        _loadedEndIndex = -1;
        _firstVisiblePageIndex = 0;
        _targetPageWidth = Math.Max(400, _viewportWidth - 40);

        for (int i = 0; i < _pdfService.PageCount; i++)
        {
            var (pageWidth, pageHeight) = _pdfService.GetPageSize(i, _targetPageWidth);
            var pageModel = new PageModel
            {
                PageIndex = i,
                PageWidth = pageWidth,
                PageHeight = pageHeight
            };

            // Restore strokes if they existed
            if (existingStrokes.TryGetValue(i, out var strokes))
            {
                foreach (var stroke in strokes)
                    pageModel.Strokes.Add(stroke);
            }

            Pages.Add(pageModel);
        }

        OnPropertyChanged(nameof(PageIndicatorText));

        if (_lastViewportHeight > 0)
            UpdateVisiblePages(_lastVerticalOffset, _lastViewportHeight);
        else
            LoadPageImages(0, Math.Min(PageBufferSize, Pages.Count - 1));
    }

    public void UpdateVisiblePages(double verticalOffset, double viewportHeight)
    {
        _lastVerticalOffset = verticalOffset;
        _lastViewportHeight = viewportHeight;

        if (_currentPdfPath == null || Pages.Count == 0 || viewportHeight <= 0)
            return;

        var (firstVisibleIndex, lastVisibleIndex) = GetVisiblePageRange(verticalOffset, viewportHeight);
        LoadPageImages(firstVisibleIndex, lastVisibleIndex);

        if (firstVisibleIndex != _firstVisiblePageIndex)
        {
            FirstVisiblePageIndex = firstVisibleIndex;
            OnPropertyChanged(nameof(PageIndicatorText));
        }
    }

    private (int FirstIndex, int LastIndex) GetVisiblePageRange(double verticalOffset, double viewportHeight)
    {
        var visibleTop = Math.Max(0, verticalOffset);
        var visibleBottom = visibleTop + viewportHeight;
        var currentTop = 0.0;
        var firstVisibleIndex = -1;
        var lastVisibleIndex = -1;

        for (int i = 0; i < Pages.Count; i++)
        {
            var page = Pages[i];
            var itemHeight = page.PageHeight * ZoomLevel + PageVerticalMargin;
            var currentBottom = currentTop + itemHeight;

            if (currentBottom >= visibleTop && firstVisibleIndex == -1)
                firstVisibleIndex = i;

            if (currentTop <= visibleBottom)
                lastVisibleIndex = i;
            else
                break;

            currentTop = currentBottom;
        }

        if (firstVisibleIndex == -1)
            firstVisibleIndex = Pages.Count - 1;
        if (lastVisibleIndex == -1)
            lastVisibleIndex = firstVisibleIndex;

        return (firstVisibleIndex, lastVisibleIndex);
    }

    private void LoadPageImages(int firstVisibleIndex, int lastVisibleIndex)
    {
        if (Pages.Count == 0)
            return;

        var startIndex = Math.Max(0, firstVisibleIndex - PageBufferSize);
        var endIndex = Math.Min(Pages.Count - 1, lastVisibleIndex + PageBufferSize);

        if (startIndex == _loadedStartIndex && endIndex == _loadedEndIndex)
            return;

        if (_loadedStartIndex >= 0 && _loadedEndIndex >= _loadedStartIndex)
        {
            for (int i = _loadedStartIndex; i <= _loadedEndIndex; i++)
            {
                if (i < startIndex || i > endIndex)
                    Pages[i].Image = null;
            }
        }

        for (int i = startIndex; i <= endIndex; i++)
        {
            if (Pages[i].Image == null)
                Pages[i].Image = _pdfService.RenderPage(i, _targetPageWidth);
        }

        _loadedStartIndex = startIndex;
        _loadedEndIndex = endIndex;
    }

    private void LoadAnnotations(string pdfPath)
    {
        var annotations = _annotationService.LoadAnnotations(pdfPath);
        if (annotations == null) return;

        try
        {
            _isApplyingHistory = true;
            foreach (var page in Pages)
            {
                if (annotations.TryGetValue(page.PageIndex, out var strokes))
                {
                    page.Strokes.Clear();
                    foreach (var stroke in strokes)
                        page.Strokes.Add(stroke);
                }
            }
            ClearHistory();
        }
        finally
        {
            _isApplyingHistory = false;
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
        try
        {
            _isApplyingHistory = true;
            foreach (var page in Pages)
                page.Strokes.Clear();
            ClearHistory();
        }
        finally
        {
            _isApplyingHistory = false;
        }
    }

    private async Task StartOcrAsync()
    {
        if (string.IsNullOrEmpty(_currentPdfPath) || _isOcrRunning)
            return;

        IsOcrRunning = true;
        OcrResult = string.Empty;
        OcrCurrentPage = 0;
        OcrTotalPages = 0;

        try
        {
            var progress = new Progress<(int Current, int Total)>(p =>
            {
                OcrCurrentPage = p.Current;
                OcrTotalPages = p.Total;
                OnPropertyChanged(nameof(OcrProgressText));
            });

            OcrResult = await GetOcrService().RecognizePdfAsync(_currentPdfPath, progress);
        }
        catch (Exception ex)
        {
            OcrResult = $"OCR 识别失败：{ex.Message}\n\n{ex.StackTrace}";
        }
        finally
        {
            IsOcrRunning = false;
            OnPropertyChanged(nameof(OcrProgressText));
        }
    }

    private IOcrService GetOcrService()
    {
        _ocrService ??= OcrServiceFactory.Create(Settings.OcrEngine);
        return _ocrService;
    }

    private void ResetOcrService()
    {
        _ocrService?.Dispose();
        _ocrService = null;
    }

    private void CopyOcrResult()
    {
        if (!string.IsNullOrEmpty(_ocrResult))
        {
            System.Windows.Clipboard.SetText(_ocrResult);
        }
    }

    public void Dispose()
    {
        _pdfService.Dispose();
        _ocrService?.Dispose();
    }

    private interface IAnnotationCommand
    {
        void Undo();
        void Redo();
    }

    private sealed class StrokeChangeCommand : IAnnotationCommand
    {
        private readonly int _pageIndex;
        private readonly Func<int, StrokeCollection?> _getPageStrokes;
        private readonly List<StrokeRecord> _addedStrokes;
        private readonly List<StrokeRecord> _removedStrokes;

        public StrokeChangeCommand(
            int pageIndex,
            Func<int, StrokeCollection?> getPageStrokes,
            StrokeCollection targetStrokes,
            StrokeCollection added,
            StrokeCollection removed,
            IReadOnlyList<Stroke>? previousStrokes)
        {
            _pageIndex = pageIndex;
            _getPageStrokes = getPageStrokes;
            var currentStrokes = targetStrokes.ToList();
            _addedStrokes = CreateRecords(currentStrokes, added);
            _removedStrokes = CreateRecords(previousStrokes ?? currentStrokes, removed);
        }

        public void Undo()
        {
            var targetStrokes = _getPageStrokes(_pageIndex);
            if (targetStrokes == null)
                return;

            RemoveStrokes(targetStrokes, _addedStrokes);
            AddStrokes(targetStrokes, _removedStrokes);
        }

        public void Redo()
        {
            var targetStrokes = _getPageStrokes(_pageIndex);
            if (targetStrokes == null)
                return;

            RemoveStrokes(targetStrokes, _removedStrokes);
            AddStrokes(targetStrokes, _addedStrokes);
        }

        private static List<StrokeRecord> CreateRecords(IReadOnlyList<Stroke> strokeOrder, StrokeCollection strokes)
        {
            var records = new List<StrokeRecord>();
            foreach (var stroke in strokes)
            {
                var index = -1;
                for (var i = 0; i < strokeOrder.Count; i++)
                {
                    if (ReferenceEquals(strokeOrder[i], stroke))
                    {
                        index = i;
                        break;
                    }
                }

                records.Add(new StrokeRecord(stroke, index < 0 ? strokeOrder.Count : index));
            }

            return records;
        }

        private static void RemoveStrokes(StrokeCollection targetStrokes, IEnumerable<StrokeRecord> records)
        {
            foreach (var record in records)
                targetStrokes.Remove(record.Stroke);
        }

        private static void AddStrokes(StrokeCollection targetStrokes, IEnumerable<StrokeRecord> records)
        {
            foreach (var record in records.OrderBy(r => r.Index))
            {
                if (targetStrokes.Contains(record.Stroke))
                    continue;

                var index = Math.Clamp(record.Index, 0, targetStrokes.Count);
                targetStrokes.Insert(index, record.Stroke);
            }
        }
    }

    private sealed record StrokeRecord(Stroke Stroke, int Index);
}
