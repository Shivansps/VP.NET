using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using VP.NET.GUI.Views;
using VP.NET.GUI.Models;

namespace VP.NET.GUI.ViewModels
{
    public partial class VpFolderViewModel : ViewModelBase
    {
        public ObservableCollection<VpFileEntryViewModel> Items { get; set; } = new ObservableCollection<VpFileEntryViewModel>();
        public ObservableCollection<VpFileEntryViewModel> SelectedItems { get; set; } = new ObservableCollection<VpFileEntryViewModel>();

        private VPFile? vpFile { get; set; }

        [ObservableProperty]
        public string? vpFilePath = string.Empty;

        /// <summary>
        /// Reset to data in view to the default
        /// </summary>
        public void ResetView()
        {
            Items.Clear();
            SelectedItems.Clear();
            vpFile = null;
            VpFilePath = string.Empty;
        }

        /// <summary>
        /// Create a VPFolderView from a VPFile to display in UI (on the right side)
        /// </summary>
        /// <param name="vpFile"></param>
        public void LoadVpFolder(VPFile? vpFile, string vpPath)
        {
            if (this.vpFile == null || this.vpFile != vpFile)
            {
                Items.Clear();
                VpFilePath = vpPath;
                if (vpFile != null && vpFile.files != null)
                {

                    foreach (VPFile fentry in vpFile.files)
                    {
                        //Do not display backdirs in folder view
                        if(fentry.type != VPFileType.BackDir)
                            Items.Add(new VpFileEntryViewModel(fentry));
                    }
                }

                this.vpFile = vpFile;
            }
        }

        /// <summary>
        /// Extract selected file entries
        /// </summary>
        internal async void ExtractSelected()
        {
            if (SelectedItems.Count == 0)
                return;
            FolderPickerOpenOptions options = new FolderPickerOpenOptions();
            options.AllowMultiple = false;
            options.Title = "Select the destination directory";
            if (MainWindowViewModel.Settings.LastFileExtractionPath != null)
            {
                options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.Settings.LastFileExtractionPath);
            }
            var result = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(options);
            var extractVpFiles = new List<VPFile>();
            var destination = string.Empty;
            if (result != null && result.Count > 0)
            {
                destination = result[0].Path.LocalPath;
                if (MainWindowViewModel.Settings.LastFileExtractionPath != destination)
                {
                    MainWindowViewModel.Settings.LastFileExtractionPath = destination;
                    MainWindowViewModel.Settings.Save();
                }
            }

            foreach (var item in SelectedItems)
            {
                if (item.vpFile != null)
                    extractVpFiles.Add(item.vpFile);
            }

            if (extractVpFiles.Count > 0 && destination != string.Empty)
            {
                var dialog = new ExtractView();
                dialog.DataContext = new ExtractViewModel(extractVpFiles, destination, dialog);
                await dialog.ShowDialog<ExtractView?>(MainWindow.Instance!);
            }
        }
    }
}
