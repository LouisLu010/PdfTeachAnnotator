# Research Summary: PDF Teaching Annotation Tool

## Tech Stack Recommendation

| Layer | Choice | Rationale |
|-------|--------|-----------|
| Framework | WPF (.NET 8) | Mature InkCanvas, good DPI support, single-file publish |
| PDF Rendering | PDFiumSharp / PdfiumViewer | Free (BSD), fast, embeddable native DLL |
| Annotation | WPF InkCanvas | Built-in stroke collection, serialization (ISF), pressure support |
| Publish | .NET 8 self-contained single-file | No runtime dependency, Win10+ target |

## Architecture

- **MVVM pattern** with minimal ViewModel layer (tool is focused, not complex)
- **PDF rendering pipeline**: PDFium renders pages to bitmaps → displayed in virtualized ScrollViewer
- **Annotation layer**: InkCanvas overlay per visible page, positioned absolutely over PDF content
- **Coordinate system**: All annotations stored in page-relative coordinates (0-1 normalized), scaled on render
- **Virtualization**: Only render visible pages + 1 buffer page above/below

## Key Features Implementation

| Feature | Approach |
|---------|----------|
| Continuous scroll | VirtualizingStackPanel or custom ItemsControl with page bitmaps |
| Fit-to-width | Scale factor = viewport width / page width, apply to both PDF and InkCanvas |
| Pen color | InkCanvas.DefaultDrawingAttributes.Color, toolbar with preset colors |
| Eraser size | StylusShape with configurable width/height |
| Clear all | InkCanvas.Strokes.Clear() per page |
| Zoom | ScaleTransform on content, recalculate annotation positions |
| Save/Restore | Serialize StrokeCollection per page to JSON + ISF binary |

## Annotation File Format

```json
{
  "version": 1,
  "pdf_hash": "sha256...",
  "pages": {
    "0": { "strokes_isf": "base64..." },
    "3": { "strokes_isf": "base64..." }
  }
}
```

- ISF (Ink Serialization Format) is WPF native, compact, preserves all stroke properties
- Only pages with annotations are stored
- PDF hash for integrity check (warn if PDF changed)

## Pitfalls & Mitigations

| Pitfall | Mitigation |
|---------|-----------|
| PDFium native DLL not bundling | Include as embedded resource or use NativeLibrary.Load |
| High-DPI blurry rendering | Use per-monitor DPI awareness manifest |
| Memory with large PDFs | Virtualize pages, dispose off-screen bitmaps |
| Annotation drift on zoom | Store in normalized coordinates, transform on render |
| Antivirus false positive | Sign exe or document for users |
| InkCanvas performance with 1000+ strokes | Batch render old strokes to bitmap, keep recent editable |

## Deployment

- `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true`
- Include app.manifest for DPI awareness and Windows 10 compatibility
- Target size estimate: ~30-50MB (includes .NET runtime + PDFium)
