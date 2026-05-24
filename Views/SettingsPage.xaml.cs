using System.Windows;
using System.Windows.Controls;
using PdfTeachAnnotator.Models;

namespace PdfTeachAnnotator.Views;

public partial class SettingsPage : UserControl
{
    public AppSettings Settings { get; }

    public SettingsPage()
    {
        InitializeComponent();
        Settings = AppSettings.Load();
        DataContext = Settings;
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        Settings.Save();
        MessageBox.Show("设置已保存", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ClearRecent_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("确定要清空最近访问记录吗？", "确认",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Settings.RecentFiles.Clear();
            Settings.Save();
            MessageBox.Show("最近访问记录已清空", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
