# VuePress 文档

本目录包含 PDF 教学批注工具的 VuePress 文档网站。

## 本地开发

### 安装依赖

```bash
cd docs
npm install
```

### 启动开发服务器

```bash
npm run docs:dev
```

访问 http://localhost:8080 查看文档。

### 构建生产版本

```bash
npm run docs:build
```

构建输出位于 `docs/.vuepress/dist` 目录。

## 部署

### GitHub Pages

1. 在 GitHub 仓库设置中启用 GitHub Pages
2. 选择 `gh-pages` 分支作为源
3. 推送代码后自动部署

### 手动部署

```bash
npm run docs:build
cd .vuepress/dist
# 将 dist 目录内容部署到服务器
```

## 文档结构

```
docs/
├── .vuepress/
│   └── config.js          # VuePress 配置
├── guide/
│   ├── getting-started.md # 快速开始
│   ├── features.md        # 功能介绍
│   ├── settings.md        # 设置选项
│   ├── shortcuts.md       # 快捷键
│   └── faq.md            # 常见问题
├── package.json          # 依赖配置
└── README.md            # 首页
```

## 贡献

欢迎提交文档改进建议！
