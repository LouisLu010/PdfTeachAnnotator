using System.Text;
using IronOcr;

namespace PdfTeachAnnotator.Services;

public class IronOcrService : IDisposable
{
    private IronTesseract? _engine;

    public IronOcrService()
    {
        InitializeEngine();
    }

    private void InitializeEngine()
    {
        if (_engine != null)
            return;

        _engine = new IronTesseract();

        // 设置支持中文简体 + 英文
        _engine.Language = OcrLanguage.ChineseSimplified;
        _engine.AddSecondaryLanguage(OcrLanguage.English);

        // 关闭不需要的功能以提升速度
        _engine.Configuration.ReadBarCodes = false;
    }

    public async Task<string> RecognizePdfAsync(string pdfPath, IProgress<(int Current, int Total)>? progress = null)
    {
        if (_engine == null)
            throw new InvalidOperationException("OCR engine not initialized.");

        var result = new StringBuilder();

        await Task.Run(() =>
        {
            // 先用完整 PDF 加载获取总页数
            using var fullInput = new OcrInput();
            fullInput.LoadPdf(pdfPath);
            int totalPages = fullInput.PageCount();

            for (int i = 0; i < totalPages; i++)
            {
                progress?.Report((i + 1, totalPages));

                // 逐页识别
                using var pageInput = new OcrInput();
                pageInput.LoadPdfPage(pdfPath, i);

                var pageResult = _engine.Read(pageInput);

                result.AppendLine($"========== 第 {i + 1} 页 / 共 {totalPages} 页 ==========");
                result.AppendLine(pageResult.Text);
                result.AppendLine();
            }
        });

        return result.ToString();
    }

    ~IronOcrService()
    {
        _engine = null;
    }

    void IDisposable.Dispose()
    {
        _engine = null;
    }
}
