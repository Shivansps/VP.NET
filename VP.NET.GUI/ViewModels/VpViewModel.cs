using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public bool unsavedChanges = false;

        [ObservableProperty]
        public ObservableCollection<VpViewModel> files = new ObservableCollection<VpViewModel>();

        [ObservableProperty]
        public string? name = string.Empty;

        [ObservableProperty]
        public string numberOfFiles = string.Empty;

        [ObservableProperty]
        public string numberOfFolders = string.Empty;

        [ObservableProperty]
        public bool compression = false;

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
                Compression = VpContainer.compression;
                NumberOfFiles = VpContainer.numberFiles.ToString();
                NumberOfFolders = VpContainer.numberFolders.ToString();
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

        public void ReloadUIItems()
        {
            if (VpFile == null || VpFile.files == null || VpPath == null)
                return;
            Files.Clear();
            foreach (var file in VpFile.files)
            {
                if (file.type == VPFileType.Directory)
                    Files.Add(new VpViewModel(file, VpPath));
            }
        }

        public void ShowFolder()
        {
            MainWindowViewModel.Instance!.FolderViewModel.LoadVpFolder(VpFile, VpPath!);
        }

        internal async void RemoveFile()
        {
            try
            {
                if(UnsavedChanges)
                {
                    var res = await MessageBox.Show(MainWindow.Instance, "Close file without saving?", "Confirm", MessageBox.MessageBoxButtons.YesCancel);
                    if (res == MessageBox.MessageBoxResult.Cancel)
                        return;
                }
                MainWindowViewModel.Instance!.RemoveFile(this);
                MainWindow.CleanRemovedVpFromList();
                MainWindowViewModel.Instance?.PrevViewModel?.CloseIfPath(VpContainer?.vpFilePath);
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpViewModel.RemoveFile()", ex);
            }
        }

        public async Task SaveFile()
        {
            if (!UnsavedChanges)
                return;
            if (VpContainer != null && VpPath != null)
            {
                try
                {
                    MainWindowViewModel.Instance?.PrevViewModel?.CloseIfPath(VpContainer.vpFilePath);

                    var dialog = new ProgressView();
                    dialog.DataContext = new ProgressViewModel(VpContainer, Name, dialog);
                    await dialog.ShowDialog<ProgressView?>(MainWindow.Instance!);

                    MarkAsUnsavedChanges(false);
                    MainWindowViewModel.Instance!.UpdateWorkingFile(VpPath);
                    MainWindow.CleanRemovedVpFromList();
                }
                catch (Exception ex)
                {
                    Log.Add(Log.LogSeverity.Error, "VpViewModel.SaveFile()", ex);
                }
            }
        }

        internal void ReloadFile()
        {
            if (VpContainer != null && VpPath != null)
            {
                try
                {
                    MarkAsUnsavedChanges(false);
                    MainWindowViewModel.Instance!.UpdateWorkingFile(VpPath);
                }
                catch (Exception ex)
                {
                    Log.Add(Log.LogSeverity.Error, "VpViewModel.ReloadFile()", ex);
                }
            }
        }

        public void MarkAsUnsavedChanges(bool value = true)
        {
            UnsavedChanges = value;
        }

        public void UpdateCompressionStatus(bool value)
        {
            if(VpContainer != null && value != VpContainer.compression)
            {
                UnsavedChanges = true;
            }
            if (value)
            {
                VpContainer?.EnableCompression();
            }
            else
            {
                VpContainer?.DisableCompression();
            }
        }
    }
}
