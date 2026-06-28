# PDF 教学批注工具

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?style=flat-square&logo=windows)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

面向课堂讲解、课件批注和 PDF 备课场景的 Windows 桌面应用，支持手写批注、荧光标注、激光笔演示、离线 OCR 和批注自动保存。

[功能特性](#功能特性) • [快速开始](#快速开始) • [使用说明](#使用说明) • [技术栈](#技术栈) • [路线图](#路线图)

</div>

---

## ✨ 功能特性

### 📝 PDF 阅读与批注

- **PDF 渲染与显示**
  - 基于 `Docnet.Core` / PDFium 的本地 PDF 渲染
  - 支持多页 PDF 文档、滚动阅读和动态页码指示
  - 可见页按需加载，降低长文档阅读时的资源占用
  - 支持 50% 到 400% 缩放，提供按钮缩放和 `Ctrl + 鼠标滚轮` 缩放

- **手写批注工具**
  - 钢笔、荧光笔、激光笔和橡皮擦
  - 12 种预设颜色，覆盖课堂常用标注色
  - 钢笔 / 激光笔 5 档大小，荧光笔 5 档大小，橡皮擦 5 档大小
  - 激光笔笔迹会短暂显示后自动消失，适合课堂讲解指示重点
  - 支持撤销和重做，降低误操作成本

- **批注管理**
  - 批注保存为同名旁路文件：`{pdfPath}.annotations`
  - 批注与原始 PDF 分离存储，不修改原始文件
  - 重新打开 PDF 时自动加载已有批注
  - 支持手动保存和关闭时自动保存
  - 支持滑动确认清除全部批注，避免误触

### 🎨 界面体验

- **现代化 UI**
  - WPF / Fluent 风格界面
  - 支持深色模式和浅色模式
  - 侧边栏可折叠，折叠后保留图标导航
  - 侧边栏折叠和页面切换带有过渡动画

- **清晰的信息结构**
  - 主页：打开或拖入 PDF，查看最近文件
  - PDF 编辑视图：阅读、缩放、批注和保存
  - 工具箱：OCR 文字识别
  - 设置：主题、自动保存、最近文件数量、默认画笔和 OCR 引擎信息
  - 关于：项目简介和核心特性

- **课堂友好交互**
  - 大尺寸触控按钮，适合教学一体机和触摸屏
  - 支持拖放打开 PDF
  - 最近文件列表便于快速回到常用课件
  - 暗光教室下可切换深色主题，减少视觉刺激

### 🔍 离线 OCR 文字识别

- 基于 `Tesseract 5.2.0` 的本地 OCR
- 默认支持中文简体和英文：`chi_sim + eng`
- 支持整篇 PDF 识别，识别过程中显示页级进度
- 识别结果可一键复制
- OCR 处理在本地完成，不上传 PDF 或识别结果

> `main` 分支默认只使用 Tesseract。PaddleOCR 实验方案保留在 `feature/paddleocr` 分支，便于后续单独验证和迭代。

---

## 🚀 快速开始

### 系统要求

- **操作系统**：Windows 10/11 x64
- **内存**：建议 4 GB 以上
- **显示器**：建议 1920×1080 或更高分辨率
- **运行时**：发布包为 self-contained win-x64，普通用户通常无需单独安装 .NET Runtime

### 方法 1：下载预编译版本

1. 前往 [Releases](https://github.com/LouisLu010/PdfTeachAnnotator/releases) 页面。
2. 下载最新版本的 `PdfTeachAnnotator-Release.zip`。
3. 解压到任意目录。
4. 双击 `PdfTeachAnnotator.lnk`，或运行启动脚本打开应用。

> OCR 需要发布目录中包含 `tessdata` 文件夹，项目当前包含 `chi_sim.traineddata` 和 `eng.traineddata`。

### 方法 2：从源码编译

```bash
git clone https://github.com/LouisLu010/PdfTeachAnnotator.git
cd PdfTeachAnnotator

dotnet build -c Release
dotnet run --project PdfTeachAnnotator.csproj
```

---

## 📖 使用说明

### 1. 打开 PDF

- 在主页点击“打开 PDF 文件”。
- 将 PDF 文件拖放到应用窗口。
- 从最近文件列表中选择历史课件。
- 使用快捷键 `Ctrl + O` 打开 PDF。

当前应用只支持打开 `.pdf` 文件。

### 2. 选择批注工具

- **钢笔**：用于普通手写批注。
- **荧光笔**：用于半透明重点标记。
- **激光笔**：用于课堂演示指示，笔迹会在短时间后自动消失。
- **橡皮擦**：用于擦除已有批注。

可在工具栏中选择颜色、笔迹大小和橡皮擦大小。

### 3. 保存和恢复批注

- 使用 `Ctrl + S` 保存当前批注。
- 开启自动保存后，关闭文件或退出应用时会保存批注。
- 批注会写入 PDF 同目录下的 `.annotations` 文件。
- 原始 PDF 不会被修改，也不会被重新导出覆盖。

### 4. 使用 OCR

1. 打开一个 PDF。
2. 进入侧边栏的“工具箱”。
3. 在 OCR 卡片中点击“开始识别”。
4. 等待进度完成后复制识别结果。

识别准确率取决于 PDF 页面清晰度、字体质量、扫描分辨率和版面复杂度。

### 5. 快捷键

| 快捷键 | 功能 |
| --- | --- |
| `Ctrl + O` | 打开 PDF 文件 |
| `Ctrl + S` | 保存批注 |
| `Ctrl + Z` | 撤销 |
| `Ctrl + Y` | 重做 |
| `Ctrl + Shift + Z` | 重做 |
| `Ctrl + 鼠标滚轮` | 放大 / 缩小 |

---

## 🎯 项目特色

### 面向教学场景

PdfTeachAnnotator 不是通用 PDF 编辑器，而是围绕课堂讲解和备课批注设计：打开 PDF 后即可书写、标重点、擦除、撤销、保存，尽量减少课堂中的操作干扰。

### 不破坏原始 PDF

批注使用旁路 `.annotations` 文件保存，原始 PDF 保持不变。教师可以安全地在课件上做课堂标记，也可以保留干净的原文件。

### 本地离线处理

PDF 渲染、批注保存和 OCR 都在本地完成。应用不需要上传文件，适合学校内网、离线教室和对隐私有要求的教学环境。

### 适合大屏与触控

界面按钮和工具栏以触控友好为目标，配合深浅色主题、侧边栏折叠和动画反馈，适合教学一体机、大屏投影和 Windows 平板。

---

## 🏗️ 技术栈

### 核心技术

- **应用框架**：`.NET 8` / `net8.0-windows`
- **UI 框架**：WPF
- **架构风格**：MVVM、WPF Data Binding、`ICommand` / `RelayCommand`
- **PDF 渲染**：`Docnet.Core 2.6.0`（PDFium native 渲染）
- **批注系统**：WPF `InkCanvas`、`StrokeCollection`、`DrawingAttributes`
- **OCR 引擎**：`Tesseract 5.2.0`
- **数据序列化**：`System.Text.Json`
- **发布目标**：Windows x64，self-contained

### 项目结构

```text
PdfTeachAnnotator/
├── Models/                    # 设置、页面、批注和最近文件模型
│   ├── AnnotationFile.cs
│   ├── AppSettings.cs
│   ├── PageModel.cs
│   └── RecentFile.cs
├── ViewModels/                # MVVM 视图模型
│   ├── MainViewModel.cs
│   ├── ToolbarViewModel.cs
│   ├── ViewModelBase.cs
│   └── RelayCommand.cs
├── Services/                  # PDF、批注和 OCR 服务
│   ├── AnnotationFileService.cs
│   ├── IOcrService.cs
│   ├── OcrServiceFactory.cs
│   ├── PdfRenderService.cs
│   └── TesseractOcrService.cs
├── Converters/                # WPF 值转换器
│   └── Converters.cs
├── Resources/                 # 颜色、按钮和主题资源
│   ├── Buttons.xaml
│   ├── Colors.xaml
│   └── Theme.xaml
├── tessdata/                  # Tesseract 语言数据
│   ├── chi_sim.traineddata
│   └── eng.traineddata
├── MainWindow.xaml            # 主窗口 UI
├── MainWindow.xaml.cs         # 主窗口交互逻辑
├── App.xaml                   # 应用入口资源
└── PdfTeachAnnotator.csproj   # 项目文件
```

---

## 🗺️ 路线图

### 已完成 ✅

- [x] PDF 打开、渲染和滚动阅读
- [x] 钢笔、荧光笔、激光笔和橡皮擦
- [x] 颜色和工具大小预设
- [x] 批注保存与重新加载
- [x] 撤销 / 重做
- [x] 最近文件列表
- [x] 深浅色主题切换
- [x] 侧边栏折叠与动画
- [x] 页面切换动画
- [x] 动态页码指示
- [x] 离线 Tesseract OCR

### 计划中 🚧

- [ ] 文本批注
- [ ] 形状工具（矩形、圆形、箭头）
- [ ] 导出带批注的 PDF
- [ ] 批注搜索
- [ ] 批注分层管理
- [ ] PaddleOCR 分支验证和性能评估

---

## 🤝 贡献

欢迎提交 Issue、建议或 Pull Request。

1. Fork 本仓库。
2. 创建特性分支：`git checkout -b feature/your-feature`。
3. 提交更改：`git commit -m 'feat: 添加某功能'`。
4. 推送分支：`git push origin feature/your-feature`。
5. 发起 Pull Request。

如果发现 bug 或有功能建议，请在 [GitHub Issues](https://github.com/LouisLu010/PdfTeachAnnotator/issues) 中反馈。

---

## 📄 许可证

本项目采用 MIT 许可证，详见 [LICENSE](LICENSE) 文件。

---

## 👨‍💻 作者

**Hongjun Lu**

- GitHub：[@LouisLu010](https://github.com/LouisLu010)

---

## 🙏 致谢

- [Docnet.Core](https://github.com/GowenGit/docnet) - .NET PDFium 渲染封装
- [PDFium](https://pdfium.googlesource.com/pdfium/) - PDF 渲染引擎
- [Tesseract OCR](https://github.com/tesseract-ocr/tesseract) - 本地 OCR 引擎
- [.NET](https://dotnet.microsoft.com/) - 应用运行平台
- [LINUX DO](https://linux.do/) - 新的理想型社区

---

<div align="center">

**如果这个项目对您有帮助，欢迎给个 ⭐ Star！**

Made with ❤️ by Hongjun Lu

</div>
