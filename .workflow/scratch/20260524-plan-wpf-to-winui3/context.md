# WPF to WinUI 3 Conversion - Context

**Created:** 2026-05-24
**Type:** Technology Migration + UI Modernization

## Goal

将现有的 WPF PDF 教学批注工具完整迁移到 WinUI 3 平台，并使用 Fluent Design 设计语言进行界面美化。

## Current State

### 现有技术栈
- **框架**: WPF (.NET 8)
- **PDF 渲染**: Docnet.Core (PDFium)
- **批注**: WPF InkCanvas
- **架构**: MVVM (ViewModels, Services, Models)

### 现有功能
1. PDF 文件打开和连续滚动显示
2. 画笔批注（颜色选择、粗细调节）
3. 橡皮擦除（大小可调）
4. 一键清空所有批注
5. 缩放功能（放大、缩小、重置）
6. 批注保存和恢复（ISF + JSON 格式）
7. 单文件 exe 发布

### 现有文件结构
```
src/PdfTeachAnnotator/
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── Models/
│   ├── PageModel.cs
│   └── AnnotationFile.cs
├── ViewModels/
│   ├── ViewModelBase.cs
│   ├── MainViewModel.cs
│   └── ToolbarViewModel.cs
├── Services/
│   ├── PdfRenderService.cs
│   └── AnnotationFileService.cs
└── Converters/
    └── Converters.cs
```

## Target State

### 新技术栈
- **框架**: WinUI 3 (.NET 8)
- **PDF 渲染**: 保持 Docnet.Core 或迁移到 Windows.Data.Pdf
- **批注**: InkCanvas (WinUI 3 版本) 或 Canvas + PointerPoint
- **UI 设计**: Fluent Design System
  - Acrylic 材质背景
  - 圆角设计
  - 阴影效果
  - 流畅动画
  - 现代化图标

### 设计要求
1. **标题栏**: 自定义标题栏，集成 Acrylic 材质
2. **工具栏**: 使用 CommandBar，圆角按钮，悬停效果
3. **颜色选择器**: 现代化色板设计
4. **PDF 显示区**: 白色卡片 + 阴影，Acrylic 背景
5. **响应式**: 支持窗口大小调整，流畅过渡动画

## Technical Decisions

### Locked Decisions
1. **保留所有现有功能** - 不能丢失任何功能
2. **保持 MVVM 架构** - ViewModels 逻辑尽量复用
3. **保持批注文件格式兼容** - 现有保存的批注文件必须能加载
4. **单文件发布** - 保持 self-contained 单 exe 发布能力

### Implementation Choices
1. **PDF 渲染方案**:
   - 优先尝试 Windows.Data.Pdf (UWP API)
   - 如果不兼容，保持 Docnet.Core
2. **批注实现**:
   - 优先使用 WinUI 3 InkCanvas
   - 如果功能不足，使用 Canvas + PointerPoint 自定义实现
3. **项目结构**:
   - 创建新的 WinUI 3 项目
   - 逐步迁移 ViewModels 和 Services
   - 重写所有 XAML 视图

## Migration Strategy

### Wave 1: 项目骨架
- 创建 WinUI 3 项目
- 配置单文件发布
- 迁移基础 ViewModels 和 Services

### Wave 2: PDF 渲染
- 实现 PDF 渲染服务
- 实现连续滚动显示
- 实现缩放功能

### Wave 3: 批注功能
- 实现画笔工具
- 实现橡皮工具
- 实现清空功能
- 实现颜色和大小选择

### Wave 4: 保存/恢复
- 实现批注保存
- 实现批注加载
- 确保格式兼容

### Wave 5: UI 美化
- 应用 Fluent Design
- Acrylic 材质
- 圆角和阴影
- 动画效果

## Constraints

- **兼容性**: Windows 10 1809+ 或 Windows 11
- **性能**: 不能比 WPF 版本慢
- **文件大小**: 单文件 exe 尽量控制在合理范围
- **开发时间**: 分阶段实现，每个 wave 独立可测试

## References

- WinUI 3 文档: https://learn.microsoft.com/windows/apps/winui/
- Fluent Design: https://fluent2.microsoft.design/
- Windows.Data.Pdf: https://learn.microsoft.com/uwp/api/windows.data.pdf
- WinUI 3 InkCanvas: https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.inkcanvas
