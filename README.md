# MathForge

[![Build](https://github.com/biyuehu/mathforge/actions/workflows/build.yml/badge.svg)](https://github.com/biyuehu/mathforge/actions/workflows/build.yml) [![License: GPL-3.0-only](https://img.shields.io/badge/License-GPL--3.0--only-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) ![C#](https://img.shields.io/badge/C%23-.NET-512BD4?labelColor=green) ![WPF](https://img.shields.io/badge/WPF-Windows-0078D6?logo=windows&logoColor=white) ![MathJax](https://img.shields.io/badge/MathJax-LaTeX-1E6FBA?logo=latex&logoColor=white)

一个基于本地题库的高考数学刷题桌面应用，用 WPF (.NET) 构建，内置 LaTeX 公式渲染。

## 功能

- **顺序刷题**：按题库顺序逐题练习，自动记录进度，支持断点续做
- **筛选刷题**：按知识板块、题型、难度、年份、卷名筛选，支持仅练习未做过的题
- **分类刷题**：按知识板块分类练习
- **错题练习**：自动收录做错的题目，支持设置"连续做对几次视为解决"
- **数据分析**：作答总览、知识板块正确率、错误归因分布

## 技术栈

- .NET 10 / WPF
- WebView2 + MathJax（LaTeX 公式渲染，MathJax 脚本以嵌入资源打包，运行时释放到临时目录，无需联网）

## 开发

初始化：

```bash
lefthook install
```

使用 [just](https://github.com/casey/just)：

```bash
just build
just run
just watch
```

## 数据

题库数据以嵌入资源打包在 `Assets/gaokao-math.json`（已修复），格式为英文键名的结构化 JSON（题干、选项、答案、解析、知识板块、难度、关键词等字段）。

## 许可

GPL-3.0-only
