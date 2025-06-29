using CommunityToolkit.Mvvm.ComponentModel;
using System;
using ImageMagick;
using System.IO;
using Avalonia.Media.Imaging;
using VP.NET.GUI.Models;
using Avalonia.Platform;
using LibVLCSharp.Shared;
using AnimatedImage.Avalonia;

namespace VP.NET.GUI.ViewModels
{
    public partial class PreviewerViewModel : ViewModelBase
    {
        [ObservableProperty]
        internal string filename = "";

        [ObservableProperty]
        internal AnimatedImageSourceStream? animation = null;

        [ObservableProperty]
        internal bool barVisible = false;

        [ObservableProperty]
        internal bool mediaPaused = false;

        [ObservableProperty]
        internal bool mediaButtonsVisible = false;

        [ObservableProperty]
        internal string infoFile = "";

        [ObservableProperty]
        internal string error = "";

        [ObservableProperty]
        internal Bitmap? imageSource = null;

        [ObservableProperty]
        internal bool playingAnim = false;

        private LibVLC? _libVlc = null;
        private MediaPlayer? _mediaPlayerVlc = null;
        private MemoryStream? _previewStream;
        private string _vpPath = "";

        [ObservableProperty]
        internal int mediaVolume = 100;

        public PreviewerViewModel()
        {
            try
            {
                _libVlc = new LibVLC();
                _mediaPlayerVlc = new MediaPlayer(_libVlc);
                _mediaPlayerVlc.Volume = 100;
            }
            catch (Exception ex)
            {
                Error = "LibVLC is not avalible";
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.Constructor", ex);
            }
        }


        public void Reset()
        {
            InfoFile = "";
            BarVisible = false;
            Error = "";
            ImageSource = null;
            if (_libVlc != null && _mediaPlayerVlc != null)
                StopVLC();
            _previewStream?.Dispose();
            _previewStream = null;
            Animation = null;
            MediaButtonsVisible = false;
            Filename = "";
            MediaPaused = false;
            _vpPath = "";
        }

        public async void StartPreview(VpFileEntryViewModel item, string vpPath)
        {
            Reset();
            if (item.vpFile == null || item.vpFile.type != VPFileType.File || item.IsNewFile)
                return;
            BarVisible = true;
            _vpPath = vpPath;
            _previewStream = new MemoryStream();
            try
            {
                Filename = item.Name;
                switch (item.extension)
                {
                    /* Images */
                    case "jpg":
                    case "jpeg":
                    case "pcx":
                    case "dds":
                    case "tga":
                        await item.vpFile.ReadToStream(_previewStream);
                        ImageLoader(item.extension);
                        break;
                    /* Animations, maybe */
                    case "png":
                        await item.vpFile.ReadToStream(_previewStream);
                        if (APNGHelper.IsApng(_previewStream!))
                            AnimationLoader(item.extension);
                        else
                            ImageLoader(item.extension);  
                        break;
                    case "gif":
                    case "apng":
                        await item.vpFile.ReadToStream(_previewStream);
                        AnimationLoader(item.extension);
                        break;
                    /* VLC */
                    case "mp4":
                    case "mve":
                    case "ogg":
                    case "wav":
                    case "mp3":
                    case "aac":
                        if (_libVlc != null && _mediaPlayerVlc != null)
                        {
                            await item.vpFile.ReadToStream(_previewStream);
                            VLCPlayback();
                        }
                        else 
                        {
                            Error = "LibVLC is needed for this format";
                            _previewStream.Dispose();
                        }
                        break;

                    /* Text */
                    case "lua":
                    case "tbl":
                    case "tbm":
                    case "eff":

                    case "ani":
                    /* Default */
                    default:
                        Error = "Unsupported format";
                        break;
                }
            }
            catch (Exception ex)
            {
                Error = "An exception has ocurred";
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.StartPreview()", ex);
            }
        }
       
        private void ImageLoader(string ext)
        {
            using (var image = new MagickImage(_previewStream!))
            {
                image.Format = MagickFormat.Png;
                if(ext == "dds")
                    InfoFile = image.Compression.ToString();
                using (var st2 = new MemoryStream())
                {
                    image.Write(st2);
                    st2.Position = 0;
                    ImageSource = new Bitmap(st2);
                    PlayingAnim = false;
                }
            }
        }

        private void AnimationLoader(string ext)
        {
            InfoFile = ext == "png" ? "APNG" : ext.ToUpper();
            if (ext == "png" || ext == "gif")
            {
                Animation = new AnimatedImageSourceStream(_previewStream!);
                PlayingAnim = true;
            }
        }

        private void VLCPlayback()
        {
            InfoFile = "LibVLC";
            _previewStream?.Seek(0, SeekOrigin.Begin);
            using var media = new Media(_libVlc!, new StreamMediaInput(_previewStream!));
            _mediaPlayerVlc!.Play(media);
            MediaButtonsVisible = true;
        }

        internal void StopVLC()
        {
            try
            {
                _mediaPlayerVlc?.Stop();
                MediaPaused = true;
                _previewStream?.Seek(0,SeekOrigin.Begin);
            }
            catch (Exception ex) 
            {
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.StopVLC()", ex);
            }

        }

        internal void PauseVLC()
        {
            try
            {
                _mediaPlayerVlc?.Pause();
                MediaPaused = true;
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.PauseVLC()", ex);
            }

        }

        internal void ResumeVLC()
        {
            try
            {
                _mediaPlayerVlc?.Play();
                MediaPaused = false;
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.ResumeVLC()", ex);
            }
        }

        internal void RestartVLC()
        {
            VLCPlayback();
        }

        internal void OpenExternally()
        {
            try
            {
                Directory.CreateDirectory(Utils.GetCacheFolderPath());
                var dest = Path.Combine(Utils.GetCacheFolderPath(), Filename);
                StopVLC();
                using (var fileStream = File.Create(dest))
                {
                    _previewStream?.Seek(0, SeekOrigin.Begin);
                    _previewStream?.CopyTo(fileStream);
                }

                Utils.OpenExternal(dest);
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.OpenExternally()", ex);
            }
        }

        internal void CloseIfPath(string? vpFilePath)
        {
            if(vpFilePath != null && vpFilePath == _vpPath)
            {
                Reset();
            }
        }

        public void UpdateMediaVolume()
        {
            if(_mediaPlayerVlc != null)
                _mediaPlayerVlc.Volume = MediaVolume;
        }
    }
}
