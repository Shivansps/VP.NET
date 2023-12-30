using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using VP.NET.GUI.Views;
using VP.NET.GUI.Models;
using System.Linq;

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
        /// Double click function for the listbox, when the user double clicks anywhere on the list object this function is called
        /// </summary>
        public void DoubleClick()
        {
            try
            {
                //Folder - Backdir navigation
                if (SelectedItems.Count == 1)
                {
                    if (SelectedItems[0] != null && VpFilePath != null && SelectedItems[0].vpFile != null)
                    {
                        if (SelectedItems[0].vpFile!.type == VPFileType.Directory)
                        {
                            if (SelectedItems[0].vpFile != null && VpFilePath != null)
                            {
                                MainWindow.LeftPanelSelectItem(SelectedItems[0].vpFile!, VpFilePath); //Right panel navigation update selection on the left panel
                                LoadVpFolder(SelectedItems[0].vpFile, VpFilePath!);
                            }

                        }
                        else if (SelectedItems[0].vpFile!.type == VPFileType.BackDir && vpFile != null && vpFile.parent != null)
                        {
                            if (vpFile.parent != null && VpFilePath != null)
                            {
                                MainWindow.LeftPanelSelectItem(vpFile.parent, VpFilePath); //Right panel navigation update selection on the left panel
                                LoadVpFolder(vpFile.parent, VpFilePath);
                            }
                        }
                    }
                }
            }catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpFolderViewModel.DoubleClick", ex);
            }
        }

        /// <summary>
        /// Create a VPFolderView from a VPFile to display in UI (on the right side)
        /// </summary>
        /// <param name="vpFile"></param>
        public void LoadVpFolder(VPFile? vpFile, string vpPath)
        {
            try
            {
                if (this.vpFile == null || this.vpFile != vpFile)
                {
                    Items.Clear();
                    VpFilePath = vpPath;
                    if (vpFile != null && vpFile.files != null)
                    {
                        VpFileEntryViewModel? backdir = null;
                        foreach (VPFile fentry in vpFile.files)
                        {
                            var vm = new VpFileEntryViewModel(fentry);
                            Items.Add(vm);
                            if (fentry.type == VPFileType.BackDir)
                                backdir = vm;
                        }
                        //Move backdir to the top
                        if (backdir != null && Items.IndexOf(backdir) != -1)
                        {
                            Items.Move(Items.IndexOf(backdir), 0);
                        }
                    }
                    this.vpFile = vpFile;
                }
            }catch(Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpFolderViewModel.LoadVpFolder", ex);
            }
        }

        /// <summary>
        /// Extract selected file entries
        /// </summary>
        internal async void ExtractSelected()
        {
            try
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
            }catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpFolderViewModel.ExtractSelected", ex);
            }
        }
    }
}
