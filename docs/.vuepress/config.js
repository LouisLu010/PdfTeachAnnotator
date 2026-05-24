import { defaultTheme } from '@vuepress/theme-default'
import { defineUserConfig } from 'vuepress/cli'
import { viteBundler } from '@vuepress/bundler-vite'

export default defineUserConfig({
  lang: 'zh-CN',
  title: 'PDF 教学批注工具',
  description: '轻松为 PDF 添加批注和标记的桌面应用',

  base: '/PdfTeachAnnotator/',

  theme: defaultTheme({
    logo: '/images/logo.png',
    navbar: [
      { text: '首页', link: '/' },
      { text: '快速开始', link: '/guide/getting-started' },
      { text: '功能介绍', link: '/guide/features' },
      { text: '常见问题', link: '/guide/faq' },
      { text: 'GitHub', link: 'https://github.com/LouisLu010/PdfTeachAnnotator' }
    ],
    sidebar: {
      '/guide/': [
        {
          text: '指南',
          children: [
            '/guide/getting-started.md',
            '/guide/features.md',
            '/guide/settings.md',
            '/guide/shortcuts.md',
            '/guide/faq.md'
          ]
        }
      ]
    },
    editLink: false,
    lastUpdated: true,
    contributors: false
  }),

  bundler: viteBundler()
})
