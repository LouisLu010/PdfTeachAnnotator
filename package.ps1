# 整理发布文件脚本
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "正在整理发布文件..." -ForegroundColor Cyan

$sourceDir = "bin\Release\net8.0-windows\win-x64\publish"
$targetDir = "PdfTeachAnnotator-Release"

# 清理目标目录
if (Test-Path $targetDir) {
    Remove-Item $targetDir -Recurse -Force
}
New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
New-Item -ItemType Directory -Path "$targetDir\lib" -Force | Out-Null

# 复制 exe 和配置文件到根目录
Copy-Item "$sourceDir\PdfTeachAnnotator.exe" -Destination $targetDir
Copy-Item "$sourceDir\PdfTeachAnnotator.deps.json" -Destination $targetDir
Copy-Item "$sourceDir\PdfTeachAnnotator.runtimeconfig.json" -Destination $targetDir
Copy-Item "$sourceDir\createdump.exe" -Destination $targetDir
if (Test-Path "$sourceDir\LICENSE") {
    Copy-Item "$sourceDir\LICENSE" -Destination $targetDir
}

# 复制所有 DLL 到 lib
Get-ChildItem "$sourceDir\*.dll" | Copy-Item -Destination "$targetDir\lib\"

# 复制子目录到 lib
Copy-Item "$sourceDir\x64" -Destination "$targetDir\lib\x64" -Recurse -Force
Copy-Item "$sourceDir\x86" -Destination "$targetDir\lib\x86" -Recurse -Force
Copy-Item "$sourceDir\tessdata" -Destination "$targetDir\lib\tessdata" -Recurse -Force

# 复制语言资源文件夹到根目录
$langDirs = @("cs","de","es","fr","it","ja","ko","pl","pt-BR","ru","tr","zh-Hans","zh-Hant")
foreach ($dir in $langDirs) {
    if (Test-Path "$sourceDir\$dir") {
        Copy-Item "$sourceDir\$dir" -Destination "$targetDir\$dir" -Recurse -Force
    }
}

# 复制 PDB 到 lib
if (Test-Path "$sourceDir\PdfTeachAnnotator.pdb") {
    Copy-Item "$sourceDir\PdfTeachAnnotator.pdb" -Destination "$targetDir\lib\"
}

# 创建 .exe.config
$configContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <probing privatePath="lib" />
    </assemblyBinding>
  </runtime>
</configuration>
"@
Set-Content -Path "$targetDir\PdfTeachAnnotator.exe.config" -Value $configContent -Encoding UTF8

Write-Host ""
Write-Host "整理完成！" -ForegroundColor Green
Write-Host "目标目录: $targetDir"
Write-Host "正在测试运行..."

# 测试启动
Start-Process "$targetDir\PdfTeachAnnotator.exe"
Start-Sleep -Seconds 3
$process = Get-Process -Name "PdfTeachAnnotator" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "应用启动成功" -ForegroundColor Green
    $process | Stop-Process -Force
} else {
    Write-Host "应用启动失败" -ForegroundColor Red
}
