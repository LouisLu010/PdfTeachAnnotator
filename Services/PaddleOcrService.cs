using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using PdfTeachAnnotator.Models;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;

namespace PdfTeachAnnotator.Services;

public class PaddleOcrService : IOcrService
{
    private readonly AppSettings _settings;
    private PaddleOcrAll? _engine;

    public PaddleOcrService(AppSettings settings)
    {
        _settings = settings;
    }

    private PaddleOcrAll Engine
    {
        get
        {
            if (_engine != null)
                return _engine;

            ValidateSettings();
            var model = new FullOcrModel(
                DetectionModel.FromDirectory(_settings.PaddleOcrDetectionModelDirectory, ModelVersion.V4),
                ClassificationModel.FromDirectory(_settings.PaddleOcrClassificationModelDirectory, ModelVersion.V4),
                RecognizationModel.FromDirectory(_settings.PaddleOcrRecognitionModelDirectory, _settings.PaddleOcrLabelFile, ModelVersion.V4));

            _engine = new PaddleOcrAll(model, ConfigurePaddle);
            _engine.AllowRotateDetection = true;
            _engine.Enable180Classification = true;
            return _engine;
        }
    }

    public async Task<string> RecognizePdfAsync(string pdfPath, IProgress<(int Current, int Total)>? progress = null)
    {
        ValidateSettings();

        var result = new StringBuilder();
        using var pdfService = new PdfRenderService();

        await Task.Run(() =>
        {
            pdfService.OpenDocument(pdfPath);
            int totalPages = pdfService.PageCount;

            for (int i = 0; i < totalPages; i++)
            {
                progress?.Report((i + 1, totalPages));

                var bitmap = pdfService.RenderPage(i, Math.Clamp(_settings.OcrRenderWidth, 1200, 3200));
                var pageText = RecognizeImage(bitmap);

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
        string tempFile = Path.Combine(Path.GetTempPath(), $"paddle_ocr_{Guid.NewGuid()}.png");
        try
        {
            SaveBitmapSourceToFile(bitmapSource, tempFile);
            using var mat = Cv2.ImRead(tempFile, ImreadModes.Color);
            var ocrResult = Engine.Run(mat);
            return ocrResult.Text;
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private void ConfigurePaddle(PaddleConfig config)
    {
        config.MkldnnEnabled = _settings.PaddleOcrEnableMkldnn;
        config.CpuMathThreadCount = Math.Clamp(_settings.PaddleOcrCpuThreads, 1, Environment.ProcessorCount);
        config.MemoryOptimized = true;
        config.GLogEnabled = false;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.PaddleOcrDetectionModelDirectory) || !Directory.Exists(_settings.PaddleOcrDetectionModelDirectory))
            throw new DirectoryNotFoundException("PaddleOCR 检测模型目录不存在。请在设置中配置 det 模型目录。");

        if (string.IsNullOrWhiteSpace(_settings.PaddleOcrClassificationModelDirectory) || !Directory.Exists(_settings.PaddleOcrClassificationModelDirectory))
            throw new DirectoryNotFoundException("PaddleOCR 方向分类模型目录不存在。请在设置中配置 cls 模型目录。");

        if (string.IsNullOrWhiteSpace(_settings.PaddleOcrRecognitionModelDirectory) || !Directory.Exists(_settings.PaddleOcrRecognitionModelDirectory))
            throw new DirectoryNotFoundException("PaddleOCR 识别模型目录不存在。请在设置中配置 rec 模型目录。");

        if (string.IsNullOrWhiteSpace(_settings.PaddleOcrLabelFile) || !File.Exists(_settings.PaddleOcrLabelFile))
            throw new FileNotFoundException("PaddleOCR 字典文件不存在。请在设置中配置 ppocr_keys_v*.txt 字典文件。", _settings.PaddleOcrLabelFile);
    }

    private static void SaveBitmapSourceToFile(BitmapSource bitmapSource, string filePath)
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
