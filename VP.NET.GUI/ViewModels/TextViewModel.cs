using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VP.NET.GUI.ViewModels
{
    public partial class TextViewModel : ViewModelBase
    {
        [ObservableProperty]
        public string text = "";

        public TextViewModel()
        {

        }
    }
}
