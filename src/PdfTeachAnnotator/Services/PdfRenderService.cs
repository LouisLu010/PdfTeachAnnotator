using System.Windows.Media.Imaging;
using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;

namespace PdfTeachAnnotator.Services;

public class PdfRenderService : IDisposable
{
    private IDocLib? _library;
    private IDocReader? _reader;
    private string? _currentPath;

    public int PageCount => _reader?.GetPageCount() ?? 0;

    public void OpenDocument(string path)
    {
        CloseDocument();
        _library = DocLib.Instance;
        _reader = _library.GetDocReader(path, new PageDimensions(1));
        _currentPath = path;
    }

    public BitmapSource RenderPage(int pageIndex, int targetWidth)
    {
        if (_reader == null)
            throw new InvalidOperationException("No document loaded.");

        using var pageReader = _reader.GetPageReader(pageIndex);
        var originalWidth = pageReader.GetPageWidth();
        var originalHeight = pageReader.GetPageHeight();

        double scale = (double)targetWidth / originalWidth;
        int renderWidth = targetWidth;
        int renderHeight = (int)(originalHeight * scale);

        _reader.Dispose();
        _reader = _library!.GetDocReader(_currentPath!, new PageDimensions(renderWidth, renderHeight));

        using var scaledPageReader = _reader.GetPageReader(pageIndex);
        var rawBytes = scaledPageReader.GetImage();
        var width = scaledPageReader.GetPageWidth();
        var height = scaledPageReader.GetPageHeight();

        return ConvertToBitmapSource(rawBytes, width, height);
    }

    public List<BitmapSource> RenderAllPages(int targetWidth)
    {
        if (_reader == null)
            throw new InvalidOperationException("No document loaded.");

        var pages = new List<BitmapSource>();
        int count = _reader.GetPageCount();

        for (int i = 0; i < count; i++)
        {
            using var pageReader = _reader.GetPageReader(i);
            var originalWidth = pageReader.GetPageWidth();
            var originalHeight = pageReader.GetPageHeight();

            double scale = (double)targetWidth / originalWidth;
            int renderHeight = (int)(originalHeight * scale);

            pages.Add(RenderPageAtSize(i, targetWidth, renderHeight));
        }

        return pages;
    }

    private BitmapSource RenderPageAtSize(int pageIndex, int width, int height)
    {
        using var reader = _library!.GetDocReader(_currentPath!, new PageDimensions(width, height));
        using var pageReader = reader.GetPageReader(pageIndex);
        var rawBytes = pageReader.GetImage();
        var w = pageReader.GetPageWidth();
        var h = pageReader.GetPageHeight();
        return ConvertToBitmapSource(rawBytes, w, h);
    }

    private static BitmapSource ConvertToBitmapSource(byte[] rawBytes, int width, int height)
    {
        var bitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
        bitmap.Lock();
        System.Runtime.InteropServices.Marshal.Copy(rawBytes, 0, bitmap.BackBuffer, rawBytes.Length);
        bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
        bitmap.Unlock();
        bitmap.Freeze();
        return bitmap;
    }

    public void CloseDocument()
    {
        _reader?.Dispose();
        _reader = null;
        _currentPath = null;
    }

    public void Dispose()
    {
        CloseDocument();
    }
}
