using System.Text.Json.Serialization;

namespace PdfTeachAnnotator.Models;

public class AnnotationFile
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("pdf_hash")]
    public string PdfHash { get; set; } = string.Empty;

    [JsonPropertyName("pages")]
    public Dictionary<int, string> Pages { get; set; } = new();
}
