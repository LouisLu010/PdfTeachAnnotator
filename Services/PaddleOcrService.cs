namespace PdfTeachAnnotator.Services;

public class PaddleOcrService : IOcrService
{
    public Task<string> RecognizePdfAsync(string pdfPath, IProgress<(int Current, int Total)>? progress = null)
    {
        throw new NotSupportedException("PaddleOCR 引擎尚未安装模型文件。该引擎性能要求较高，请先配置 PP-OCR 模型后再启用。");
    }

    public void Dispose()
    {
    }
}
