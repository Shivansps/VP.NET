using Avalonia.Controls;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using VP.NET.GUI.ViewModels;

namespace VP.NET.GUI.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public static List<VpView> VPViewList { get; } = new List<VpView>();

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
        }

        /// <summary>
        /// Remove any VPView still in the list that was removed from view
        /// The datacontext is changed to null when this happen
        /// </summary>
        public static void CleanRemovedVpFromList()
        {
            foreach(var vp in VPViewList.ToList())
            {
                if (vp.DataContext == null)
                    VPViewList.Remove(vp);
            }
        }

        public static void LeftPanelSelectItem(VPFile vPFile, string vpPath)
        {
            foreach (var vp in VPViewList)
            {
                var vm = (VpViewModel?)vp.DataContext;
                if(vm != null && vm.VpPath != null && vm.VpPath == vpPath)
                {
                    vp.SelectItem(vPFile);
                }
            }
        }
    }
}