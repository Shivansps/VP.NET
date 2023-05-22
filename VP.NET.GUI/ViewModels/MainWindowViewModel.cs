using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VP.NET.GUI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<VpContainerViewModel> workingFiles = new ObservableCollection<VpContainerViewModel>();

        public void Open()
        {
            WorkingFiles.Add(new VpContainerViewModel(new VPContainer("D:\\root_fs2.vp")));
            WorkingFiles.Add(new VpContainerViewModel(new VPContainer("D:\\root_fs2.vp")));
        }

        public void Save()
        {
            WorkingFiles[0].files[0].file.AddDirectoryRecursive("D:\\extract\\data");
            WorkingFiles[0] = WorkingFiles[0];
        }
    }
}
