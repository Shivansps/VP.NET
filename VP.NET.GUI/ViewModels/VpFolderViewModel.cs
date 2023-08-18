using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VP.NET.GUI.ViewModels
{
    public partial class VpFolderViewModel : ViewModelBase
    {
        public ObservableCollection<VpFileEntryViewModel> Items { get; set; } = new ObservableCollection<VpFileEntryViewModel>();

        private VPFile? vpFile { get; set; }

        public void LoadVpFolder(VPFile? vpFile)
        {
            if (this.vpFile == null || this.vpFile != vpFile)
            {
                Items.Clear();

                if (vpFile != null && vpFile.files != null)
                {

                    foreach (VPFile fentry in vpFile.files)
                    {
                        Items.Add(new VpFileEntryViewModel(fentry));
                    }
                }

                this.vpFile = vpFile;
                //GC.Collect();
            }
        }
    }
}
