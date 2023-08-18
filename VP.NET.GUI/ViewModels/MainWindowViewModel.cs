using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<VpViewModel> WorkingFiles { get; set; } = new ObservableCollection<VpViewModel>();

        [ObservableProperty]
        public VpFolderViewModel folderViewModel = new VpFolderViewModel();

        public static MainWindowViewModel? Instance;

        public MainWindowViewModel()
        {
            Instance = this;
        }


        internal async void OpenFile()
        {
            FilePickerOpenOptions options = new FilePickerOpenOptions();
            options.AllowMultiple = true;
            options.Title = "Open VP files";
            options.FileTypeFilter = new List<FilePickerFileType> {
                new("VP files (*.vp, *.vpc)") { Patterns = new[] { "*.vp", "*.vpc" } },
                new("All files (*.*)") { Patterns = new[] { "*" } }
            };


            var result = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(options);

            if (result != null && result.Count > 0)
            {
                foreach (var file in result)
                {
                    try
                    {
                        WorkingFiles.Add(new VpViewModel(file.Path.LocalPath));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }

        internal async void OpenFolder()
        {
            FolderPickerOpenOptions options = new FolderPickerOpenOptions();
            options.Title = "Open a folder containing VP files";
            var result = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(options);

            if (result != null && result.Count > 0)
            {

                string[] files = Directory.GetFiles(result[0].Path.LocalPath,"*.vp*");

                foreach (var file in files)
                {
                    try
                    {
                        WorkingFiles.Add(new VpViewModel(file));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }
    }
}
