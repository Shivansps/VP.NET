using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VP.NET.GUI.Models;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<VpViewModel> WorkingFiles { get; set; } = new ObservableCollection<VpViewModel>();

        public static Settings settings { get; } = new Settings();

        [ObservableProperty]
        public VpFolderViewModel folderViewModel = new VpFolderViewModel();

        [ObservableProperty]
        public PreviewerViewModel? prevViewModel;

        [ObservableProperty]
        public bool prevViewVisible = false;

        [ObservableProperty]
        public bool saveAllEnabled = false;

        [ObservableProperty]
        public bool disableInput = false;

        public static MainWindowViewModel? Instance;

        public MainWindowViewModel()
        {
            Instance = this;
            try
            {
                if (!Directory.Exists(Utils.GetDataFolderPath()))
                {
                    Directory.CreateDirectory(Utils.GetDataFolderPath());
                }
                settings.Load();
                //Delete cache folder
                if(Directory.Exists(Utils.GetCacheFolderPath()))
                    Directory.Delete(Utils.GetCacheFolderPath(), true);
            }catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "MainWindowViewModel", ex);
            }
        }

        /// <summary>
        /// Add a new working file to the list of working files
        /// If the file is already open it will be skipped
        /// </summary>
        /// <param name="path"></param>
        private void AddWorkingFile(string path)
        {
            if (WorkingFiles.FirstOrDefault(wf => wf.VpPath == path) == null)
            {
                WorkingFiles.Add(new VpViewModel(path));
                SaveAllEnabled = true;
            }
        }

        /// <summary>
        /// Update a open file after saving
        /// It closes the file and re opens it and inserts it into the old tree index
        /// </summary>
        /// <param name="path"></param>
        public void UpdateWorkingFile(string path)
        {
            var old = WorkingFiles.FirstOrDefault(wf => wf.VpPath == path);
            if (old != null)
            {
                var index = WorkingFiles.IndexOf(old);
                RemoveFile(old);
                try
                {
                    if (index != -1)
                    {
                        WorkingFiles.Insert(index, new VpViewModel(path));
                    }
                }
                catch
                {
                    WorkingFiles.Add(new VpViewModel(path));
                }
            }
        }

        /// <summary>
        /// Open one or multiple vp files into the working file list
        /// </summary>
        internal async void OpenFile()
        {
            try
            {
                FilePickerOpenOptions options = new FilePickerOpenOptions();
                options.AllowMultiple = true;
                options.Title = "Open VP files";
                if (settings.LastVPLoadPath != null)
                {
                    options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(settings.LastVPLoadPath);
                }
                options.FileTypeFilter = new List<FilePickerFileType> {
                new("VP files (*.vp, *.vpc)") { Patterns = new[] { "*.vp", "*.vpc" } },
                new("All files (*.*)") { Patterns = new[] { "*" } }
            };


                var result = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(options);

                if (result != null && result.Count > 0)
                {
                    try
                    {
                        var newPath = Directory.GetParent(result[0].TryGetLocalPath()!)!.FullName; 
                        if (settings.LastVPLoadPath != newPath)
                        {
                            settings.LastVPLoadPath = newPath;
                            settings.Save();
                        }
                    }
                    catch { }

                    if (PrevViewModel == null && settings.PreviewerEnabled)
                        PrevViewModel = new PreviewerViewModel(); //start previewer
                    if(settings.PreviewerEnabled)
                        PrevViewVisible = true;
                    foreach (var file in result)
                    {
                        try
                        {
                            if (file.TryGetLocalPath() == null)
                                throw new Exception("Unable to determine file full path");
                            AddWorkingFile(file.TryGetLocalPath()!);
                        }
                        catch (Exception ex)
                        {
                            Log.Add(Log.LogSeverity.Error, "MainWindowViewModel.OpenFile(1)", ex);
                        }
                        file.Dispose();
                    }
                }
            }catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "MainWindowViewModel.OpenFile(2)", ex);
            }
        }

        /// <summary>
        /// Open al VPs in a folder into the working files list
        /// </summary>
        internal async void OpenFolder()
        {
            try
            {
                FolderPickerOpenOptions options = new FolderPickerOpenOptions();
                options.Title = "Open a folder containing VP files";
                if (settings.LastVPLoadPath != null)
                {
                    options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(settings.LastVPLoadPath);
                }
                var result = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(options);

                if (result != null && result.Count > 0)
                {
                    string[] files = Directory.GetFiles(result[0].TryGetLocalPath()!, "*.vp*");
                    try
                    {
                        if (settings.LastVPLoadPath != result[0].TryGetLocalPath())
                        {
                            settings.LastVPLoadPath = result[0].TryGetLocalPath();
                            settings.Save();
                        }
                    }
                    catch { }
                    if (PrevViewModel == null && settings.PreviewerEnabled)
                        PrevViewModel = new PreviewerViewModel(); //start previewer
                    if (settings.PreviewerEnabled)
                        PrevViewVisible = true;
                    foreach (var file in files)
                    {
                        try
                        {
                            AddWorkingFile(file);
                        }
                        catch (Exception ex)
                        {
                            Log.Add(Log.LogSeverity.Error, "MainWindowViewModel.OpenFolder(1)", ex);
                        }
                    }
                }
            } catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "MainWindowViewModel.OpenFolder(2)", ex);
            }
        }

        /// <summary>
        /// Remove a file from the working file list
        /// </summary>
        /// <param name="file"></param>
        internal void RemoveFile(VpViewModel file)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                try
                {
                    //We are closing the file it is currently open in view?
                    if (FolderViewModel.VpFilePath == file.VpPath)
                    {
                        FolderViewModel.ResetView();
                    }
                    WorkingFiles.Remove(file);
                    if(!WorkingFiles.Any())
                        SaveAllEnabled = false;
                }catch (Exception ex)
                {
                    Log.Add(Log.LogSeverity.Error, "MainWindowViewModel.RemoveFile()", ex);
                }
            });
        }

        internal void DecompressLooseFiles()
        {
            var dialog = new MultiToolView();
            var dataContext = new MultiToolViewModel();
            dialog.DataContext = dataContext;
            _ = dialog.ShowDialog<MultiToolView?>(MainWindow.Instance!);
            dataContext.DecompressLZ41Files();
        }

        internal void DecompressVPs()
        {
            var dialog = new MultiToolView();
            var dataContext = new MultiToolViewModel();
            dialog.DataContext = dataContext;
            _ = dialog.ShowDialog<MultiToolView?>(MainWindow.Instance!);
            dataContext.DecompressVPCs();
        }

        internal void CompressLooseFiles()
        {
            var dialog = new MultiToolView();
            var dataContext = new MultiToolViewModel();
            dialog.DataContext = dataContext;
            _ = dialog.ShowDialog<MultiToolView?>(MainWindow.Instance!);
            dataContext.CompressLZ41Files();
        }

        internal void CompressVPs()
        {
            var dialog = new MultiToolView();
            var dataContext = new MultiToolViewModel();
            dialog.DataContext = dataContext;
            _ = dialog.ShowDialog<MultiToolView?>(MainWindow.Instance!);
            dataContext.CompressVPCs();
        }

        internal void FolderToVP()
        {
            var dialog = new FolderToVPView();
            var dataContext = new FolderToVPViewModel();
            dialog.DataContext = dataContext;
            _ = dialog.ShowDialog<FolderToVPView?>(MainWindow.Instance!);
        }

        internal void OpenSettings()
        {
            var dialog = new SettingsView();
            var dataContext = new SettingsViewModel();
            dialog.DataContext = dataContext;
            _ = dialog.ShowDialog<SettingsView?>(MainWindow.Instance!);
        }

        internal async void NewFile()
        {
            try
            {
                FilePickerSaveOptions options = new FilePickerSaveOptions();
                options.Title = "Choose the location for new file";
                if (settings.LastVPLoadPath != null)
                {
                    options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(settings.LastVPLoadPath);
                }
                options.FileTypeChoices = new List<FilePickerFileType> {
                new("VP file (*.vp)") { Patterns = new[] { "*.vp" } },
                new("VPC file (*.vpc)") { Patterns = new[] { "*.vpc" } }
            };


                var result = await MainWindow.Instance!.StorageProvider.SaveFilePickerAsync(options);

                if (result != null)
                {
                    try
                    {
                        var newPath = (await result.GetParentAsync())?.Path.LocalPath;
                        if (settings.LastVPLoadPath != newPath)
                        {
                            settings.LastVPLoadPath = newPath;
                            settings.Save();
                        }
                    }
                    catch { }
                    try
                    {
                        var vp = new VPContainer();
                        var ext = Path.GetExtension(result.Name);
                        if (ext != null && ext.ToLower() == ".vpc")
                        {
                            vp.EnableCompression();
                        }
                        await vp.SaveAsAsync(result.Path.LocalPath);

                        if (PrevViewModel == null && settings.PreviewerEnabled)
                            PrevViewModel = new PreviewerViewModel(); //start previewer
                        if (settings.PreviewerEnabled)
                            PrevViewVisible = true;
                        AddWorkingFile(result.Path.LocalPath);
                    }
                    catch (Exception ex)
                    {
                        Log.Add(Log.LogSeverity.Error, "MainWindowViewModel.NewFile(1)", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "MainWindowViewModel.NewFile(2)", ex);
            }
        }

        internal async Task SaveAll()
        {
            foreach(var file in WorkingFiles.ToList())
            {
                await file.SaveFile();
            }
        }
    }
}
