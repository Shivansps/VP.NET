using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using VP.NET.GUI.Models;

namespace VP.NET.GUI.ViewModels
{
    public partial class VpFileEntryViewModel : ViewModelBase
    {
        [ObservableProperty]
        public string name = string.Empty;

        [ObservableProperty]
        public string? fileDate = string.Empty;

        [ObservableProperty]
        public string? fileSize = string.Empty;

        [ObservableProperty]
        public string? compression = string.Empty;

        [ObservableProperty]
        public bool isMarkedDelete = false;

        [ObservableProperty]
        public bool isNewFile = false;

        [ObservableProperty]
        private Bitmap? icon;

        public VPFile? vpFile;

        public string extension = string.Empty;

        public VpFileEntryViewModel()
        {

        }

        /// <summary>
        /// Unflags this file for seletion
        /// </summary>
        internal void CancelDelete()
        {
            if(vpFile != null && vpFile.parent != null)
            {
                if(vpFile.parent.DeleteStatus() == false)
                {
                    vpFile.Delete(false);
                    IsMarkedDelete = false;
                }
            }
        }

        /// <summary>
        /// Creates a VPFileEntryView from a VPFile
        /// This creates the icons and display data for each file that is displayed on the right side
        /// </summary>
        /// <param name="vpFile"></param>
        public VpFileEntryViewModel(VPFile vpFile)
        {
            try
            {
                Name = "_"+vpFile.info.name; //visual hack
                if (vpFile.type == VPFileType.Directory)
                {
                    icon = new Bitmap(AssetLoader.Open(new Uri("avares://VP.NET.GUI/Assets/icons/folder.png")));
                }
                else
                {
                    if (vpFile.type == VPFileType.File)
                    {
                        FileDate = VPTime.GetDateFromUnixTimeStamp(vpFile.info.timestamp).ToString();
                        FileSize = Utils.FormatBytes(vpFile.info.size);
                        if (vpFile.compressionInfo.header.HasValue && vpFile.type == VPFileType.File)
                        {
                            Compression = vpFile.compressionInfo.header.ToString();
                        }
                        else
                        {
                            Compression = "NO";
                        }
                        var nameparts = vpFile.info.name.Split(".");
                        extension = nameparts[nameparts.Length - 1].ToLower();
                        switch (extension)
                        {
                            /* Images */
                            case "png":
                            case "jpg":
                            case "jpeg":
                            case "pcx":
                            case "gif":
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
            }catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "VpFileEntryViewModel", ex);
            }
            this.vpFile = vpFile;
        }
    }
}
