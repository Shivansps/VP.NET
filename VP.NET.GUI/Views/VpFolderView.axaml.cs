using Avalonia.Controls;
using System;
using VP.NET.GUI.Models;
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

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        try
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var item = listBox.SelectedItem as VpFileEntryViewModel;
                var dt = this.DataContext as VpFolderViewModel;
                if (item != null && dt != null && dt.VpFilePath != null)
                {
                    MainWindowViewModel.Instance?.PrevViewModel?.StartPreview(item, dt.VpFilePath);                
                }
                else
                {
                    throw new Exception("Listbox.SelectedItem was null");
                }
            }
            else
            {
                throw new Exception("Listbox was null");
            }
        }
        catch (Exception ex) 
        {
            Log.Add(Log.LogSeverity.Error, "VpFolderView.ListBox_SelectionChanged", ex);
        }
    }
}