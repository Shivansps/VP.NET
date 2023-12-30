using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        MainWindow.VPViewList.Add(this);
    }

    public void DeselectAll()
    {
        var vpTree = this.FindControl<TreeView>("VPTree");
        if(vpTree != null && vpTree.ItemsSource != null)
        {
            var items = vpTree.GetSelfAndVisualDescendants().OfType<TreeViewItem>();
            foreach (TreeViewItem item in items)
            {
                item.IsSelected = false;
            }
        }
    }

    public void SelectItem(VPFile vpFile)
    {
        var vpTree = this.FindControl<TreeView>("VPTree");
        if (vpTree != null && vpTree.ItemsSource != null)
        {
            var foundElements = vpTree.GetSelfAndVisualDescendants().OfType<TreeViewItem>();
            if (foundElements != null)
            {
                foreach (TreeViewItem item in foundElements)
                {
                    var vm = (VpViewModel?)item.DataContext;
                    if(vm != null && vm.VpFile == vpFile)
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                }
            }
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

            foreach(VpView vp in MainWindow.VPViewList)
            {
                if(vp != null && vp != this)
                {
                    vp.DeselectAll();
                }
            }
        }
        catch(Exception ex)
        {

        }
    }
}