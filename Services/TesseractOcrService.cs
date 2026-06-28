using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using Tesseract;

namespace PdfTeachAnnotator.Services;

public class TesseractOcrService : IOcrService
{
    private TesseractEngine? _engine;
    private readonly string _tessDataPath;
    private readonly int _renderWidth;

    public TesseractOcrService(int renderWidth = 2000)
    {
        // tessdata 目录位于应用程序根目录
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _tessDataPath = Path.Combine(appDirectory, "tessdata");
        _renderWidth = Math.Clamp(renderWidth, 1200, 3200);
    }

    public void InitializeEngine()
    {
        if (_engine != null)
            return;

        if (!Directory.Exists(_tessDataPath))
            throw new DirectoryNotFoundException($"Tessdata directory not found: {_tessDataPath}");

        // 初始化 Tesseract 引擎，支持中文和英文
        _engine = new TesseractEngine(_tessDataPath, "chi_sim+eng", EngineMode.Default);
    }

    public async Task<string> RecognizePdfAsync(string pdfPath, IProgress<(int Current, int Total)>? progress = null)
    {
        InitializeEngine();

        var result = new StringBuilder();
        using var pdfService = new PdfRenderService();

        await Task.Run(() =>
        {
            pdfService.OpenDocument(pdfPath);
            int totalPages = pdfService.PageCount;

            for (int i = 0; i < totalPages; i++)
            {
                // 报告进度
                progress?.Report((i + 1, totalPages));

                // 渲染页面为图片（使用较高分辨率以提高 OCR 准确度）
                var bitmap = pdfService.RenderPage(i, _renderWidth);

                // 执行 OCR 识别
                var pageText = RecognizeImage(bitmap);

                // 添加页码分隔
                result.AppendLine($"========== 第 {i + 1} 页 / 共 {totalPages} 页 ==========");
                result.AppendLine(pageText);
                result.AppendLine();
            }

            pdfService.CloseDocument();
        });

        return result.ToString();
    }

    private string RecognizeImage(BitmapSource bitmapSource)
    {
        if (_engine == null)
            throw new InvalidOperationException("OCR engine not initialized.");

        // 保存 BitmapSource 到临时文件
        string tempFile = Path.Combine(Path.GetTempPath(), $"ocr_temp_{Guid.NewGuid()}.png");
        try
        {
            SaveBitmapSourceToFile(bitmapSource, tempFile);

            // 从文件加载 Pix
            using var pix = Pix.LoadFromFile(tempFile);
            using var page = _engine.Process(pix);

            return page.GetText();
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private void SaveBitmapSourceToFile(BitmapSource bitmapSource, string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
        encoder.Save(fileStream);
    }

    public void Dispose()
    {
        _engine?.Dispose();
        _engine = null;
    }
}
