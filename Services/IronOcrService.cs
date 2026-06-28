using System.Text;
using IronOcr;

namespace PdfTeachAnnotator.Services;

public class IronOcrService : IOcrService
{
    private IronTesseract? _engine;

    private IronTesseract Engine
    {
        get
        {
            if (_engine != null)
                return _engine;

            _engine = new IronTesseract
            {
                Language = OcrLanguage.ChineseSimplified
            };
            _engine.AddSecondaryLanguage(OcrLanguage.English);
            _engine.Configuration.ReadBarCodes = false;
            return _engine;
        }
    }

    public async Task<string> RecognizePdfAsync(string pdfPath, IProgress<(int Current, int Total)>? progress = null)
    {
        var result = new StringBuilder();

        await Task.Run(() =>
        {
            using var fullInput = new OcrInput();
            fullInput.LoadPdf(pdfPath);
            int totalPages = fullInput.PageCount();

            for (int i = 0; i < totalPages; i++)
            {
                progress?.Report((i + 1, totalPages));

                using var pageInput = new OcrInput();
                pageInput.LoadPdfPage(pdfPath, i);
                var pageResult = Engine.Read(pageInput);

                result.AppendLine($"========== 第 {i + 1} 页 / 共 {totalPages} 页 ==========");
                result.AppendLine(pageResult.Text);
                result.AppendLine();
            }
        });

        return result.ToString();
    }

    public void Dispose()
    {
        _engine = null;
    }
}
