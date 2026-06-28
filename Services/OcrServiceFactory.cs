using PdfTeachAnnotator.Models;

namespace PdfTeachAnnotator.Services;

public static class OcrServiceFactory
{
    public static IOcrService Create(string engineName) => engineName switch
    {
        OcrEngineNames.IronOcr => new IronOcrService(),
        OcrEngineNames.PaddleOcr => new PaddleOcrService(),
        _ => new TesseractOcrService()
    };
}
