using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Ink;
using PdfTeachAnnotator.Models;

namespace PdfTeachAnnotator.Services;

public class AnnotationFileService
{
    private static string GetAnnotationPath(string pdfPath)
        => pdfPath + ".annotations";

    public void SaveAnnotations(string pdfPath, Dictionary<int, StrokeCollection> pageStrokes)
    {
        var file = new AnnotationFile
        {
            Version = 1,
            PdfHash = ComputePdfHash(pdfPath)
        };

        foreach (var (pageIndex, strokes) in pageStrokes)
        {
            if (strokes.Count == 0) continue;
            using var ms = new MemoryStream();
            strokes.Save(ms);
            file.Pages[pageIndex] = Convert.ToBase64String(ms.ToArray());
        }

        var json = JsonSerializer.Serialize(file, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetAnnotationPath(pdfPath), json);
    }

    public Dictionary<int, StrokeCollection>? LoadAnnotations(string pdfPath)
    {
        var path = GetAnnotationPath(pdfPath);
        if (!File.Exists(path)) return null;

        var json = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<AnnotationFile>(json);
        if (file == null) return null;

        var result = new Dictionary<int, StrokeCollection>();
        foreach (var (pageIndex, base64) in file.Pages)
        {
            var bytes = Convert.FromBase64String(base64);
            using var ms = new MemoryStream(bytes);
            result[pageIndex] = new StrokeCollection(ms);
        }

        return result;
    }

    private static string ComputePdfHash(string path)
    {
        using var stream = File.OpenRead(path);
        var buffer = new byte[Math.Min(1024 * 1024, stream.Length)];
        int read = stream.Read(buffer, 0, buffer.Length);
        var hash = SHA256.HashData(buffer.AsSpan(0, read));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
