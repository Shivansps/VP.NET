using Avalonia.Controls;
using VP.NET.GUI.ViewModels;

namespace VP.NET.GUI.Views;

public partial class PreviewerView : UserControl
{
    public PreviewerView()
    {
        InitializeComponent();
    }

    private void Slider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        var dt = this.DataContext as PreviewerViewModel;
        if (dt != null)
        {
            dt.UpdateMediaVolume();
        }
    }
}