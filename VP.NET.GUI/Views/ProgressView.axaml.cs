using Avalonia.Controls;
using VP.NET.GUI.ViewModels;

namespace VP.NET.GUI.Views;

public partial class ProgressView : Window
{
    public ProgressView()
    {
        InitializeComponent();
    }

    private void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        var dt = DataContext as ProgressViewModel;
        if (dt != null)
        {
            dt.Cancel();
        }
    }
}