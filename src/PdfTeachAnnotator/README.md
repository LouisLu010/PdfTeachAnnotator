# PDF 教学批注工具

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?style=flat-square&logo=windows)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

一款现代化的 PDF 批注工具，专为教学场景设计，支持手写批注、多种画笔工具和自动保存功能。

[功能特性](#功能特性) • [快速开始](#快速开始) • [使用说明](#使用说明) • [技术栈](#技术栈)

</div>

---

## ✨ 功能特性

### 📝 核心功能

- **PDF 渲染与显示**
  - 高质量 PDF 渲染（基于 PDFium）
  - 流畅的页面滚动和缩放
  - 支持多页 PDF 文档

- **手写批注**
  - 自由手写批注
  - 多种画笔粗细（5 档可选：1, 3, 5, 8, 12）
  - 8 种预设颜色（红、蓝、绿、黑、橙、紫、黄、白）
  - 橡皮擦工具（5 档大小：10, 20, 30, 40, 50）
  - 平滑绘图支持

- **批注管理**
  - 自动保存批注到独立文件
  - 批注与 PDF 分离存储
  - 重新打开时自动加载批注
  - 滑动确认清除功能（防止误操作）

### 🎨 界面设计

- **现代化 UI**
  - WPF-UI Fluent Design 风格
  - 深色主题（护眼舒适）
  - Mica 半透明背景效果
  - 响应式布局

- **直观的工具栏**
  - 圆形按钮选择画笔/橡皮擦大小
  - 颜色选择器
  - 缩放控制（放大/缩小/重置）
  - 动态画笔图标（显示当前颜色）

- **智能导航**
  - 主页、PDF 编辑、设置三视图
  - 智能返回（记住上一个页面）
  - 最近访问文件列表
  - 快捷菜单（三横线图标）

### ⚙️ 高级功能

- **设置选项**
  - 自动保存开关
  - 启动时显示主页
  - 默认画笔/橡皮擦大小
  - 默认缩放级别
  - 平滑绘图开关
  - 工具提示开关
  - 最近文件数量配置

- **用户体验**
  - 拖放打开 PDF 文件
  - 键盘快捷键支持
  - 滑动确认清除（带进度条和勾号）
  - 窗口控制（最小化、最大化、关闭）

---

## 🚀 快速开始

### 系统要求

- **操作系统**: Windows 10/11 (x64)
- **.NET 运行时**: .NET 8.0 或更高版本
- **内存**: 建议 4GB 以上
- **显示器**: 支持 1920x1080 或更高分辨率

### 安装步骤

#### 方法 1：下载预编译版本（推荐）

1. 前往 [Releases](https://github.com/yourusername/PdfTeachAnnotator/releases) 页面
2. 下载最新版本的 `PdfTeachAnnotator-win-x64.zip`
3. 解压到任意目录
4. 运行 `PdfTeachAnnotator.exe`

#### 方法 2：从源码编译

```bash
# 克隆仓库
git clone https://github.com/yourusername/PdfTeachAnnotator.git
cd PdfTeachAnnotator

# 编译项目
dotnet build -c Release

# 运行应用
dotnet run --project src/PdfTeachAnnotator/PdfTeachAnnotator.csproj
```

---

## 📖 使用说明

### 基本操作

#### 1. 打开 PDF 文件

- **方法 1**: 点击主页的"打开 PDF 文件"按钮
- **方法 2**: 点击工具栏的"📂"图标
- **方法 3**: 从最近访问列表中选择
- **方法 4**: 拖放 PDF 文件到窗口

#### 2. 批注工具

**画笔工具** 🖊️
- 点击工具栏的"✏️"图标激活画笔
- 选择颜色：点击颜色方块
- 选择粗细：点击圆形按钮（1-12）
- 在 PDF 上自由绘制

**橡皮擦工具** 🧹
- 点击工具栏的"🧹"图标激活橡皮擦
- 选择大小：点击圆形按钮（10-50）
- 在批注上擦除

**清除所有批注** 🗑️
- 点击工具栏的"🗑️"图标
- 向右滑动确认条到底
- 显示绿色勾号后自动清除

#### 3. 视图控制

**缩放** 🔍
- 放大：点击"🔍+"或滚轮向上
- 缩小：点击"🔍-"或滚轮向下
- 重置：点击"🔄"图标

**滚动**
- 鼠标滚轮：上下滚动
- 拖动滚动条：快速定位

#### 4. 保存批注

- **自动保存**：关闭文件时自动保存（可在设置中关闭）
- **手动保存**：点击菜单 → 保存批注

### 高级功能

#### 设置选项

点击菜单 → 设置，可配置：

- **常规设置**
  - 自动保存批注
  - 启动时显示主页
  - 显示工具提示

- **绘图设置**
  - 默认画笔粗细（1-20）
  - 默认橡皮擦大小（5-50）
  - 平滑绘图

- **视图设置**
  - 默认缩放级别（50%-200%）

- **最近文件**
  - 最大记录数量（5-20）
  - 清空最近访问

#### 键盘快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl + O` | 打开 PDF 文件 |
| `Ctrl + S` | 保存批注 |
| `Ctrl + +` | 放大 |
| `Ctrl + -` | 缩小 |
| `Ctrl + 0` | 重置缩放 |

---

## 🎯 优点与特色

### 为什么选择 PDF 教学批注工具？

#### 1. 🎨 专为教学设计

- **直观的批注工具**：简单易用，无需学习成本
- **多种画笔选项**：满足不同批注需求
- **清晰的视觉反馈**：选中状态一目了然

#### 2. 💾 智能批注管理

- **独立存储**：批注与原 PDF 分离，不修改原文件
- **自动保存**：防止批注丢失
- **快速加载**：重新打开时自动恢复批注

#### 3. 🚀 高性能

- **流畅渲染**：基于 PDFium 引擎
- **低内存占用**：优化的资源管理
- **快速响应**：即时的绘图反馈

#### 4. 🎨 现代化界面

- **Fluent Design**：符合 Windows 11 设计语言
- **深色主题**：护眼舒适，长时间使用不疲劳
- **响应式布局**：适配不同屏幕尺寸

#### 5. 🔒 安全可靠

- **本地处理**：所有数据本地存储，无需联网
- **隐私保护**：不上传任何文件或批注
- **开源透明**：代码完全开源，可审计

#### 6. 🛠️ 高度可定制

- **丰富的设置选项**：自定义默认值
- **灵活的工具配置**：适应不同使用习惯
- **可扩展架构**：易于添加新功能

---

## 🏗️ 技术栈

### 核心技术

- **框架**: .NET 8.0
- **UI 框架**: WPF (Windows Presentation Foundation)
- **UI 库**: WPF-UI 3.0.5 (Fluent Design)
- **PDF 引擎**: PDFium.Windows 129.0.6668
- **数据序列化**: System.Text.Json

### 架构设计

- **设计模式**: MVVM (Model-View-ViewModel)
- **数据绑定**: WPF Data Binding
- **命令模式**: ICommand / RelayCommand
- **转换器**: IValueConverter / IMultiValueConverter

### 项目结构

```
PdfTeachAnnotator/
├── src/
│   └── PdfTeachAnnotator/
│       ├── Models/              # 数据模型
│       │   ├── AppSettings.cs
│       │   ├── PageModel.cs
│       │   └── RecentFile.cs
│       ├── ViewModels/          # 视图模型
│       │   ├── MainViewModel.cs
│       │   ├── ToolbarViewModel.cs
│       │   └── ViewModelBase.cs
│       ├── Services/            # 服务层
│       │   ├── PdfRenderService.cs
│       │   └── AnnotationFileService.cs
│       ├── Converters/          # 值转换器
│       │   └── Converters.cs
│       ├── MainWindow.xaml      # 主窗口
│       └── App.xaml             # 应用程序
├── README.md
└── LICENSE
```

---

## 🗺️ 路线图

### 已完成 ✅

- [x] PDF 渲染与显示
- [x] 手写批注功能
- [x] 多种画笔工具
- [x] 批注保存与加载
- [x] 深色主题
- [x] 设置页面
- [x] 最近文件管理
- [x] 圆形按钮选择大小

### 计划中 🚧

- [ ] 文本批注（添加文字）
- [ ] 形状工具（矩形、圆形、箭头）
- [ ] 批注导出（导出为带批注的 PDF）
- [ ] 多语言支持
- [ ] 触摸屏优化
- [ ] 批注搜索功能
- [ ] 批注历史记录（撤销/重做）
- [ ] 批注分层管理

---

## 🤝 贡献

欢迎贡献代码、报告问题或提出建议！

### 如何贡献

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

### 报告问题

如果您发现 bug 或有功能建议，请[创建 Issue](https://github.com/yourusername/PdfTeachAnnotator/issues)。

---

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件。

---

## 👨‍💻 作者

**Hongjun Lu**

- GitHub: [@yourusername](https://github.com/yourusername)

---

## 🙏 致谢

- [WPF-UI](https://github.com/lepoco/wpfui) - 现代化的 WPF UI 库
- [PDFium](https://pdfium.googlesource.com/pdfium/) - 强大的 PDF 渲染引擎
- [.NET Foundation](https://dotnetfoundation.org/) - .NET 平台支持

---

## 📞 联系方式

如有问题或建议，欢迎通过以下方式联系：

- 💬 Issues: [GitHub Issues](https://github.com/yourusername/PdfTeachAnnotator/issues)

---

<div align="center">

**如果这个项目对您有帮助，请给个 ⭐ Star！**

Made with ❤️ by Hongjun Lu

</div>
