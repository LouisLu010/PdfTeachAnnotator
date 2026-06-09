using System.Windows.Ink;
using System.Windows.Media.Imaging;
using PdfTeachAnnotator.ViewModels;

namespace PdfTeachAnnotator.Models;

public class PageModel : ViewModelBase
{
    private BitmapSource? _image;
    private int _pageWidth;
    private int _pageHeight;

    public int PageIndex { get; init; }
    public StrokeCollection Strokes { get; } = new();

    public BitmapSource? Image
    {
        get => _image;
        set
        {
            if (SetField(ref _image, value))
                OnPropertyChanged(nameof(IsImageLoaded));
        }
    }

    public int PageWidth
    {
        get => _pageWidth;
        set => SetField(ref _pageWidth, value);
    }

    public int PageHeight
    {
        get => _pageHeight;
        set => SetField(ref _pageHeight, value);
    }

    public bool IsImageLoaded => Image != null;
}
