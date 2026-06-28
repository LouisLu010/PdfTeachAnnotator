using PdfTeachAnnotator.Models;

namespace PdfTeachAnnotator.Services;

public static class OcrServiceFactory
{
    public static IOcrService Create(AppSettings settings) => settings.OcrEngine switch
    {
        OcrEngineNames.PaddleOcr => new PaddleOcrService(settings),
        _ => new TesseractOcrService(settings.OcrRenderWidth)
    };
}
