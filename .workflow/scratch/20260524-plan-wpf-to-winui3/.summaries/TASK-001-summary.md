# TASK-001 执行总结

## 任务目标
创建 WinUI 3 项目骨架并迁移核心代码

## 执行内容

### 1. 项目结构创建
- ✅ 创建 `src/PdfTeachAnnotatorWinUI/` 目录
- ✅ 创建项目文件 `PdfTeachAnnotatorWinUI.csproj`
  - 配置 TargetFramework: `net8.0-windows10.0.19041.0`
  - 添加 Microsoft.WindowsAppSDK 引用
  - 配置单文件发布（PublishSingleFile, SelfContained, IncludeNativeLibrariesForSelfExtract）
  - 保留 Docnet.Core 依赖（暂时）

### 2. 应用入口文件
- ✅ 创建 `App.xaml` 和 `App.xaml.cs`
  - WinUI 3 Application 入口
  - 配置资源字典
- ✅ 创建 `MainWindow.xaml` 和 `MainWindow.xaml.cs`
  - 基础窗口结构（占位符界面）
  - 绑定 MainViewModel

### 3. Models 迁移
- ✅ `Models/PageModel.cs`
  - 将 `BitmapSource` 改为 `WriteableBitmap`
  - 将 `StrokeCollection` 改为 `InkStrokeContainer`
- ✅ `Models/AnnotationFile.cs`
  - 直接复制（无需修改）

### 4. ViewModels 迁移
- ✅ `ViewModels/ViewModelBase.cs`
  - 移除 WPF 的 `CommandManager.RequerySuggested`
  - 添加 `RaiseCanExecuteChanged()` 方法
- ✅ `ViewModels/MainViewModel.cs`
  - 调整为异步方法签名
  - 保留业务逻辑结构
  - 标记 TODO 待后续任务实现
- ✅ `ViewModels/ToolbarViewModel.cs`
  - 将 `System.Windows.Media.Color` 改为 `Windows.UI.Color`
  - 将 `DrawingAttributes` 改为 `InkDrawingAttributes`
  - 调整属性设置方式

### 5. Services 迁移
- ✅ `Services/PdfRenderService.cs`
  - 创建占位符结构
  - 标记 TODO 待 TASK-002 实现
- ✅ `Services/AnnotationFileService.cs`
  - 创建占位符结构
  - 标记 TODO 待 TASK-004 实现

### 6. 其他文件
- ✅ `app.manifest` - DPI 感知配置
- ✅ `GlobalUsings.cs` - 全局 using 声明
- ✅ `Converters/Converters.cs` - 占位符转换器

## 技术调整

### WPF → WinUI 3 类型映射
| WPF | WinUI 3 |
|-----|---------|
| `System.Windows.Media.Color` | `Windows.UI.Color` |
| `System.Windows.Media.Imaging.BitmapSource` | `Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap` |
| `System.Windows.Ink.StrokeCollection` | `Microsoft.UI.Input.Inking.InkStrokeContainer` |
| `System.Windows.Ink.DrawingAttributes` | `Microsoft.UI.Input.Inking.InkDrawingAttributes` |
| `CommandManager.RequerySuggested` | 手动 `RaiseCanExecuteChanged()` |

## 收敛标准验证

等待编译完成后验证：
- [ ] PdfTeachAnnotatorWinUI.csproj 包含 `<TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>`
- [ ] PdfTeachAnnotatorWinUI.csproj 包含 PackageReference 'Microsoft.WindowsAppSDK'
- [ ] PdfTeachAnnotatorWinUI.csproj 包含 `<PublishSingleFile>true</PublishSingleFile>`
- [ ] 存在 Models/PageModel.cs 且命名空间为 PdfTeachAnnotatorWinUI.Models
- [ ] 存在 ViewModels/ViewModelBase.cs
- [ ] 存在 Services/ 目录
- [ ] dotnet build src/PdfTeachAnnotatorWinUI 成功编译

## 下一步
TASK-002: 实现 PDF 渲染和缩放功能（使用 Windows.Data.Pdf API）
