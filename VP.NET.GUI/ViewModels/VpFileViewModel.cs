using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VP.NET.GUI.ViewModels
{
    public class VpFileViewModel : ObservableObject
    {
        public VPFile? file;
        public string name
        {
            get
            {
                return file.info.name;
            }
        }

        public List<VpFileViewModel>? files
        {
            get
            {
                if (file.files != null)
                {
                    var l = new List<VpFileViewModel>();
                    foreach (var item in file.files)
                    {
                        if(item.type == VPFileType.Directory)
                        l.Add(new VpFileViewModel(item));
                    }
                    return l;
                }
                return null;
            }
        }

        public VpFileViewModel()
        {

        }

        public VpFileViewModel(VPFile file)
        {
            this.file = file;
        }
    }
}
