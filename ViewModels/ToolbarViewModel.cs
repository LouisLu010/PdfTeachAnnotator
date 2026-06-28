using System.Windows.Ink;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace PdfTeachAnnotator.ViewModels;

public class ToolbarViewModel : ViewModelBase
{
    private Color _selectedColor = Colors.Red;
    private double _eraserSize = 20;
    private ToolMode _activeTool = ToolMode.Pen;
    private double _penSize = 4;
    private DrawingAttributes _drawingAttributes = null!;
    private bool _showToolPopup;

    public enum ToolMode { Pen, Eraser, Highlighter, Laser }

    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (SetField(ref _selectedColor, value))
                UpdateDrawingAttributes();
        }
    }

    public double PenSize
    {
        get => _penSize;
        set
        {
            if (SetField(ref _penSize, Math.Clamp(value, 1, 20)))
                UpdateDrawingAttributes();
        }
    }

    public double EraserSize
    {
        get => _eraserSize;
        set => SetField(ref _eraserSize, Math.Clamp(value, 5, 50));
    }

    public ToolMode ActiveTool
    {
        get => _activeTool;
        set
        {
            if (SetField(ref _activeTool, value))
            {
                NotifyActiveToolFlags();
                UpdateDrawingAttributes();
            }
        }
    }

    public bool IsPenActive => ActiveTool == ToolMode.Pen;
    public bool IsEraserActive => ActiveTool == ToolMode.Eraser;
    public bool IsHighlighterActive => ActiveTool == ToolMode.Highlighter;
    public bool IsLaserActive => ActiveTool == ToolMode.Laser;

    public bool ShowToolPopup
    {
        get => _showToolPopup;
        set => SetField(ref _showToolPopup, value);
    }

    public DrawingAttributes DrawingAttributes
    {
        get => _drawingAttributes;
        private set => SetField(ref _drawingAttributes, value);
    }

    // 12 色预设（对应 App.jsx PEN_COLORS）
    public Color[] PenColors { get; } =
    [
        Color.FromRgb(0x1A, 0x1A, 0x2E), // 深黑
        Color.FromRgb(0xE7, 0x4C, 0x3C), // 红色
        Color.FromRgb(0xF3, 0x9C, 0x12), // 橙色
        Color.FromRgb(0xF1, 0xC4, 0x0F), // 黄色
        Color.FromRgb(0x2E, 0xCC, 0x71), // 绿色
        Color.FromRgb(0x1A, 0xBC, 0x9C), // 青色
        Color.FromRgb(0x34, 0x98, 0xDB), // 蓝色
        Color.FromRgb(0x29, 0x80, 0xB9), // 深蓝
        Color.FromRgb(0x9B, 0x59, 0xB6), // 紫色
        Color.FromRgb(0xE8, 0x43, 0x93), // 粉红
        Color.FromRgb(0xE6, 0x7E, 0x22), // 深橙
        Color.FromRgb(0x63, 0x6E, 0x72), // 灰色
    ];

    public double[] PenSizes { get; } = [2, 4, 6, 9, 12];           // 钢笔/激光 5档
    public double[] HighlighterSizes { get; } = [4, 8, 12, 18, 24]; // 荧光笔 5档
    public double[] EraserSizes { get; } = [10, 20, 30, 40, 50];

    public RelayCommand SetPenSizeCommand { get; }
    public RelayCommand SetEraserSizeCommand { get; }
    public RelayCommand SetPenColorCommand { get; }
    public RelayCommand SelectPenCommand { get; }
    public RelayCommand SelectEraserCommand { get; }
    public RelayCommand SelectHighlighterCommand { get; }
    public RelayCommand SelectLaserCommand { get; }
    public RelayCommand ToggleToolPopupCommand { get; }
    public RelayCommand ClearAllCommand { get; }
    public RelayCommand ToggleClearSliderCommand { get; }

    private bool _showClearSlider;
    private double _clearSliderValue;
    private bool _showCheckmark;

    public bool ShowClearSlider
    {
        get => _showClearSlider;
        set => SetField(ref _showClearSlider, value);
    }

    public double ClearSliderValue
    {
        get => _clearSliderValue;
        set
        {
            if (SetField(ref _clearSliderValue, value))
            {
                if (value >= 100)
                {
                    ShowCheckmarkAndClear();
                }
            }
        }
    }

    public bool ShowCheckmark
    {
        get => _showCheckmark;
        set => SetField(ref _showCheckmark, value);
    }

    public event Action? ClearAllRequested;
    public Func<bool>? ConfirmClearAll { get; set; }

    public ToolbarViewModel()
    {
        SetPenSizeCommand = new RelayCommand(param =>
        {
            if (param is double size)
                PenSize = size;
        });
        SetEraserSizeCommand = new RelayCommand(param =>
        {
            if (param is double size)
                EraserSize = size;
        });
        SetPenColorCommand = new RelayCommand(param =>
        {
            if (param is Color color)
                SelectedColor = color;
        });
        SelectPenCommand = new RelayCommand(() => SelectTool(ToolMode.Pen));
        SelectEraserCommand = new RelayCommand(() => SelectTool(ToolMode.Eraser));
        SelectHighlighterCommand = new RelayCommand(() => SelectTool(ToolMode.Highlighter));
        SelectLaserCommand = new RelayCommand(() => SelectTool(ToolMode.Laser));
        ToggleToolPopupCommand = new RelayCommand(() => ShowToolPopup = !ShowToolPopup);
        ClearAllCommand = new RelayCommand(() => ShowClearSlider = !ShowClearSlider);
        ToggleClearSliderCommand = new RelayCommand(() => ShowClearSlider = !ShowClearSlider);
        UpdateDrawingAttributes();
    }

    private void SelectTool(ToolMode tool)
    {
        if (ActiveTool == tool)
        {
            ShowToolPopup = !ShowToolPopup;
            NotifyActiveToolFlags();
            return;
        }

        ActiveTool = tool;
        ShowToolPopup = false;
    }

    private void NotifyActiveToolFlags()
    {
        OnPropertyChanged(nameof(IsPenActive));
        OnPropertyChanged(nameof(IsEraserActive));
        OnPropertyChanged(nameof(IsHighlighterActive));
        OnPropertyChanged(nameof(IsLaserActive));
    }

    private void ExecuteClearAll()
    {
        ClearAllRequested?.Invoke();
    }

    private async void ShowCheckmarkAndClear()
    {
        ShowCheckmark = true;
        await Task.Delay(1000); // 显示勾号 1 秒
        ShowCheckmark = false;
        ExecuteClearAll();
        ShowClearSlider = false;
        ClearSliderValue = 0;
    }

    private void UpdateDrawingAttributes()
    {
        if (_activeTool == ToolMode.Highlighter)
        {
            // 荧光笔：半透明色层
            var c = _selectedColor;
            DrawingAttributes = new DrawingAttributes
            {
                Color = Color.FromArgb(100, c.R, c.G, c.B), // ~40% 不透明度
                Width = _penSize,
                Height = _penSize,
                FitToCurve = true,
                IsHighlighter = true
            };
        }
        else
        {
            // 钢笔/激光：普通墨迹
            DrawingAttributes = new DrawingAttributes
            {
                Color = _selectedColor,
                Width = _penSize,
                Height = _penSize,
                FitToCurve = true,
                IsHighlighter = false
            };
        }
    }
}
