@echo off
chcp 65001 > nul
echo 正在整理发布文件...

set SOURCE_DIR=%~dp0bin\Release\net8.0-windows\win-x64\publish
set TARGET_DIR=%~dp0PdfTeachAnnotator-Release

:: 清理目标目录
if exist "%TARGET_DIR%" rmdir /s /q "%TARGET_DIR%"
mkdir "%TARGET_DIR%"
mkdir "%TARGET_DIR%\lib"

:: 复制 exe 和配置文件到根目录
copy "%SOURCE_DIR%\PdfTeachAnnotator.exe" "%TARGET_DIR%\"
copy "%SOURCE_DIR%\PdfTeachAnnotator.deps.json" "%TARGET_DIR%\"
copy "%SOURCE_DIR%\PdfTeachAnnotator.runtimeconfig.json" "%TARGET_DIR%\"
copy "%SOURCE_DIR%\createdump.exe" "%TARGET_DIR%\"
if exist "%SOURCE_DIR%\LICENSE" copy "%SOURCE_DIR%\LICENSE" "%TARGET_DIR%\"

:: 复制所有 DLL 到 lib
xcopy "%SOURCE_DIR%\*.dll" "%TARGET_DIR%\lib\" /Y /Q

:: 复制子目录到 lib
xcopy "%SOURCE_DIR%\x64" "%TARGET_DIR%\lib\x64\" /E /I /Y /Q
xcopy "%SOURCE_DIR%\x86" "%TARGET_DIR%\lib\x86\" /E /I /Y /Q
xcopy "%SOURCE_DIR%\tessdata" "%TARGET_DIR%\lib\tessdata\" /E /I /Y /Q

:: 复制语言资源文件夹到根目录
for %%D in (cs de es fr it ja ko pl pt-BR ru tr zh-Hans zh-Hant) do (
    if exist "%SOURCE_DIR%\%%D" xcopy "%SOURCE_DIR%\%%D" "%TARGET_DIR%\%%D\" /E /I /Y /Q
)

:: 复制 PDB 到 lib
if exist "%SOURCE_DIR%\PdfTeachAnnotator.pdb" copy "%SOURCE_DIR%\PdfTeachAnnotator.pdb" "%TARGET_DIR%\lib\"

:: 创建 .exe.config
echo ^<?xml version="1.0" encoding="utf-8"?^> > "%TARGET_DIR%\PdfTeachAnnotator.exe.config"
echo ^<configuration^> >> "%TARGET_DIR%\PdfTeachAnnotator.exe.config"
echo   ^<runtime^> >> "%TARGET_DIR%\PdfTeachAnnotator.exe.config"
echo     ^<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1"^> >> "%TARGET_DIR%\PdfTeachAnnotator.exe.config"
echo       ^<probing privatePath="lib" /^> >> "%TARGET_DIR%\PdfTeachAnnotator.exe.config"
echo     ^</assemblyBinding^> >> "%TARGET_DIR%\PdfTeachAnnotator.exe.config"
echo   ^</runtime^> >> "%TARGET_DIR%\PdfTeachAnnotator.exe.config"
echo ^</configuration^> >> "%TARGET_DIR%\PdfTeachAnnotator.exe.config"

echo.
echo 整理完成！
echo 目标目录: %TARGET_DIR%
pause
