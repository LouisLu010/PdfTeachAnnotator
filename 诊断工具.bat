@echo off
chcp 65001 > nul
title PDF 教学批注工具 - 完整诊断报告

echo.
echo ========================================
echo   PDF 教学批注工具 - 完整诊断
echo ========================================
echo.

:: 生成诊断报告文件
set REPORT=diagnostic-report.txt
echo PDF 教学批注工具 - 诊断报告 > %REPORT%
echo 生成时间: %date% %time% >> %REPORT%
echo ======================================== >> %REPORT%
echo. >> %REPORT%

:: 检查是否在正确的目录
if not exist "PdfTeachAnnotator.exe" (
    echo [错误] 未找到 PdfTeachAnnotator.exe
    echo [错误] 未找到 PdfTeachAnnotator.exe >> %REPORT%
    echo 请确保在应用程序目录下运行此脚本
    pause
    exit /b
)

echo [1/8] 检查系统信息...
echo. >> %REPORT%
echo === 系统信息 === >> %REPORT%
systeminfo | findstr /C:"OS 名称" /C:"OS 版本" /C:"系统类型" /C:"处理器" >> %REPORT%
systeminfo | findstr /C:"OS 名称" /C:"OS 版本" /C:"系统类型"

echo.
echo [2/8] 检查 .NET 运行时...
echo. >> %REPORT%
echo === .NET 运行时 === >> %REPORT%
dotnet --list-runtimes 2>nul >> %REPORT%
if %errorlevel% neq 0 (
    echo [!] 未安装 dotnet CLI（不影响应用运行，因为是自包含发布） >> %REPORT%
)

echo.
echo [3/8] 检查关键文件...
echo. >> %REPORT%
echo === 关键文件检查 === >> %REPORT%

set FILES_OK=1
if exist "PdfTeachAnnotator.exe" (echo [√] PdfTeachAnnotator.exe & echo [√] PdfTeachAnnotator.exe >> %REPORT%) else (echo [×] PdfTeachAnnotator.exe 缺失 & echo [×] PdfTeachAnnotator.exe 缺失 >> %REPORT% & set FILES_OK=0)
if exist "PdfTeachAnnotator.dll" (echo [√] PdfTeachAnnotator.dll & echo [√] PdfTeachAnnotator.dll >> %REPORT%) else (echo [×] PdfTeachAnnotator.dll 缺失 & echo [×] PdfTeachAnnotator.dll 缺失 >> %REPORT% & set FILES_OK=0)
if exist "pdfium.dll" (echo [√] pdfium.dll & echo [√] pdfium.dll >> %REPORT%) else (echo [×] pdfium.dll 缺失 & echo [×] pdfium.dll 缺失 >> %REPORT% & set FILES_OK=0)
if exist "Tesseract.dll" (echo [√] Tesseract.dll & echo [√] Tesseract.dll >> %REPORT%) else (echo [×] Tesseract.dll 缺失 & echo [×] Tesseract.dll 缺失 >> %REPORT% & set FILES_OK=0)
if exist "x64\tesseract50.dll" (echo [√] x64\tesseract50.dll & echo [√] x64\tesseract50.dll >> %REPORT%) else (echo [×] x64\tesseract50.dll 缺失 & echo [×] x64\tesseract50.dll 缺失 >> %REPORT% & set FILES_OK=0)
if exist "x64\leptonica-1.82.0.dll" (echo [√] x64\leptonica-1.82.0.dll & echo [√] x64\leptonica-1.82.0.dll >> %REPORT%) else (echo [×] x64\leptonica-1.82.0.dll 缺失 & echo [×] x64\leptonica-1.82.0.dll 缺失 >> %REPORT% & set FILES_OK=0)
if exist "tessdata\chi_sim.traineddata" (echo [√] tessdata\chi_sim.traineddata & echo [√] tessdata\chi_sim.traineddata >> %REPORT%) else (echo [×] tessdata\chi_sim.traineddata 缺失 & echo [×] tessdata\chi_sim.traineddata 缺失 >> %REPORT% & set FILES_OK=0)
if exist "tessdata\eng.traineddata" (echo [√] tessdata\eng.traineddata & echo [√] tessdata\eng.traineddata >> %REPORT%) else (echo [×] tessdata\eng.traineddata 缺失 & echo [×] tessdata\eng.traineddata 缺失 >> %REPORT% & set FILES_OK=0)

if %FILES_OK% equ 0 (
    echo. >> %REPORT%
    echo [!] 发现缺失文件，请重新下载完整的发布包 >> %REPORT%
)

echo.
echo [4/8] 检查 Visual C++ 运行库...
echo. >> %REPORT%
echo === Visual C++ 运行库 === >> %REPORT%

set VCPP_OK=0
reg query "HKLM\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" >nul 2>&1
if %errorlevel% equ 0 (
    echo [√] Visual C++ 2015-2022 x64 已安装
    echo [√] Visual C++ 2015-2022 x64 已安装 >> %REPORT%
    reg query "HKLM\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" /v Version >> %REPORT% 2>&1
    set VCPP_OK=1
) else (
    echo [!] 未检测到 Visual C++ 2015-2022 x64 运行库
    echo [!] 未检测到 Visual C++ 2015-2022 x64 运行库 >> %REPORT%
    echo     Tesseract OCR 需要此运行库
    echo     下载地址: https://aka.ms/vs/17/release/vc_redist.x64.exe
    echo     下载地址: https://aka.ms/vs/17/release/vc_redist.x64.exe >> %REPORT%
)

echo.
echo [5/8] 检查依赖 DLL...
echo. >> %REPORT%
echo === 依赖 DLL 检查 === >> %REPORT%
powershell -Command "try { [System.Reflection.Assembly]::LoadFile('%CD%\PdfTeachAnnotator.dll'); 'PdfTeachAnnotator.dll 可加载' } catch { 'PdfTeachAnnotator.dll 加载失败: ' + $_.Exception.Message }" >> %REPORT% 2>&1

echo.
echo [6/8] 尝试启动应用程序...
echo. >> %REPORT%
echo === 应用启动测试 === >> %REPORT%

:: 删除旧日志
if exist "startup.log" del "startup.log"
if exist "error.log" del "error.log"
if exist "critical.log" del "critical.log"

echo 正在启动...
start "" "PdfTeachAnnotator.exe"
timeout /t 5 /nobreak > nul

tasklist | find /i "PdfTeachAnnotator.exe" > nul
if %errorlevel% equ 0 (
    echo [√] 应用程序正在运行
    echo [√] 应用程序正在运行 >> %REPORT%
    echo.
    echo 请检查应用程序窗口是否正常显示

    :: 等待用户确认
    echo.
    set /p "RUNNING=应用窗口是否正常显示？(Y/N): "
    if /i "%RUNNING%"=="N" (
        echo [!] 应用进程存在但窗口未显示 >> %REPORT%
    )

    :: 关闭测试进程
    taskkill /f /im PdfTeachAnnotator.exe >nul 2>&1
) else (
    echo [×] 应用程序未运行或已崩溃
    echo [×] 应用程序未运行或已崩溃 >> %REPORT%
)

echo.
echo [7/8] 检查应用日志...
echo. >> %REPORT%
echo === 应用日志 === >> %REPORT%

if exist "startup.log" (
    echo. >> %REPORT%
    echo --- startup.log --- >> %REPORT%
    type startup.log >> %REPORT%

    echo [i] 发现启动日志，部分内容：
    powershell -Command "Get-Content startup.log -Encoding UTF8 | Select-Object -First 15"
) else (
    echo [!] 未生成 startup.log - 应用可能在初始化前就崩溃了
    echo [!] 未生成 startup.log >> %REPORT%
)

if exist "error.log" (
    echo. >> %REPORT%
    echo --- error.log --- >> %REPORT%
    type error.log >> %REPORT%

    echo.
    echo [!] 发现错误日志：
    echo ========================================
    powershell -Command "Get-Content error.log -Encoding UTF8 | Select-Object -Last 20"
    echo ========================================
) else (
    echo [i] 未找到 error.log
    echo [i] 未找到 error.log >> %REPORT%
)

if exist "critical.log" (
    echo. >> %REPORT%
    echo --- critical.log --- >> %REPORT%
    type critical.log >> %REPORT%

    echo.
    echo [!] 发现严重错误日志：
    type critical.log
)

echo.
echo [8/8] 检查 Windows 事件日志...
echo. >> %REPORT%
echo === Windows 事件日志（应用程序错误） === >> %REPORT%
powershell -Command "Get-EventLog -LogName Application -Source 'Application Error' -Newest 5 -ErrorAction SilentlyContinue | Where-Object { $_.Message -like '*PdfTeachAnnotator*' } | Format-List TimeGenerated, Message" >> %REPORT% 2>&1

echo.
echo ========================================
echo   诊断完成
echo ========================================
echo.

type %REPORT% | find /i "×" >nul
if %errorlevel% equ 0 (
    echo [!] 发现问题，请查看诊断报告
)

echo 完整诊断报告已保存到: %REPORT%
echo.
echo 如果应用仍无法启动：
echo 1. 查看 %REPORT% 了解详细信息
if %VCPP_OK% equ 0 echo 2. 安装 Visual C++ 运行库: https://aka.ms/vs/17/release/vc_redist.x64.exe
if %FILES_OK% equ 0 echo 3. 重新下载完整的发布包
echo 4. 将 %REPORT% 发送给开发者
echo.
pause
