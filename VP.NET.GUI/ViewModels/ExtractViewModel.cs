using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class ExtractViewModel : ViewModelBase
    {
        private ExtractView? extractView;
        private List<VPFile>? extractVpFiles;

        [ObservableProperty]
        string currentFileName = string.Empty;

        [ObservableProperty]
        int maxFiles = 0;

        [ObservableProperty]
        int currentFile = 0;

        bool cancelExtraction = false;

        public ExtractViewModel() { }

        public ExtractViewModel(List<VPFile> extractVpFiles, string destination, ExtractView dialog)
        {
            this.extractVpFiles = extractVpFiles;
            this.extractView = dialog;
            _ = Task.Factory.StartNew(async () => {
                //Get number of files to extract
                foreach(var file in extractVpFiles)
                {
                    MaxFiles += file.GetNumberOfFiles();
                }
                foreach (var file in extractVpFiles)
                {
                    if(!cancelExtraction)
                        await file.ExtractRecursiveAsync(destination, progressCallback);
                }
                Dispatcher.UIThread.Invoke(() => { extractView?.Close(); });
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
        }
    }
}
