using System.Windows;
using System.Windows.Controls;
using PdfTeachAnnotator.ViewModels;

namespace PdfTeachAnnotator.Views;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void RecentFile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string filePath)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ViewModel.LoadPdf(filePath);
        }
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        mainWindow?.ViewModel.OpenFileCommand.Execute(null);
    }
}
