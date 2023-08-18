using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Remote.Protocol.Input;
using Avalonia.VisualTree;
using System;
using System.Linq;
using VP.NET.GUI.ViewModels;

namespace VP.NET.GUI.Views;

public partial class VpView : UserControl
{
    public VpView()
    {
        InitializeComponent();
        var vpTree = this.FindControl<TreeView>("VPTree");
        if(vpTree != null)
        {
            vpTree.AddHandler(Gestures.TappedEvent, VpTree_PointerPressed);
        }
    }

    private void VpTree_PointerPressed(object? sender, RoutedEventArgs e)
    {
        try
        {
            var item = ((Visual)e.Source!).GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();

            if (item != null)
            {
                var data = (VpViewModel?)item.DataContext;
                data?.ShowFolder();
                e.Handled = true;
            }
        }catch(Exception ex)
        {

        }
    }
}