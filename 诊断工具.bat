@echo off
chcp 65001 > nul
title PDF 教学批注工具 - 启动诊断

echo.
echo ========================================
echo   PDF 教学批注工具 - 启动诊断
echo ========================================
echo.

:: 检查是否在正确的目录
if not exist "PdfTeachAnnotator.exe" (
    echo [错误] 未找到 PdfTeachAnnotator.exe
    echo 请确保在应用程序目录下运行此脚本
    pause
    exit /b
)

echo [1/5] 检查系统信息...
systeminfo | findstr /C:"OS 名称" /C:"OS 版本" /C:"系统类型"

echo.
echo [2/5] 检查关键文件...
if exist "PdfTeachAnnotator.dll" (echo [√] PdfTeachAnnotator.dll) else (echo [×] PdfTeachAnnotator.dll 缺失)
if exist "pdfium.dll" (echo [√] pdfium.dll) else (echo [×] pdfium.dll 缺失)
if exist "x64\tesseract50.dll" (echo [√] x64\tesseract50.dll) else (echo [×] x64\tesseract50.dll 缺失)
if exist "x64\leptonica-1.82.0.dll" (echo [√] x64\leptonica-1.82.0.dll) else (echo [×] x64\leptonica-1.82.0.dll 缺失)
if exist "tessdata\chi_sim.traineddata" (echo [√] tessdata\chi_sim.traineddata) else (echo [×] tessdata\chi_sim.traineddata 缺失)
if exist "tessdata\eng.traineddata" (echo [√] tessdata\eng.traineddata) else (echo [×] tessdata\eng.traineddata 缺失)

echo.
echo [3/5] 检查 Visual C++ 运行库...
reg query "HKLM\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" >nul 2>&1
if %errorlevel% equ 0 (
    echo [√] Visual C++ 2015-2022 x64 已安装
) else (
    echo [!] 未检测到 Visual C++ 2015-2022 x64 运行库
    echo     Tesseract OCR 需要此运行库
    echo     下载地址: https://aka.ms/vs/17/release/vc_redist.x64.exe
)

echo.
echo [4/5] 尝试启动应用程序...
start "" "PdfTeachAnnotator.exe"
timeout /t 3 /nobreak > nul

tasklist | find /i "PdfTeachAnnotator.exe" > nul
if %errorlevel% equ 0 (
    echo [√] 应用程序正在运行
    echo.
    echo 请检查应用程序窗口是否正常显示
    echo 如果窗口正常，可以关闭此诊断窗口
) else (
    echo [×] 应用程序未运行或已崩溃
)

echo.
echo [5/5] 检查错误日志...
if exist "error.log" (
    echo [!] 发现错误日志，最后几行内容：
    echo ----------------------------------------
    powershell -Command "Get-Content error.log -Tail 10 -Encoding UTF8"
    echo ----------------------------------------
) else (
    echo [i] 未找到 error.log
)

if exist "startup.log" (
    echo.
    echo [i] 发现启动日志，最后几行内容：
    echo ----------------------------------------
    powershell -Command "Get-Content startup.log -Tail 10 -Encoding UTF8"
    echo ----------------------------------------
)

echo.
echo ========================================
echo   诊断完成
echo ========================================
echo.
echo 如果应用仍无法启动，请执行以下操作：
echo 1. 安装 Visual C++ 运行库（如上方提示）
echo 2. 将 error.log 和 startup.log 发送给开发者
echo 3. 确保 Windows 版本为 Windows 10/11 64位
echo.
pause
