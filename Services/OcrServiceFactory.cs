namespace PdfTeachAnnotator.Services;

public static class OcrServiceFactory
{
    public static IOcrService Create() => new TesseractOcrService();
}
