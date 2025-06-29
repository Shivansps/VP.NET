using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VP.NET.GUI.Models;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class ProgressViewModel : ViewModelBase
    {
        private ProgressView? extractView;
        private List<VPFile>? extractVpFiles;

        [ObservableProperty]
        internal string currentFileName = string.Empty;

        [ObservableProperty]
        internal string title = string.Empty;

        [ObservableProperty]
        int maxFiles = 0;

        [ObservableProperty]
        int currentFile = 0;

        bool cancelExtraction = false;

        CancellationTokenSource? cancelSource;

        public ProgressViewModel() { }

        /// <summary>
        /// Extract files from the vp with a Progress bar and cancel button
        /// </summary>
        /// <param name="extractVpFiles"></param>
        /// <param name="destination"></param>
        /// <param name="dialog"></param>
        public ProgressViewModel(List<VPFile> extractVpFiles, string destination, ProgressView dialog)
        {
            this.extractVpFiles = extractVpFiles;
            this.extractView = dialog;

            if (MainWindowViewModel.Instance != null)
            {
                MainWindowViewModel.Instance.DisableInput = true;
            }

            Title = "Extracting Files...";
            _ = Task.Factory.StartNew((Func<Task>)(async () => {
                //Get number of files to extract
                try
                {
                    foreach (var file in extractVpFiles)
                    {
                        this.MaxFiles += file.GetNumberOfFiles();
                    }
                    foreach (var file in extractVpFiles)
                    {
                        if (!cancelExtraction)
                            await file.ExtractRecursiveAsync(destination, progressCallback);
                    }
                } catch (Exception ex)
                {
                    Log.Add(Log.LogSeverity.Error, "ProgressViewModel", ex);
                }
                Dispatcher.UIThread.Invoke(() => { extractView?.Close(); });
                if (MainWindowViewModel.Instance != null)
                {
                    Dispatcher.UIThread.Invoke(() => { MainWindowViewModel.Instance.DisableInput = false; });
                }
            }));
        }

        /// <summary>
        /// Save VP files
        /// </summary>
        /// <param name="vPContainer"></param>
        /// <param name="name"></param>
        /// <param name="dialog"></param>
        public ProgressViewModel(VPContainer vPContainer, string? name, ProgressView dialog)
        {
            this.extractView = dialog;
            this.cancelSource = new CancellationTokenSource();

            if (MainWindowViewModel.Instance != null)
            {
                MainWindowViewModel.Instance.DisableInput = true;
            }

            Title = "Saving " + name;
            _ = Task.Factory.StartNew((Func<Task>)(async () => {
                try
                {
                    await vPContainer.SaveAsync(saveProgressCallback, cancelSource);
                }
                catch (Exception ex)
                {
                    Log.Add(Log.LogSeverity.Error, "ProgressViewModel.SaveProgress()", ex);
                }
                Dispatcher.UIThread.Invoke(() => { extractView?.Close(); });
                if (MainWindowViewModel.Instance != null)
                {
                    Dispatcher.UIThread.Invoke(() => { MainWindowViewModel.Instance.DisableInput = false; });
                }
            }));
        }

        internal void saveProgressCallback(string name, int numberOfFiles)
        {
            Dispatcher.UIThread.Invoke(() => {
                CurrentFileName = name;
                CurrentFile ++;
                MaxFiles = numberOfFiles;
            });
        }

        internal void progressCallback(string name, int increase, int _)
        {
            Dispatcher.UIThread.Invoke(() => {
                CurrentFileName = name;
                CurrentFile += increase;
            });
        }

        internal void Cancel()
        {
            cancelExtraction = true;
            cancelSource?.Cancel();
            if (MainWindowViewModel.Instance != null)
            {
                Dispatcher.UIThread.Invoke(() => { MainWindowViewModel.Instance.DisableInput = false; });
            }
        }
    }
}
