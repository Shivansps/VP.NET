using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace VP.NET.GUI.ViewModels
{
    public partial class VpFileEntryViewModel : ViewModelBase
    {
        [ObservableProperty]
        public string name = string.Empty;

        [ObservableProperty]
        public string? fileDate = string.Empty;

        [ObservableProperty]
        public string? compression = string.Empty;

        [ObservableProperty]
        public bool isMarked = false;

        [ObservableProperty]
        private Bitmap? icon;

        public VPFile? vpFile;

        public VpFileEntryViewModel()
        {

        }

        /// <summary>
        /// Creates a VPFileEntryView from a VPFile
        /// This creates the icons and display data for each file that is displayed on the right side
        /// </summary>
        /// <param name="vpFile"></param>
        public VpFileEntryViewModel(VPFile vpFile)
        {
            Name = vpFile.info.name;
            FileDate = VPTime.GetDateFromUnixTimeStamp(vpFile.info.timestamp).ToString();
            if (vpFile.compressionInfo.header.HasValue && vpFile.type == VPFileType.File)
            {
                Compression = vpFile.compressionInfo.header.ToString();
            }
            if (vpFile.type == VPFileType.Directory)
            {
                icon = new Bitmap(AssetLoader.Open(new Uri("avares://VP.NET.GUI/Assets/icons/folder.png")));
            }
            else
            {
                if (vpFile.type == VPFileType.File)
                {
                    var nameparts = vpFile.info.name.Split(".");
                    var extension = nameparts[nameparts.Length - 1];
                    switch(extension.ToLower())
                    {
                        /* Images */
                        case "png":
                        case "jpg":
                        case "jpeg":
                        case "pcx":
                        case "dds":
                        case "tga":
                        case "apng":
                            icon = new Bitmap(AssetLoader.Open(new Uri("avares://VP.NET.GUI/Assets/icons/image.png")));
                            break;
                        /* Videos */
                        case "mp4":
                        case "mve":
                                icon = new Bitmap(AssetLoader.Open(new Uri("avares://VP.NET.GUI/Assets/icons/movie.png")));
                            break;
                        /* OGG */
                        case "ogg":
                            icon = new Bitmap(AssetLoader.Open(new Uri("avares://VP.NET.GUI/Assets/icons/ogg.png")));
                            break;
                        /* Audio */
                        case "wav":
                            icon = new Bitmap(AssetLoader.Open(new Uri("avares://VP.NET.GUI/Assets/icons/audio.png")));
                            break;
                        /* Scripts */
                        case "lua":
                            icon = new Bitmap(AssetLoader.Open(new Uri("avares://VP.NET.GUI/Assets/icons/script.png")));
                            break;
                        /* Tables */
                        case "tbl":
                        case "tbm":
                            icon = new Bitmap(AssetLoader.Open(new Uri("avares://VP.NET.GUI/Assets/icons/table.png")));
                            break;
                        /* Default */
                        default:
                            icon = new Bitmap(AssetLoader.Open(new Uri("avares://VP.NET.GUI/Assets/icons/text.png")));
                            break;
                    }
                }
            }

            this.vpFile = vpFile;
        }
    }
}
