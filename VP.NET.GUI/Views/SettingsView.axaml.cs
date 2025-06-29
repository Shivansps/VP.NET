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
            MainWindowViewModel.settings.Save();
        }
        catch(Exception ex)
        {
            Log.Add(Log.LogSeverity.Error, "SettingsView.Window_Closing", ex);
        }
    }
}