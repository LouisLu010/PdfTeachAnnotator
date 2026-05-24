using System.IO;
using System.Text.Json;

namespace PdfTeachAnnotator.Models;

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
    public string Theme { get; set; } = "Light";

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
