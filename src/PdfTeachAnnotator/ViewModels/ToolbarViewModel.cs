using System.Windows.Ink;
using System.Windows.Media;

namespace PdfTeachAnnotator.ViewModels;

public class ToolbarViewModel : ViewModelBase
{
    private Color _selectedColor = Colors.Red;
    private double _eraserSize = 20;
    private ToolMode _activeTool = ToolMode.Pen;
    private double _penSize = 3;
    private DrawingAttributes _drawingAttributes = null!;

    public enum ToolMode { Pen, Eraser }

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
                OnPropertyChanged(nameof(IsPenActive));
                OnPropertyChanged(nameof(IsEraserActive));
            }
        }
    }

    public bool IsPenActive => ActiveTool == ToolMode.Pen;
    public bool IsEraserActive => ActiveTool == ToolMode.Eraser;

    public DrawingAttributes DrawingAttributes
    {
        get => _drawingAttributes;
        private set => SetField(ref _drawingAttributes, value);
    }

    public Color[] PenColors { get; } =
    [
        Colors.Red, Colors.Blue, Colors.Green, Colors.Black,
        Colors.Orange, Colors.Purple, Colors.Yellow, Colors.White
    ];

    public RelayCommand SelectPenCommand { get; }
    public RelayCommand SelectEraserCommand { get; }
    public RelayCommand ClearAllCommand { get; }

    public event Action? ClearAllRequested;
    public Func<bool>? ConfirmClearAll { get; set; }

    public ToolbarViewModel()
    {
        SelectPenCommand = new RelayCommand(() => ActiveTool = ToolMode.Pen);
        SelectEraserCommand = new RelayCommand(() => ActiveTool = ToolMode.Eraser);
        ClearAllCommand = new RelayCommand(ExecuteClearAll);
        UpdateDrawingAttributes();
    }

    private void ExecuteClearAll()
    {
        if (ConfirmClearAll == null || ConfirmClearAll())
            ClearAllRequested?.Invoke();
    }

    private void UpdateDrawingAttributes()
    {
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
