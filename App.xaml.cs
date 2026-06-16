using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using System.Diagnostics;

namespace PdfTeachAnnotator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        // 尽早初始化异常处理
        InitializeExceptionHandling();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            // 日志启动信息
            LogStartupInfo();
        }
        catch (Exception ex)
        {
            LogException("OnStartup", ex);
            ShowCriticalError($"应用启动失败:\n{ex.Message}\n\n详细错误已保存到 error.log\n\n请运行\"诊断工具.bat\"进行诊断", ex);
            Environment.Exit(1);
        }
    }

    private void InitializeExceptionHandling()
    {
        // 全局异常处理
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogException("CurrentDomain.UnhandledException", exception);

        ShowCriticalError(
            $"应用程序发生严重错误:\n{exception?.Message}\n\n" +
            $"详细信息已保存到 error.log\n\n" +
            $"请运行\"诊断工具.bat\"进行诊断，或将日志文件发送给开发者。",
            exception);
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogException("Dispatcher.UnhandledException", e.Exception);

        var message = $"应用程序发生错误:\n{e.Exception.Message}\n\n" +
                     $"详细信息已保存到 error.log\n\n" +
                     $"如果问题持续，请运行\"诊断工具.bat\"";

        ShowErrorMessage(message, e.Exception);
        e.Handled = true;
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException("TaskScheduler.UnobservedTaskException", e.Exception);
        e.SetObserved();
    }

    private void ShowCriticalError(string message, Exception? exception)
    {
        try
        {
            MessageBox.Show(message, "严重错误 - PDF 教学批注工具",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch
        {
            // 如果连 MessageBox 都无法显示，写入文件
            try
            {
                var criticalLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "critical.log");
                File.WriteAllText(criticalLog, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{message}\n{exception}");
            }
            catch { }
        }
    }

    private void ShowErrorMessage(string message, Exception? exception)
    {
        try
        {
            MessageBox.Show(message, "错误 - PDF 教学批注工具",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch
        {
            LogException("ShowErrorMessage.Failed", exception);
        }
    }

    private void LogException(string source, Exception? exception)
    {
        if (exception == null) return;

        try
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            var logMessage = $@"
========================================
时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
来源: {source}
========================================
错误类型: {exception.GetType().FullName}
错误消息: {exception.Message}

堆栈跟踪:
{exception.StackTrace}

内部异常: {exception.InnerException?.GetType().FullName}
内部消息: {exception.InnerException?.Message}
内部堆栈:
{exception.InnerException?.StackTrace}

加载的程序集:
{string.Join("\n", AppDomain.CurrentDomain.GetAssemblies().Select(a => $"  - {a.GetName().Name} {a.GetName().Version}"))}
";
            File.AppendAllText(logFile, logMessage);
        }
        catch (Exception logEx)
        {
            // 最后的手段 - 写入事件日志
            try
            {
                EventLog.WriteEntry("Application",
                    $"PdfTeachAnnotator logging failed: {logEx.Message}\nOriginal: {exception.Message}",
                    EventLogEntryType.Error);
            }
            catch { }
        }
    }

    private void LogStartupInfo()
    {
        try
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log");
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var logMessage = $@"
========================================
启动时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
========================================
基础目录: {baseDir}
工作目录: {Environment.CurrentDirectory}
命令行: {Environment.CommandLine}

系统信息:
- 操作系统: {Environment.OSVersion}
- .NET 版本: {Environment.Version}
- 64位进程: {Environment.Is64BitProcess}
- 64位系统: {Environment.Is64BitOperatingSystem}
- 处理器数: {Environment.ProcessorCount}
- 系统内存: {GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024} MB

关键文件检查:
- PdfTeachAnnotator.dll: {File.Exists(Path.Combine(baseDir, "PdfTeachAnnotator.dll"))}
- pdfium.dll: {File.Exists(Path.Combine(baseDir, "pdfium.dll"))}
- Tesseract.dll: {File.Exists(Path.Combine(baseDir, "Tesseract.dll"))}
- x64/tesseract50.dll: {File.Exists(Path.Combine(baseDir, "x64", "tesseract50.dll"))}
- x64/leptonica-1.82.0.dll: {File.Exists(Path.Combine(baseDir, "x64", "leptonica-1.82.0.dll"))}
- tessdata 目录: {Directory.Exists(Path.Combine(baseDir, "tessdata"))}
- tessdata/chi_sim.traineddata: {File.Exists(Path.Combine(baseDir, "tessdata", "chi_sim.traineddata"))}
- tessdata/eng.traineddata: {File.Exists(Path.Combine(baseDir, "tessdata", "eng.traineddata"))}

WPF 程序集:
- PresentationFramework: {AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "PresentationFramework")}
- PresentationCore: {AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "PresentationCore")}
- WindowsBase: {AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "WindowsBase")}
";
            File.WriteAllText(logFile, logMessage);
        }
        catch (Exception ex)
        {
            // 启动日志失败也要记录
            try
            {
                var errorFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup-error.log");
                File.WriteAllText(errorFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex}");
            }
            catch { }
        }
    }
}

