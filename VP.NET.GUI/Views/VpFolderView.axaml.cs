using Avalonia.Controls;
using VP.NET.GUI.ViewModels;

namespace VP.NET.GUI.Views;

public partial class VpFolderView : UserControl
{
    public VpFolderView()
    {
        InitializeComponent();
    }

    private void ListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var viewmodel = this.DataContext as VpFolderViewModel;
        if(viewmodel != null)
        {
            viewmodel.DoubleClick();
        }
    }
}