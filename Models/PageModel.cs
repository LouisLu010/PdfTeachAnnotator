using System.Windows.Ink;
using System.Windows.Media.Imaging;
using PdfTeachAnnotator.ViewModels;

namespace PdfTeachAnnotator.Models;

public class PageModel : ViewModelBase
{
    private BitmapSource _image = null!;

    public int PageIndex { get; init; }
    public StrokeCollection Strokes { get; } = new();

    public BitmapSource Image
    {
        get => _image;
        set => SetField(ref _image, value);
    }
}
