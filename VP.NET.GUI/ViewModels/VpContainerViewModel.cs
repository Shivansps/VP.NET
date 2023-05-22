using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VP.NET.GUI.ViewModels
{
    public class VpContainerViewModel : ObservableObject
    {
        private VPContainer container;

        public string? path
        {
            get
            {
                return container.vpFilePath;
            }
            set
            {
                container.vpFilePath = value;
            }
        }

        public List<VpFileViewModel>? files
        {
            get
            {
                if(container.vpFiles != null)
                {
                    var l = new List<VpFileViewModel>();
                    foreach(var item in container.vpFiles)
                    {
                        l.Add(new VpFileViewModel(item));
                    }
                    return l;
                }
                return null;
            }
        }
       


        public VpContainerViewModel()
        {
        }

        public VpContainerViewModel(VPContainer vpContainer)
        {
            container = vpContainer;
        }
    }
}
