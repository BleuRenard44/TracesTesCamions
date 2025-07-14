using System.Windows;
using System.Windows.Controls;

public static class PromptDialog
{
    public static string? Show(string titre, string defaultText)
    {
        var dlg = new Window
        {
            Title = titre,
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize
        };
        var tb = new TextBox { Text = defaultText, Margin = new Thickness(10) };
        var ok = new Button { Content = "OK", IsDefault = true, Width = 60, Margin = new Thickness(10) };
        ok.Click += (_, __) =>
        {
            dlg.Tag = tb.Text;
            dlg.DialogResult = true;
        };
        dlg.Content = new StackPanel
        {
            Children = { tb, ok }
        };
        return dlg.ShowDialog() == true ? dlg.Tag as string : null;
    }
}
