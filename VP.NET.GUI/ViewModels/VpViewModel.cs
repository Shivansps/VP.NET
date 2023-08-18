using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class VpViewModel : ViewModelBase
    {
        private VPContainer? VpContainer = null;
        private VPFile? VpFile = null;

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
            VpPath = path;
            VpContainer = new VPContainer(path);
            Name = VpContainer.vpFilePath;
            if (VpContainer.vpFiles.Any())
            {
                foreach (var file in VpContainer.vpFiles)
                {
                    files.Add(new VpViewModel(file));
                }
            }
        }

        public VpViewModel(VPFile vpFile)
        {
            try
            {
                Name = vpFile.info.name;
                VpFile = vpFile;
                if (vpFile.files != null && vpFile.type == VPFileType.Directory)
                {
                    foreach (var file in vpFile.files)
                    {
                        if (file.type == VPFileType.Directory)
                            files.Add(new VpViewModel(file));
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void ShowFolder()
        {
            MainWindowViewModel.Instance!.FolderViewModel.LoadVpFolder(VpFile);
        }
    }
}
