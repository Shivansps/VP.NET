using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VP.NET.GUI.Models;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class VpViewModel : ViewModelBase
    {
        private VPContainer? VpContainer = null;
        public VPFile? VpFile = null;

        [ObservableProperty]
        public string? vpPath = null;

        [ObservableProperty]
        public ObservableCollection<VpViewModel> files = new ObservableCollection<VpViewModel>();

        [ObservableProperty]
        public string? name = string.Empty;

        public VpViewModel()
        {
        }

        public VpViewModel(string path)
        {
            try
            {
                VpPath = path;
                VpContainer = new VPContainer(path);
                Name = Path.GetFileName(VpContainer.vpFilePath);
                if (VpContainer.vpFiles.Any())
                {
                    foreach (var file in VpContainer.vpFiles)
                    {
                        files.Add(new VpViewModel(file, path));
                    }
                }
            }catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpViewModel", ex);
            }
        }

        public VpViewModel(VPFile vpFile, string path)
        {
            try
            {
                Name = vpFile.info.name;
                VpFile = vpFile;
                VpPath = path;
                if (vpFile.files != null && vpFile.type == VPFileType.Directory)
                {
                    foreach (var file in vpFile.files)
                    {
                        if (file.type == VPFileType.Directory)
                            files.Add(new VpViewModel(file, path));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpViewModel(2)", ex);
            }
        }

        public void ShowFolder()
        {
            MainWindowViewModel.Instance!.FolderViewModel.LoadVpFolder(VpFile, VpPath!);
        }

        internal void RemoveFile()
        {
            try
            {
                MainWindowViewModel.Instance!.RemoveFile(this);
                MainWindow.CleanRemovedVpFromList();
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpViewModel.RemoveFile()", ex);
            }
        }
    }
}
