using System.IO;
using System.Text.Json;

namespace PdfTeachAnnotator.Models;

public static class OcrEngineNames
{
    public const string Tesseract = "Tesseract";
    public const string PaddleOcr = "PaddleOCR";
}

public class RecentFile
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime LastAccessed { get; set; }
    public int PageCount { get; set; }
    public bool HasAnnotations { get; set; }
}

public class AppSettings
{
    public List<RecentFile> RecentFiles { get; set; } = new();
    public int MaxRecentFiles { get; set; } = 10;
    public bool AutoSaveAnnotations { get; set; } = true;
    public bool ShowWelcomeScreen { get; set; } = true;
    public string Theme { get; set; } = "Dark";
    public double DefaultPenSize { get; set; } = 3.0;
    public double DefaultEraserSize { get; set; } = 20.0;
    public string DefaultPenColor { get; set; } = "#E74C3C"; // Red
    public bool EnableSmoothDrawing { get; set; } = true;
    public bool ShowToolTips { get; set; } = true;
    public double DefaultZoomLevel { get; set; } = 1.0;
    public bool IsSidebarCollapsed { get; set; } = false;
    public string OcrEngine { get; set; } = OcrEngineNames.Tesseract;
    public int OcrRenderWidth { get; set; } = 2000;
    public string PaddleOcrDetectionModelDirectory { get; set; } = string.Empty;
    public string PaddleOcrClassificationModelDirectory { get; set; } = string.Empty;
    public string PaddleOcrRecognitionModelDirectory { get; set; } = string.Empty;
    public string PaddleOcrLabelFile { get; set; } = string.Empty;
    public bool PaddleOcrEnableMkldnn { get; set; } = true;
    public int PaddleOcrCpuThreads { get; set; } = 4;

    private static readonly string SettingsPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "PdfTeachAnnotator", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }

    public void AddRecentFile(string filePath, int pageCount, bool hasAnnotations)
    {
        var existing = RecentFiles.FirstOrDefault(f => f.FilePath == filePath);
        if (existing != null)
            RecentFiles.Remove(existing);

        RecentFiles.Insert(0, new RecentFile
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            LastAccessed = DateTime.Now,
            PageCount = pageCount,
            HasAnnotations = hasAnnotations
        });

        if (RecentFiles.Count > MaxRecentFiles)
            RecentFiles.RemoveRange(MaxRecentFiles, RecentFiles.Count - MaxRecentFiles);

        Save();
    }
}
