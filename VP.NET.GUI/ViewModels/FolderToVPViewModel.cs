using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VP.NET.GUI.Models;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class FolderToVPViewModel : ViewModelBase
    {
        private CancellationTokenSource? cancellationTokenSource = null;
        [ObservableProperty]
        internal int progressMax = 1;
        [ObservableProperty]
        internal int progressCurrent = 0;
        [ObservableProperty]
        internal string progressFilename = string.Empty;
        [ObservableProperty]
        internal string folderPath  = string.Empty;
        [ObservableProperty]
        internal string folderSize = string.Empty;
        [ObservableProperty]
        internal string vPPath = string.Empty;
        [ObservableProperty]
        internal bool buttonsEnabled = true;
        [ObservableProperty]
        internal bool compressVP = false;
        [ObservableProperty]
        internal bool canCreate = false;

        public FolderToVPViewModel()
        {

        }

        /// <summary>
        /// Calculate file size of a folder
        /// </summary>
        private async void GetFolderSize()
        {
            try
            {
                FolderSize = "";
                if (Directory.Exists(FolderPath))
                {
                    FolderSize = "Estimated Size: " + Utils.FormatBytes(await Utils.GetSizeOfFolderInBytes(FolderPath));
                }
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "FolderToVPViewModel.GetFolderSize()", ex);
            }
        }

        /// <summary>
        /// Starts the process of converting a folder into a VP
        /// </summary>
        public async void Start()
        {
            await Task.Run(async () => {
                cancellationTokenSource = new CancellationTokenSource();
                Dispatcher.UIThread.Invoke(() => { 
                    ButtonsEnabled = false;
                    ProgressCurrent = 0;
                    ProgressMax = 1;
                    ProgressFilename = string.Empty;
                });
                if (Directory.Exists(FolderPath))
                {
                    var vp = new VPContainer();
                    if (CompressVP)
                    {
                        vp.EnableCompression();
                        VPPath = Path.ChangeExtension(VPPath, ".vpc");
                    }
                    else
                    {
                        vp.DisableCompression();
                        VPPath = Path.ChangeExtension(VPPath, ".vp");
                    }
                    vp.AddFolderToRoot(FolderPath);
                    await vp.SaveAsAsync(VPPath, progressCallback, cancellationTokenSource).ConfigureAwait(false);
                    if(File.Exists(VPPath))
                    {
                        FolderSize = "Final Size: " + Utils.FormatBytes(new FileInfo(VPPath).Length);
                    }
                }
                Dispatcher.UIThread.Invoke(() => {
                    ButtonsEnabled = true;
                    cancellationTokenSource = null;
                });
            }).ConfigureAwait(false);
        }

        internal void Cancel()
        {
            cancellationTokenSource?.Cancel();
        }

        internal void progressCallback(string name, int max)
        {
            Dispatcher.UIThread.Invoke(() => {
                ProgressFilename = "_"+name;//visual hack
                ProgressMax = max;
                ProgressCurrent++;
            });
        }

        public async void BrowseFolder()
        {
            FolderPickerOpenOptions options = new FolderPickerOpenOptions();
            options.Title = "Select the source folder";
            if (MainWindowViewModel.settings.ToolLastFolderToVPFolderPath != null)
            {
                options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.settings.ToolLastFolderToVPFolderPath);
            }
            var result = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(options);

            if (result != null && result.Count > 0)
            {
                try
                {
                    var newPath = (await result[0].GetParentAsync())?.TryGetLocalPath();
                    if (MainWindowViewModel.settings.ToolLastFolderToVPFolderPath != newPath)
                    {
                        MainWindowViewModel.settings.ToolLastFolderToVPFolderPath = newPath;
                        MainWindowViewModel.settings.Save();
                    }
                }
                catch { }
                FolderPath = result[0].Path.LocalPath;
                CanCreate = FolderPath.Trim().Length > 0 && VPPath.Trim().Length > 0;
                GetFolderSize();
            }
        }

        public async void BrowseFile()
        {
            FilePickerSaveOptions options = new FilePickerSaveOptions();
            options.Title = "Destination VP name";
            if (MainWindowViewModel.settings.ToolLastFolderToVPVPSavePath != null)
            {
                options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.settings.ToolLastFolderToVPVPSavePath);
                options.DefaultExtension = ".vp";
                options.FileTypeChoices = new List<FilePickerFileType> { new("VP (*.vp)") { Patterns = new[] { "*.vp" } } };
            }
            var result = await MainWindow.Instance!.StorageProvider.SaveFilePickerAsync(options);

            if (result != null)
            {
                var newPath = (await result.GetParentAsync())?.TryGetLocalPath();
                try
                {
                    if (MainWindowViewModel.settings.ToolLastFolderToVPVPSavePath != newPath)
                    {
                        MainWindowViewModel.settings.ToolLastFolderToVPVPSavePath = newPath;
                        MainWindowViewModel.settings.Save();
                    }
                }
                catch { }
                VPPath = result.Path.LocalPath;
                VPPath = Path.ChangeExtension(VPPath, ".vp");
                CanCreate = FolderPath.Trim().Length > 0 && VPPath.Trim().Length > 0;
            }
        }
    }
}
