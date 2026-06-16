using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;

namespace PdfTeachAnnotator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 全局异常处理
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        // 日志启动信息
        LogStartupInfo();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogException("CurrentDomain.UnhandledException", exception);
        MessageBox.Show($"应用程序发生严重错误:\n{exception?.Message}\n\n详细信息已保存到 error.log",
            "严重错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogException("Dispatcher.UnhandledException", e.Exception);
        MessageBox.Show($"应用程序发生错误:\n{e.Exception.Message}\n\n详细信息已保存到 error.log",
            "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void LogException(string source, Exception? exception)
    {
        if (exception == null) return;

        try
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            var logMessage = $@"
=== {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===
Source: {source}
Message: {exception.Message}
Type: {exception.GetType().FullName}
StackTrace:
{exception.StackTrace}

InnerException: {exception.InnerException?.Message}
{exception.InnerException?.StackTrace}
";
            File.AppendAllText(logFile, logMessage);
        }
        catch
        {
            // 记录日志失败，忽略
        }
    }

    private void LogStartupInfo()
    {
        try
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log");
            var logMessage = $@"
=== {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===
BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}
OS: {Environment.OSVersion}
.NET: {Environment.Version}
x64 Process: {Environment.Is64BitProcess}
x64 OS: {Environment.Is64BitOperatingSystem}

检查关键文件:
- PdfTeachAnnotator.dll: {File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfTeachAnnotator.dll"))}
- tessdata 目录: {Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"))}
- x64/pdfium.dll: {File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64", "pdfium.dll"))}
";
            File.WriteAllText(logFile, logMessage);
        }
        catch
        {
            // 记录日志失败，忽略
        }
    }
}

