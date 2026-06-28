using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static PdfTeachAnnotator.ViewModels.ToolbarViewModel;

namespace PdfTeachAnnotator.Converters;

public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Color c ? new SolidColorBrush(c) : Brushes.Transparent;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ToolModeToEditingModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is ToolMode mode
            ? mode == ToolMode.Eraser ? InkCanvasEditingMode.EraseByPoint : InkCanvasEditingMode.Ink
            : InkCanvasEditingMode.Ink;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? FontWeights.Bold : FontWeights.Normal;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ColorSelectedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is Color item && values[1] is Color selected)
            return item == selected ? new SolidColorBrush(Color.FromRgb(74, 144, 217)) : new SolidColorBrush(Color.FromRgb(192, 200, 210));
        return new SolidColorBrush(Color.FromRgb(192, 200, 210));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class PenIconColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is bool isPenActive && values[1] is Color selectedColor)
        {
            if (isPenActive)
                return Brushes.White;
            return new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Default blue
        }
        return new SolidColorBrush(Color.FromRgb(52, 152, 219));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class SliderProgressConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is double sliderValue && values[1] is double containerWidth)
        {
            // 计算进度条宽度：滑动值百分比 * 容器宽度
            return (sliderValue / 100.0) * containerWidth;
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class EraserSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double size)
            return size;
        return 16.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class SizeEqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is double currentSize && values[1] is double buttonSize)
        {
            return Math.Abs(currentSize - buttonSize) < 0.01 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}

/// <summary>
/// 比较两个 double 值，相等时返回 AccentBrush，否则返回 BorderSubtleBrush。
/// 用于粗细预设按钮的选中高亮。
/// </summary>
public class SizeSelectedBrushConverter : IMultiValueConverter
{
    private static readonly SolidColorBrush Accent = new(Color.FromRgb(0x4F, 0x6E, 0xF7));
    private static readonly SolidColorBrush Subtle = new(Color.FromArgb(0x14, 0x00, 0x00, 0x00));

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && TryGetDouble(values[0], out var current) && TryGetDouble(values[1], out var button))
            return Math.Abs(current - button) < 0.01 ? Accent : Subtle;
        return Subtle;
    }

    private static bool TryGetDouble(object value, out double result)
    {
        switch (value)
        {
            case double d:
                result = d;
                return true;
            case float f:
                result = f;
                return true;
            case decimal m:
                result = (double)m;
                return true;
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case string s:
                return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ||
                       double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
            default:
                result = 0;
                return false;
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool collapsed = value is bool b && b;
        return collapsed ? new GridLength(68) : new GridLength(220);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
