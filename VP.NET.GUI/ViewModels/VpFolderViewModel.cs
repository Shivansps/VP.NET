using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using VP.NET.GUI.Views;
using VP.NET.GUI.Models;
using System.Linq;
using System.IO;

namespace VP.NET.GUI.ViewModels
{
    public partial class VpFolderViewModel : ViewModelBase
    {
        public ObservableCollection<VpFileEntryViewModel> Items { get; set; } = new ObservableCollection<VpFileEntryViewModel>();
        public ObservableCollection<VpFileEntryViewModel> SelectedItems { get; set; } = new ObservableCollection<VpFileEntryViewModel>();

        private VPFile? vpFile { get; set; }

        [ObservableProperty]
        public string? vpFilePath = string.Empty;

        [ObservableProperty]
        public string newFolderName = string.Empty;

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
                            {
                                backdir = vm;
                            }
                            else
                            {
                                vm.IsMarkedDelete = fentry.DeleteStatus();
                                vm.IsNewFile = fentry.NewFileStatus();
                            }
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
        /// Extract all files and folders
        /// </summary>
        public async void ExtractAll()
        {
            try
            {
                FolderPickerOpenOptions options = new FolderPickerOpenOptions();
                options.AllowMultiple = false;
                options.Title = "Select the destination directory";
                if (MainWindowViewModel.settings.LastFileExtractionPath != null)
                {
                    options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.settings.LastFileExtractionPath);
                }
                var result = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(options);
                var extractVpFiles = new List<VPFile>();
                var destination = string.Empty;
                if (result != null && result.Count > 0)
                {
                    destination = result[0].TryGetLocalPath()!;
                    if (MainWindowViewModel.settings.LastFileExtractionPath != destination)
                    {
                        MainWindowViewModel.settings.LastFileExtractionPath = destination;
                        MainWindowViewModel.settings.Save();
                    }
                }

                foreach (var item in Items)
                {
                    if (item.vpFile != null)
                        extractVpFiles.Add(item.vpFile);
                }

                if (extractVpFiles.Count > 0 && destination != string.Empty)
                {
                    var dialog = new ProgressView();
                    dialog.DataContext = new ProgressViewModel(extractVpFiles, destination, dialog);
                    await dialog.ShowDialog<ProgressView?>(MainWindow.Instance!);
                }
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpFolderViewModel.ExtractAll", ex);
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
                if (MainWindowViewModel.settings.LastFileExtractionPath != null)
                {
                    options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.settings.LastFileExtractionPath);
                }
                var result = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(options);
                var extractVpFiles = new List<VPFile>();
                var destination = string.Empty;
                if (result != null && result.Count > 0)
                {
                    destination = result[0].TryGetLocalPath()!;
                    if (MainWindowViewModel.settings.LastFileExtractionPath != destination)
                    {
                        MainWindowViewModel.settings.LastFileExtractionPath = destination;
                        MainWindowViewModel.settings.Save();
                    }
                }

                foreach (var item in SelectedItems)
                {
                    if (item.vpFile != null)
                        extractVpFiles.Add(item.vpFile);
                }

                if (extractVpFiles.Count > 0 && destination != string.Empty)
                {
                    var dialog = new ProgressView();
                    dialog.DataContext = new ProgressViewModel(extractVpFiles, destination, dialog);
                    await dialog.ShowDialog<ProgressView?>(MainWindow.Instance!);
                }
            }catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpFolderViewModel.ExtractSelected", ex);
            }
        }

        internal void DeleteSelected()
        {
            bool changes = false;
            foreach (var item in SelectedItems)
            {
                if (item.vpFile != null && item.vpFile.parent != null && item.vpFile.type != VPFileType.BackDir)
                {
                    item.vpFile.Delete(true);
                    item.IsMarkedDelete = true;
                    changes = true;
                }
            }
            if (VpFilePath != null && changes)
            {
                MainWindow.LeftPanelFlagUnsavedChanges(VpFilePath);
            }
        }

        internal void CreateFolder()
        {
            if (vpFile != null && VpFilePath != null)
            {
                var exist = vpFile.files?.FirstOrDefault(x => x.info.name.ToLower() == NewFolderName.ToLower());
                if (exist == null)
                {
                    var folder = vpFile.CreateEmptyDirectory(NewFolderName);
                    folder.SetNewFile(true);
                    var vm = new VpFileEntryViewModel(folder);
                    vm.IsNewFile = true;
                    Items.Add(vm);
                    MainWindow.LeftPanelReloadItems(vpFile, VpFilePath);
                    NewFolderName = "";
                    MainWindow.LeftPanelFlagUnsavedChanges(VpFilePath);
                }
                else
                {
                    MessageBox.Show(MainWindow.Instance, "A file or folder with that name already exists", "Duplicated name", MessageBox.MessageBoxButtons.OK);
                }
            }
        }

        internal async void AddFiles()
        {
            try
            {
                FilePickerOpenOptions options = new FilePickerOpenOptions();
                options.AllowMultiple = true;
                options.Title = "Open VP files";
                if (MainWindowViewModel.settings.LastAddFilesPath != null)
                {
                    options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.settings.LastAddFilesPath);
                }
                options.FileTypeFilter = new List<FilePickerFileType> {
                new("All files (*.*)") { Patterns = new[] { "*" } }
            };


                var result = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(options);

                if (result != null && result.Count > 0)
                {
                    try
                    {
                        var newPath = Directory.GetParent(result[0].TryGetLocalPath()!)!.FullName;
                        if (MainWindowViewModel.settings.LastAddFilesPath != newPath)
                        {
                            MainWindowViewModel.settings.LastAddFilesPath = newPath;
                            MainWindowViewModel.settings.Save();
                        }
                    }
                    catch { }

                    foreach (var file in result)
                    {
                        try
                        {
                            var fullpath = file.TryGetLocalPath();
                            if(fullpath == null)
                            {
                                throw new Exception("Unable to determine file full path");
                            }
                            var ext = Path.GetExtension(fullpath);
                            if (ext != null && (ext.ToLower() == ".vp" || ext.ToLower() == ".vpc"))
                            {
                                file.Dispose();
                                continue;
                            }
                            var f = vpFile?.AddFile(new FileInfo(fullpath));
                            if (f != null && VpFilePath != null)
                            {
                                f.SetNewFile(true);
                                var vm = new VpFileEntryViewModel(f);
                                vm.IsNewFile = true;
                                Items.Add(vm);
                                MainWindow.LeftPanelFlagUnsavedChanges(VpFilePath);
                            }
                            else
                            {
                                Log.Add(Log.LogSeverity.Error, "VpFolderViewMoel.AddFiles(1)", "Addfile returned null");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Add(Log.LogSeverity.Error, "VpFolderViewMoel.AddFiles(1)", ex);
                        }
                        file.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpFolderViewMoel.AddFiles(2)", ex);
            }
        }
    }
}
