using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VP.NET.GUI.Models;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class MultiToolViewModel : ViewModelBase
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private bool cancelOperation = false;
        [ObservableProperty]
        internal bool cancelVisible = false;
        [ObservableProperty]
        internal string message = string.Empty;
        [ObservableProperty]
        internal string title = string.Empty;
        [ObservableProperty]
        internal int progressMax = 1;
        [ObservableProperty]
        internal int progressCurrent = 0;
        [ObservableProperty]
        internal int progress2Max = 0;
        [ObservableProperty]
        internal int progress2Current = 0;
        [ObservableProperty]
        internal string progress2Filename = string.Empty;
        public MultiToolViewModel() 
        {
            
        }

        public async void CompressVPCs()
        {
            Title = "Compress .VP files";
            FilePickerOpenOptions options = new FilePickerOpenOptions();
            options.AllowMultiple = true;
            options.Title = "Select the .vp files you want to compress";
            if (MainWindowViewModel.settings.ToolLastVPCompressionOpenPath != null)
            {
                options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.settings.ToolLastVPCompressionOpenPath);
            }
            options.FileTypeFilter = new List<FilePickerFileType> {
                new("VP (*.vp)") { Patterns = new[] { "*.vp", "*.VP", "*.Vp" } }
            };


            var result = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(options);

            if (result != null && result.Count > 0)
            {
                var newPath = (await result[0].GetParentAsync())?.Path.LocalPath;
                if (MainWindowViewModel.settings.ToolLastVPCompressionOpenPath != newPath)
                {
                    MainWindowViewModel.settings.ToolLastVPCompressionOpenPath = newPath;
                    MainWindowViewModel.settings.Save();
                }

                var destination = await SelectDestinationFolder(MainWindowViewModel.settings.ToolLastVPCompressionDestinationPath).ConfigureAwait(false);

                if (!String.IsNullOrEmpty(destination))
                {
                    if (MainWindowViewModel.settings.ToolLastVPCompressionDestinationPath != destination)
                    {
                        MainWindowViewModel.settings.ToolLastVPCompressionDestinationPath = destination;
                        MainWindowViewModel.settings.Save();
                    }
                    Dispatcher.UIThread.Invoke(() => {
                        Message += "Destination Folder: " + destination;
                        ProgressCurrent = 0;
                        ProgressMax = result.Count();
                        CancelVisible = true;
                    });

                    await Task.Run(async () => {
                        foreach (var file in result)
                        {
                            if (!cancelOperation)
                            {
                                try
                                {
                                    var fileExtension = Path.GetExtension(file.Name).ToLower();
                                    if (fileExtension != ".vp")
                                    {
                                        Dispatcher.UIThread.Invoke(() =>
                                        {
                                            Message += "\n" + file.Name + " - Skipped (Not allowed)";
                                        });
                                    }
                                    else
                                    {
                                        Dispatcher.UIThread.Invoke(() =>
                                        {
                                            Message += "\nCompressing: " + file.Name;
                                        });
                                        Progress2Current = 0;
                                        var destFilePath = Path.Combine(destination, file.Name.Replace(".vp", ".vpc", StringComparison.InvariantCultureIgnoreCase));
                                        var vp = new VPContainer(file.Path.LocalPath);
                                        vp.EnableCompression();
                                        await vp.SaveAsAsync(destFilePath, progress2Callback, cancellationTokenSource).ConfigureAwait(false); ;
                                        Dispatcher.UIThread.Invoke(() =>
                                        {
                                            ProgressCurrent++;
                                            try
                                            {
                                                Progress2Current = Progress2Max;
                                                Progress2Filename = string.Empty;
                                                Message += " - OK ( " + Utils.FormatBytes(new FileInfo(destFilePath).Length - new FileInfo(file.Path.LocalPath).Length) + " )";
                                            }
                                            catch { }
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Add(Log.LogSeverity.Error, "MultiToolViewModel.CompressVPCs()", ex);
                                    Dispatcher.UIThread.Invoke(() => { Message += "\nError: " + ex.Message; });
                                }
                            }
                        }
                    }).ConfigureAwait(false);
                    Dispatcher.UIThread.Invoke(() => {
                        CancelVisible = false;
                        if (!cancelOperation)
                        {
                            ProgressCurrent = ProgressMax;
                            Message += "\nOperation Completed.";
                        }
                        else
                        {
                            Message += "\nOperation Cancelled.";
                        }
                    });
                }
                else
                {
                    Dispatcher.UIThread.Invoke(() => { Message = "Destination folder not selected, operation cancelled."; });
                }
            }
            else
            {
                Dispatcher.UIThread.Invoke(() => {
                    Message += "\nOperation Cancelled.";
                });
            }
        }

        public async void DecompressVPCs()
        {
            Title = "Decompress .VPC files";
            FilePickerOpenOptions options = new FilePickerOpenOptions();
            options.AllowMultiple = true;
            options.Title = "Select the .vpc files you want to decompress";
            if (MainWindowViewModel.settings.ToolLastVPDecompressionOpenPath != null)
            {
                options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.settings.ToolLastVPDecompressionOpenPath);
            }
            options.FileTypeFilter = new List<FilePickerFileType> {
                new("Compressed VP (*.vpc)") { Patterns = new[] { "*.vpc", "*.VPC", "*.Vpc" } }
            };


            var result = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(options);

            if (result != null && result.Count > 0)
            {
                var newPath = (await result[0].GetParentAsync())?.Path.LocalPath;
                if (MainWindowViewModel.settings.ToolLastVPDecompressionOpenPath != newPath)
                {
                    MainWindowViewModel.settings.ToolLastVPDecompressionOpenPath = newPath;
                    MainWindowViewModel.settings.Save();
                }

                var destination = await SelectDestinationFolder(MainWindowViewModel.settings.ToolLastVPDecompressionDestinationPath).ConfigureAwait(false);

                if (!String.IsNullOrEmpty(destination))
                {
                    if (MainWindowViewModel.settings.ToolLastVPDecompressionDestinationPath != destination)
                    {
                        MainWindowViewModel.settings.ToolLastVPDecompressionDestinationPath = destination;
                        MainWindowViewModel.settings.Save();
                    }
                    Dispatcher.UIThread.Invoke(() => {
                        Message += "Destination Folder: " + destination;
                        ProgressCurrent = 0;
                        ProgressMax = result.Count();
                        CancelVisible = true;
                    });

                    await Task.Run(async () => { 
                        foreach (var file in result)
                        {
                            if (!cancelOperation)
                            {
                                try
                                {
                                    var fileExtension = Path.GetExtension(file.Name).ToLower();
                                    if (fileExtension != ".vpc")
                                    {
                                        Dispatcher.UIThread.Invoke(() =>
                                        {
                                            Message += "\n" + file.Name + " - Skipped (Not allowed)";
                                        });
                                    }
                                    else
                                    {
                                        Dispatcher.UIThread.Invoke(() =>
                                        {
                                            Message += "\nDecompressing: " + file.Name;
                                        });
                                        Progress2Current = 0;
                                        var destFilePath = Path.Combine(destination, file.Name.Replace(".vpc", ".vp", StringComparison.InvariantCultureIgnoreCase));
                                        var vp = new VPContainer(file.Path.LocalPath);
                                        vp.DisableCompression();
                                        await vp.SaveAsAsync(destFilePath, progress2Callback, cancellationTokenSource).ConfigureAwait(false); ;
                                        Dispatcher.UIThread.Invoke(() =>
                                        {
                                            ProgressCurrent++;
                                            try
                                            {
                                                Progress2Current = Progress2Max;
                                                Message += " - OK (+ " + Utils.FormatBytes(new FileInfo(destFilePath).Length - new FileInfo(file.Path.LocalPath).Length) + " )";
                                            }
                                            catch { }
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Add(Log.LogSeverity.Error, "MultiToolViewModel.DecompressVPCs()", ex);
                                    Dispatcher.UIThread.Invoke(() => { Message += "\nError: " + ex.Message; });
                                }
                            }
                        }
                    }).ConfigureAwait(false);
                    Dispatcher.UIThread.Invoke(() => {
                        CancelVisible = false;
                        if (!cancelOperation)
                        {
                            ProgressCurrent = ProgressMax;
                            Message += "\nOperation Completed.";
                        }
                        else
                        {
                            Message += "\nOperation Cancelled.";
                        }
                    });
                }
                else
                {
                    Dispatcher.UIThread.Invoke(() => { Message = "Destination folder not selected, operation cancelled."; });
                }
            }
            else
            {
                Dispatcher.UIThread.Invoke(() => {
                    Message += "\nOperation Cancelled.";
                });
            }
        }

        public async void DecompressLZ41Files()
        {
            Title = "Decompress LZ41 files";
            FilePickerOpenOptions options = new FilePickerOpenOptions();
            options.AllowMultiple = true;
            options.Title = "Select the .lz41 files you want to decompress";
            if (MainWindowViewModel.settings.ToolLastLZ41FileDecompressionOpenPath != null)
            {
                options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.settings.ToolLastLZ41FileDecompressionOpenPath);
            }
            options.FileTypeFilter = new List<FilePickerFileType> {
                new("LZ41 CP Files (*.lz41)") { Patterns = new[] { "*.lz41", "*.LZ41", "*.lZ41", "*.Lz41" } }
            };


            var result = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(options);

            if (result != null && result.Count > 0)
            {
                var newPath = (await result[0].GetParentAsync())?.Path.LocalPath;
                if (MainWindowViewModel.settings.ToolLastLZ41FileDecompressionOpenPath != newPath)
                {
                    MainWindowViewModel.settings.ToolLastLZ41FileDecompressionOpenPath = newPath;
                    MainWindowViewModel.settings.Save();
                }

                var destination = await SelectDestinationFolder(MainWindowViewModel.settings.ToolLastLZ41FileDecompressionDestinationPath).ConfigureAwait(false);

                if(!String.IsNullOrEmpty(destination))
                {
                    if(MainWindowViewModel.settings.ToolLastLZ41FileDecompressionDestinationPath != destination )
                    {
                        MainWindowViewModel.settings.ToolLastLZ41FileDecompressionDestinationPath = destination;
                        MainWindowViewModel.settings.Save();
                    }
                    Dispatcher.UIThread.Invoke(() => { 
                        Message += "Destination Folder: " + destination;
                        ProgressCurrent = 0;
                        ProgressMax = result.Count();
                        CancelVisible = true;
                        Message += "\nDecompressing: ";
                    });

                    Parallel.ForEach(result, new ParallelOptions { MaxDegreeOfParallelism = 4 }, file => {
                        if (!cancelOperation)
                        {
                            try
                            {
                                var fileExtension = Path.GetExtension(file.Name).ToLower();
                                if (fileExtension != ".lz41")
                                {
                                    Dispatcher.UIThread.Invoke(() =>
                                    {
                                        ProgressCurrent++;
                                        Message += "\n" + file.Name + " - Skipped (Not allowed)";
                                    });
                                }
                                else
                                {
                                    var destFilePath = Path.Combine(destination, file.Name.Replace(".lz41", "", StringComparison.InvariantCultureIgnoreCase));
                                    using (var file_in = new FileStream(file.Path.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        using (var file_out = new FileStream(destFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                                        {
                                            var unCompressedSize = VPCompression.DecompressStream(file_in, file_out);
                                            Dispatcher.UIThread.Invoke(() =>
                                            {
                                                ProgressCurrent++;
                                                Message += "\n" + file.Name + " - OK (+ " + Utils.FormatBytes(unCompressedSize - file_in.Length) + " )";
                                            });
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Add(Log.LogSeverity.Error, "MultiToolViewModel.DecompressLZ41Files()", ex);
                                Dispatcher.UIThread.Invoke(() => { Message += "\nError: " + ex.Message; });
                            }
                        }
                    });
                    Dispatcher.UIThread.Invoke(() => {
                        CancelVisible = false;
                        if (!cancelOperation)
                        {
                            ProgressCurrent = ProgressMax;
                            Message += "\nOperation Completed.";
                        }
                        else
                        {
                            Message += "\nOperation Cancelled.";
                        }
                    });
                }
                else
                {
                    Dispatcher.UIThread.Invoke(() => { Message = "Destination folder not selected, operation cancelled."; });
                }
            }
            else
            {
                Dispatcher.UIThread.Invoke(() => {
                    Message += "\nOperation Cancelled.";
                });
            }
        }

        public async void CompressLZ41Files()
        {
            Title = "Compress LZ41 files";
            FilePickerOpenOptions options = new FilePickerOpenOptions();
            options.AllowMultiple = true;
            options.Title = "Select the files you want to compress as .lz41 (.vp/.vpc not allowed)";
            if (MainWindowViewModel.settings.ToolLastLZ41FileCompressionOpenPath != null)
            {
                options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(MainWindowViewModel.settings.ToolLastLZ41FileCompressionOpenPath);
            }
            options.FileTypeFilter = new List<FilePickerFileType> {
                new("Any Files (*.*)") { Patterns = new[] { "*.*" } }
            };


            var result = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(options);

            if (result != null && result.Count > 0)
            {
                var newPath = (await result[0].GetParentAsync())?.Path.LocalPath;
                if (MainWindowViewModel.settings.ToolLastLZ41FileCompressionOpenPath != newPath)
                {
                    MainWindowViewModel.settings.ToolLastLZ41FileCompressionOpenPath = newPath;
                    MainWindowViewModel.settings.Save();
                }

                var destination = await SelectDestinationFolder(MainWindowViewModel.settings.ToolLastLZ41FileCompressionDestinationPath).ConfigureAwait(false);

                if (!String.IsNullOrEmpty(destination))
                {
                    if (MainWindowViewModel.settings.ToolLastLZ41FileCompressionDestinationPath != destination)
                    {
                        MainWindowViewModel.settings.ToolLastLZ41FileCompressionDestinationPath = destination;
                        MainWindowViewModel.settings.Save();
                    }
                    Dispatcher.UIThread.Invoke(() => {
                        Message += "Destination Folder: " + destination;
                        ProgressCurrent = 0;
                        ProgressMax = result.Count();
                        CancelVisible = true;
                        Message += "\nCompressing: ";
                    });

                    Parallel.ForEach(result, new ParallelOptions { MaxDegreeOfParallelism = 4 }, file => {
                        if (!cancelOperation)
                        {
                            try
                            {
                                var fileExtension = Path.GetExtension(file.Name).ToLower();
                                if (fileExtension == ".vp" || fileExtension == ".vpc" || fileExtension == ".lz41")
                                {
                                    Dispatcher.UIThread.Invoke(() =>
                                    {
                                        ProgressCurrent++;
                                        Message += "\n" + file.Name + " - Skipped (Not allowed)";
                                    });
                                }
                                else
                                {
                                    var destFilePath = Path.Combine(destination, file.Name + ".lz41");
                                    using (var file_in = new FileStream(file.Path.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        using (var file_out = new FileStream(destFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                                        {
                                            var compressedSize = VPCompression.CompressStream(file_in, file_out);
                                            Dispatcher.UIThread.Invoke(() =>
                                            {
                                                ProgressCurrent++;
                                                Message += "\n" + file.Name + " - OK ( " + Utils.FormatBytes(compressedSize - file_in.Length) + " )";
                                            });
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Add(Log.LogSeverity.Error, "MultiToolViewModel.CompressLZ41Files()", ex);
                                Dispatcher.UIThread.Invoke(() => { Message += "\nError: " + ex.Message; });
                            }
                        }
                    });
                    Dispatcher.UIThread.Invoke(() => {
                        CancelVisible = false;
                        if (!cancelOperation)
                        {
                            ProgressCurrent = ProgressMax;
                            Message += "\nOperation Completed.";
                        }
                        else
                        {
                            Message += "\nOperation Cancelled.";
                        }
                    });
                }
                else
                {
                    Dispatcher.UIThread.Invoke(() => { Message = "Destination folder not selected, operation cancelled."; });
                }
            }
            else
            {
                Dispatcher.UIThread.Invoke(() => {
                    Message += "\nOperation Cancelled.";
                });
            }
        }

        internal async Task<string?> SelectDestinationFolder(string? startPathSuggestion)
        {
            FolderPickerOpenOptions options = new FolderPickerOpenOptions();
            options.Title = "Select the destination folder";
            if (startPathSuggestion != null)
            {
                options.SuggestedStartLocation = await MainWindow.Instance!.StorageProvider.TryGetFolderFromPathAsync(startPathSuggestion);
            }
            var result = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(options);

            if (result != null && result.Count > 0)
            {
                return result[0].Path.LocalPath;
            }

            return null;
        }

        internal void Cancel()
        {
            cancelOperation = true;
            cancellationTokenSource.Cancel();
        }
        internal void progress2Callback(string name, int max)
        {
            Dispatcher.UIThread.Invoke(() => {
                Progress2Filename = name;
                Progress2Max = max;
                Progress2Current++;
            });
        }
    }
}
