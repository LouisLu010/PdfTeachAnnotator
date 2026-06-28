using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using PdfTeachAnnotator.Models;
using PdfTeachAnnotator.ViewModels;
using Color = System.Windows.Media.Color;

namespace PdfTeachAnnotator;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly Dictionary<StrokeCollection, int> _trackedStrokePages = new();
    private readonly Dictionary<StrokeCollection, List<Stroke>> _strokeSnapshots = new();
    private ScrollViewer? _pdfScrollViewer;
    private readonly Dictionary<InkCanvas, StrokeCollection> _eraserBatchedRemovals = new();
    private readonly Dictionary<InkCanvas, List<Stroke>> _eraserBatchSnapshots = new();
    private readonly List<(DispatcherTimer Timer, InkCanvas Canvas, Stroke Stroke)> _laserTimers = new();

    public MainViewModel ViewModel => _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        ToolPopup.DataContext = _viewModel;
        ApplyTheme();

        _viewModel.Toolbar.ConfirmClearAll = () =>
            System.Windows.MessageBox.Show("确定要清除所有批注吗？", "确认清除",
                System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.OK;

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsDarkMode))
                ApplyTheme();
            else if (e.PropertyName == nameof(MainViewModel.ZoomLevel))
                UpdateEraserShapes();
            else if (e.PropertyName == nameof(MainViewModel.IsSidebarCollapsed))
                AnimateSidebar();
        };

        _viewModel.Toolbar.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(ToolbarViewModel.EraserSize) or nameof(ToolbarViewModel.ActiveTool))
                UpdateEraserShapes();

            // 根据当前工具设置弹窗定位目标
            if (e.PropertyName == nameof(ToolbarViewModel.ShowToolPopup) && _viewModel.Toolbar.ShowToolPopup)
            {
                ToolPopup.PlacementTarget = _viewModel.Toolbar.ActiveTool switch
                {
                    ToolbarViewModel.ToolMode.Pen => PenBtn,
                    ToolbarViewModel.ToolMode.Highlighter => HighlighterBtn,
                    ToolbarViewModel.ToolMode.Laser => LaserBtn,
                    ToolbarViewModel.ToolMode.Eraser => EraserBtn,
                    _ => PenBtn
                };
            }
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

    private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_viewModel.Toolbar.ShowToolPopup || e.OriginalSource is not DependencyObject source)
            return;

        if (IsWithin(source, PenBtn) ||
            IsWithin(source, HighlighterBtn) ||
            IsWithin(source, LaserBtn) ||
            IsWithin(source, EraserBtn) ||
            IsWithin(source, ToolPopup.Child as DependencyObject))
            return;

        _viewModel.Toolbar.ShowToolPopup = false;
    }

    private static bool IsWithin(DependencyObject source, DependencyObject? target)
    {
        if (target == null)
            return false;

        for (DependencyObject? current = source; current != null; current = GetParent(current))
        {
            if (ReferenceEquals(current, target))
                return true;
        }

        return false;
    }

    private static DependencyObject? GetParent(DependencyObject current)
    {
        if (current is Visual or System.Windows.Media.Media3D.Visual3D)
            return VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);

        return LogicalTreeHelper.GetParent(current);
    }

    private void UpdateEraserShapes()
    {
        foreach (var inkCanvas in FindVisualChildren<InkCanvas>(this))
            UpdateEraserShape(inkCanvas);
    }

    private void AnimateSidebar()
    {
        var targetWidth = _viewModel.IsSidebarCollapsed ? 68.0 : 220.0;
        var animation = new DoubleAnimation(targetWidth, new Duration(TimeSpan.FromMilliseconds(300)))
        {
            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
        };
        SidebarBorder.BeginAnimation(WidthProperty, animation);
    }

    private void UpdateEraserShape(InkCanvas inkCanvas)
    {
        inkCanvas.EraserShape = CreateEraserShape();
        RefreshEraserCursor(inkCanvas);
    }

    private void RefreshEraserCursor(InkCanvas inkCanvas)
    {
        if (_viewModel.Toolbar.ActiveTool != ToolbarViewModel.ToolMode.Eraser)
            return;

        inkCanvas.SetCurrentValue(InkCanvas.EditingModeProperty, InkCanvasEditingMode.None);
        inkCanvas.Dispatcher.BeginInvoke(() =>
        {
            if (_viewModel.Toolbar.ActiveTool == ToolbarViewModel.ToolMode.Eraser)
                inkCanvas.SetCurrentValue(InkCanvas.EditingModeProperty, InkCanvasEditingMode.EraseByPoint);
        }, DispatcherPriority.Render);
    }

    private RectangleStylusShape CreateEraserShape()
    {
        var zoom = Math.Max(_viewModel.ZoomLevel, 0.01);
        var size = _viewModel.Toolbar.EraserSize / zoom;
        return new RectangleStylusShape(size, size);
    }

    private void ApplyTheme()
    {
        if (_viewModel.IsDarkMode)
        {
            SetBrush("SurfaceBaseBrush", "DarkSurfaceBaseColor");
            SetBrush("SurfaceRaisedBrush", "DarkSurfaceRaisedColor");
            SetBrush("SurfaceOverlayBrush", "DarkSurfaceOverlayColor", 0.9);
            SetBrush("SidebarBgBrush", "DarkSidebarBgColor");
            SetBrush("SidebarHoverBrush", "DarkSidebarHoverColor");
            SetBrush("SidebarActiveBrush", "DarkSidebarActiveColor");
            SetBrush("TextPrimaryBrush", "DarkTextPrimaryColor");
            SetBrush("TextSecondaryBrush", "DarkTextSecondaryColor");
            SetBrush("TextMutedBrush", "DarkTextMutedColor");
            SetBrush("BorderBrush", "DarkBorderColor");
            SetBrush("BorderSubtleBrush", "DarkBorderColor");
        }
        else
        {
            SetBrush("SurfaceBaseBrush", Color.FromRgb(0xF0, 0xF2, 0xF5));
            SetBrush("SurfaceRaisedBrush", Colors.White);
            SetBrush("SurfaceOverlayBrush", Colors.White, 0.85);
            SetBrush("SidebarBgBrush", Color.FromRgb(0xF3, 0xF3, 0xF5));
            SetBrush("SidebarHoverBrush", Colors.Black, 0.05);
            SetBrush("SidebarActiveBrush", Colors.Black, 0.08);
            SetBrush("TextPrimaryBrush", Color.FromRgb(0x1A, 0x1A, 0x2E));
            SetBrush("TextSecondaryBrush", Color.FromRgb(0x5A, 0x5A, 0x72));
            SetBrush("TextMutedBrush", Color.FromRgb(0x8A, 0x8A, 0x9E));
            SetBrush("BorderBrush", Color.FromRgb(0xD1, 0xD1, 0xD6));
            SetBrush("BorderSubtleBrush", Colors.Black, 0.08);
        }
    }

    private void SetBrush(string brushKey, string colorKey, double opacity = 1)
    {
        if (TryFindResource(colorKey) is Color color)
            SetBrush(brushKey, color, opacity);
    }

    private void SetBrush(string key, Color color, double opacity = 1)
    {
        Resources[key] = new SolidColorBrush(color) { Opacity = opacity };
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
        UpdateEraserShape(inkCanvas);
        inkCanvas.PreviewMouseDown += InkCanvas_PreviewMouseDown;
        inkCanvas.PreviewMouseUp += InkCanvas_PreviewMouseUp;
    }

    private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
    {
        CommandManager.InvalidateRequerySuggested();

        // 激光笔：收集笔画后 3 秒自动淡化移除
        if (sender is InkCanvas inkCanvas &&
            _viewModel.Toolbar.ActiveTool == ToolbarViewModel.ToolMode.Laser)
        {
            var stroke = e.Stroke;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                _laserTimers.RemoveAll(t => t.Stroke == stroke);
                if (inkCanvas.Strokes.Contains(stroke))
                    inkCanvas.Strokes.Remove(stroke);
            };
            timer.Start();
            _laserTimers.Add((timer, inkCanvas, stroke));
        }
    }

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
