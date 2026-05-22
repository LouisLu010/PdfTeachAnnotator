# Project: PDF Teach Annotator

## What This Is

一个面向教学演示场景的轻量级 PDF 批注工具。教师可以在 PDF 上实时画标注、擦除、清屏，批注独立保存和恢复。免安装单文件 exe，打开即用。

## Core Value

教学演示时零摩擦地在 PDF 上画标注——打开就能用，不需要安装、不需要网络。

## Requirements

### Validated

<!-- Shipped and confirmed valuable. -->

(None yet — ship to validate)

### Active

<!-- Current scope being built toward. These are hypotheses until shipped. -->

- [ ] PDF 连续滚动阅读（横向适应屏幕宽度）
- [ ] 画笔工具（可调整颜色）
- [ ] 橡皮工具（可调整大小）
- [ ] 一键清屏（清除当前所有批注）
- [ ] 缩放功能（放大/缩小 PDF 及批注）
- [ ] 批注独立保存为文件（与 PDF 分离）
- [ ] 批注恢复（打开 PDF 时加载已保存批注）
- [ ] 单文件 exe 免安装分发

### Out of Scope

<!-- Explicit boundaries. Include reasoning to prevent re-adding. -->

- 编辑 PDF 内容 — 本工具仅做批注覆盖层，不修改原始 PDF
- 文本选择/搜索 — 教学演示场景以视觉标注为主
- 网络功能 — 目标环境无网络，避免复杂度
- 多平台支持 — 专注 Windows 10+ 体验
- 打印功能 — 超出核心教学批注场景

## Context

教学场景中教师需要在 PDF 课件上实时标注重点、画图解释。现有工具要么太重（Adobe）、要么需要安装、要么批注不能保存。需要一个极简的"打开-标注-保存"工具。

## Constraints

- **部署**: 单文件 exe，免安装 — 学校电脑可能无管理员权限
- **网络**: 完全离线运行 — 教室环境可能无网络
- **兼容性**: Windows 10+ — 学校电脑最低配置
- **体积**: 尽量控制在 50MB 以内 — 便于 U 盘分发

## Tech Stack

- **Language**: C# (.NET 8)
- **Framework**: WPF
- **PDF Engine**: PDFium (via PDFiumSharp)
- **Annotation**: WPF InkCanvas

## Key Decisions

<!-- Decisions that constrain future work. Add throughout project lifecycle. -->

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| WPF over WinUI 3 | InkCanvas 成熟稳定，单文件发布更简单 | — Pending |
| PDFium 作为 PDF 引擎 | BSD 许可证，性能好，可嵌入 | — Pending |
| 批注独立文件存储 | 不修改原 PDF，避免权限和格式问题 | — Pending |
| ISF + JSON 混合格式 | ISF 是 WPF 原生墨迹格式，紧凑高效 | — Pending |

## Stakeholders

- 教师（主要用户）
- 学生（间接受益，看到更清晰的标注演示）

---
*Last updated: 2026-05-10 after initialization*
