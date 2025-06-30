using Avalonia.Controls;
using System;
using VP.NET.GUI.Models;
using VP.NET.GUI.ViewModels;

namespace VP.NET.GUI.Views;

public partial class SettingsView : Window
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        try
        {
            var dt = this.DataContext as SettingsViewModel;
            dt!.Save();
        }
        catch(Exception ex)
        {
            Log.Add(Log.LogSeverity.Error, "SettingsView.Window_Closing", ex);
        }
    }

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var dt = this.DataContext as SettingsViewModel;
        var listBox = sender as ListBox;
        var item = listBox?.SelectedItem as ExternalPreviewApp;
        if (item != null && dt != null)
        {
            dt.LoadEdit(item);
        }
    }
}