using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VP.NET.GUI.Models;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<VpViewModel> WorkingFiles { get; set; } = new ObservableCollection<VpViewModel>();

        public static Settings Settings { get; } = new Settings();

        [ObservableProperty]
        public VpFolderViewModel folderViewModel = new VpFolderViewModel();

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
                Settings.Load();
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
            if(WorkingFiles.FirstOrDefault(wf => wf.VpPath == path) == null)
                WorkingFiles.Add(new VpViewModel(path));
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
                if (Settings.LastVPLoadPath != null)
                {
                    options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(Settings.LastVPLoadPath);
                }
                options.FileTypeFilter = new List<FilePickerFileType> {
                new("VP files (*.vp, *.vpc)") { Patterns = new[] { "*.vp", "*.vpc" } },
                new("All files (*.*)") { Patterns = new[] { "*" } }
            };


                var result = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(options);

                if (result != null && result.Count > 0)
                {
                    var newPath = (await result[0].GetParentAsync())?.Path.LocalPath;
                    if (Settings.LastVPLoadPath != newPath)
                    {
                        Settings.LastVPLoadPath = newPath;
                        Settings.Save();
                    }

                    foreach (var file in result)
                    {
                        try
                        {
                            AddWorkingFile(file.Path.LocalPath);
                        }
                        catch (Exception ex)
                        {
                            Log.Add(Log.LogSeverity.Error, "MainWindowViewModel.OpenFile(1)", ex);
                        }
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
                if (Settings.LastVPLoadPath != null)
                {
                    options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(Settings.LastVPLoadPath);
                }
                var result = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(options);

                if (result != null && result.Count > 0)
                {

                    string[] files = Directory.GetFiles(result[0].Path.LocalPath, "*.vp*");
                    if (Settings.LastVPLoadPath != result[0].Path.LocalPath)
                    {
                        Settings.LastVPLoadPath = result[0].Path.LocalPath;
                        Settings.Save();
                    }
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
    }
}
