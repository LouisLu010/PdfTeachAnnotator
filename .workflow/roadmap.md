# Roadmap: PDF Teach Annotator

## Overview

构建一个面向教学演示的 PDF 批注桌面工具。单阶段完成全部功能：从 PDF 渲染引擎到批注层、缩放、保存/恢复，最终打包为免安装单文件 exe。内部通过 wave DAG 管理任务依赖和并行度。

## Phases

**Minimum-phase principle:** Default 1 phase. Wave DAG inside the phase handles task ordering.

- [ ] **Phase 1: Full Application Build** - 交付完整 PDF 教学批注工具，含所有核心功能

## Phase Details

### Phase 1: Full Application Build
**Goal**: 交付完整的 PDF 教学批注工具，包含 PDF 连续滚动阅读、画笔批注、橡皮擦除、清屏、缩放、保存/恢复批注、单文件 exe 分发
**Depends on**: Nothing (first phase)
**Requirements**: REQ-01 (PDF 连续滚动), REQ-02 (画笔颜色), REQ-03 (橡皮大小), REQ-04 (一键清屏), REQ-05 (缩放), REQ-06 (批注保存), REQ-07 (批注恢复), REQ-08 (单文件 exe)
**Success Criteria** (what must be TRUE):
  1. 用户可打开任意 PDF 并连续滚动阅读，横向自适应屏幕宽度
  2. 画笔可选颜色在 PDF 页面上自由标注，橡皮可调大小擦除笔迹
  3. 一键清屏清除当前所有批注
  4. 可缩放查看 PDF 及批注，批注跟随缩放不漂移
  5. 批注可保存为独立文件，重新打开 PDF 时可恢复之前的批注
  6. 生成单个 exe 文件，在其他 Win10+ 机器上免安装直接运行

**Wave DAG** (task ordering within phase):
- Wave 1: 项目骨架 + PDF 渲染引擎（PDFium 集成、连续滚动、适应宽度）
- Wave 2: 批注层（InkCanvas 覆盖、画笔颜色、橡皮大小、清屏）
- Wave 3: 缩放 + 批注坐标同步
- Wave 4: 保存/恢复批注文件
- Wave 5: 单文件发布 + 打包优化

## Scope Decisions

- **In scope**: PDF 阅读（连续滚动、适应宽度、缩放）、批注（画笔、橡皮、清屏）、持久化（保存/恢复）、分发（单文件 exe）
- **Deferred**: 文本高亮、形状工具、多文档标签页、触控优化
- **Out of scope**: PDF 编辑、文本搜索、网络功能、跨平台、打印

## Progress

| Phase | Status | Completed |
|-------|--------|-----------|
| 1. Full Application Build | Not started | - |
