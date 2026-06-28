namespace PdfTeachAnnotator.Services;

public interface IOcrService : IDisposable
{
    Task<string> RecognizePdfAsync(string pdfPath, IProgress<(int Current, int Total)>? progress = null);
}
