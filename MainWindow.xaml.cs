using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using PdfTeachAnnotator.Models;
using PdfTeachAnnotator.ViewModels;
using Wpf.Ui.Controls;

namespace PdfTeachAnnotator;

public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;
    private readonly Dictionary<StrokeCollection, int> _trackedStrokePages = new();
    private readonly Dictionary<StrokeCollection, List<Stroke>> _strokeSnapshots = new();
    private ScrollViewer? _pdfScrollViewer;
    private readonly Dictionary<InkCanvas, StrokeCollection> _eraserBatchedRemovals = new();
    private readonly Dictionary<InkCanvas, List<Stroke>> _eraserBatchSnapshots = new();

    public MainViewModel ViewModel => _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        _viewModel.Toolbar.ConfirmClearAll = () =>
            System.Windows.MessageBox.Show("确定要清除所有批注吗？", "确认清除",
                System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.OK;

        _viewModel.Toolbar.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(ToolbarViewModel.EraserSize) or nameof(ToolbarViewModel.ActiveTool))
                UpdateEraserShapes();
        };

        InputBindings.Add(new KeyBinding(_viewModel.OpenFileCommand, Key.O, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(_viewModel.SaveCommand, Key.S, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(_viewModel.UndoCommand, Key.Z, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(_viewModel.RedoCommand, Key.Y, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(_viewModel.RedoCommand, Key.Z, ModifierKeys.Control | ModifierKeys.Shift));

        _viewModel.Pages.CollectionChanged += Pages_CollectionChanged;
    }

    private void Pages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (PageModel page in e.OldItems)
                UntrackStrokeCollection(page.Strokes);
        }

        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            ResetTrackedStrokeCollections();
    }

    private void UpdateEraserShapes()
    {
        var size = _viewModel.Toolbar.EraserSize;
        var shape = new RectangleStylusShape(size, size);
        foreach (var inkCanvas in FindVisualChildren<InkCanvas>(this))
            inkCanvas.EraserShape = shape;
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t) yield return t;
            foreach (var sub in FindVisualChildren<T>(child))
                yield return sub;
        }
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            _viewModel.ZoomLevel += e.Delta > 0 ? 0.1 : -0.1;
        }
    }

    private void InkCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not InkCanvas inkCanvas || inkCanvas.DataContext is not PageModel page)
            return;

        TrackStrokeCollection(page);
        inkCanvas.EraserShape = new RectangleStylusShape(_viewModel.Toolbar.EraserSize, _viewModel.Toolbar.EraserSize);
        inkCanvas.PreviewMouseDown += InkCanvas_PreviewMouseDown;
        inkCanvas.PreviewMouseUp += InkCanvas_PreviewMouseUp;
    }

    private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e) =>
        CommandManager.InvalidateRequerySuggested();

    private void InkCanvas_StrokeErased(object sender, RoutedEventArgs e) =>
        CommandManager.InvalidateRequerySuggested();

    private void InkCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not InkCanvas inkCanvas)
            return;

        if (_viewModel.Toolbar.ActiveTool == ToolbarViewModel.ToolMode.Eraser)
        {
            _eraserBatchedRemovals[inkCanvas] = new StrokeCollection();
            _eraserBatchSnapshots[inkCanvas] = inkCanvas.Strokes.ToList();
        }
    }

    private void InkCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not InkCanvas inkCanvas || inkCanvas.DataContext is not PageModel page)
            return;

        if (_viewModel.Toolbar.ActiveTool == ToolbarViewModel.ToolMode.Eraser &&
            _eraserBatchedRemovals.TryGetValue(inkCanvas, out var removedStrokes) &&
            removedStrokes.Count > 0)
        {
            var previousSnapshot = _eraserBatchSnapshots.TryGetValue(inkCanvas, out var snapshot)
                ? snapshot
                : inkCanvas.Strokes.ToList();

            _viewModel.RegisterStrokeChange(page.PageIndex, new StrokeCollection(), removedStrokes, previousSnapshot);
            _strokeSnapshots[page.Strokes] = inkCanvas.Strokes.ToList();
        }

        _eraserBatchedRemovals.Remove(inkCanvas);
        _eraserBatchSnapshots.Remove(inkCanvas);
    }

    private void TrackStrokeCollection(PageModel page)
    {
        if (_trackedStrokePages.ContainsKey(page.Strokes))
            return;

        _trackedStrokePages[page.Strokes] = page.PageIndex;
        _strokeSnapshots[page.Strokes] = page.Strokes.ToList();
        page.Strokes.StrokesChanged += PageStrokes_StrokesChanged;
    }

    private void UntrackStrokeCollection(StrokeCollection strokes)
    {
        if (!_trackedStrokePages.Remove(strokes))
            return;

        strokes.StrokesChanged -= PageStrokes_StrokesChanged;
        _strokeSnapshots.Remove(strokes);
    }

    private void ResetTrackedStrokeCollections()
    {
        foreach (var strokes in _trackedStrokePages.Keys.ToList())
            UntrackStrokeCollection(strokes);
    }

    private void PageStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
    {
        if (sender is not StrokeCollection strokes || !_trackedStrokePages.TryGetValue(strokes, out var pageIndex))
            return;

        // Skip individual eraser events - they will be batched on mouse up
        if (_viewModel.Toolbar.ActiveTool == ToolbarViewModel.ToolMode.Eraser && e.Removed.Count > 0)
        {
            // Find the InkCanvas for this stroke collection and accumulate removals
            foreach (var (inkCanvas, removals) in _eraserBatchedRemovals)
            {
                if (inkCanvas.Strokes == strokes)
                {
                    foreach (var stroke in e.Removed)
                        removals.Add(stroke);
                    return;
                }
            }
        }

        var previousStrokes = _strokeSnapshots.TryGetValue(strokes, out var snapshot)
            ? snapshot
            : strokes.ToList();

        _viewModel.RegisterStrokeChange(pageIndex, e.Added, e.Removed, previousStrokes);
        _strokeSnapshots[strokes] = strokes.ToList();
    }

    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _viewModel.ViewportWidth = (int)e.NewSize.Width;
    }

    private void PagesControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_pdfScrollViewer != null)
            _pdfScrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;

        _pdfScrollViewer = FindVisualChildren<ScrollViewer>(PagesControl).FirstOrDefault();
        if (_pdfScrollViewer != null)
        {
            _pdfScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            _viewModel.UpdateVisiblePages(_pdfScrollViewer.VerticalOffset, _pdfScrollViewer.ViewportHeight);
        }
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_pdfScrollViewer == null)
            return;

        _viewModel.UpdateVisiblePages(_pdfScrollViewer.VerticalOffset, _pdfScrollViewer.ViewportHeight);
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            var pdf = files.FirstOrDefault(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
            if (pdf != null)
                _viewModel.LoadPdf(pdf);
        }
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void ColorSwatch_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement el && el.Tag is Color color)
        {
            _viewModel.Toolbar.SelectedColor = color;
            _viewModel.Toolbar.ActiveTool = ToolbarViewModel.ToolMode.Pen;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void RecentFile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string filePath)
        {
            if (System.IO.File.Exists(filePath))
                _viewModel.LoadPdf(filePath);
            else
                System.Windows.MessageBox.Show("文件不存在或已被移动", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Settings.Save();
        System.Windows.MessageBox.Show("设置已保存", "成功", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void ClearRecent_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show("确定要清空所有最近访问记录吗？", "确认清空",
            System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            _viewModel.Settings.RecentFiles.Clear();
            _viewModel.Settings.Save();
            _viewModel.LoadRecentFiles();
            System.Windows.MessageBox.Show("最近访问记录已清空", "成功", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Auto-save annotations if enabled
        if (_viewModel.Settings.AutoSaveAnnotations && _viewModel.CurrentPdfPath != null)
        {
            _viewModel.SaveCommand.Execute(null);
        }
        _viewModel.Dispose();
        ResetTrackedStrokeCollections();
        base.OnClosed(e);
    }
}
