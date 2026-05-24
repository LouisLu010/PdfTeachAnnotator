using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using PdfTeachAnnotator.ViewModels;
using Wpf.Ui.Controls;

namespace PdfTeachAnnotator;

public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;

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

    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _viewModel.ViewportWidth = (int)e.NewSize.Width;
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
        base.OnClosed(e);
    }
}
