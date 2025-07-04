using CommunityToolkit.Mvvm.ComponentModel;
using System;
using ImageMagick;
using System.IO;
using Avalonia.Media.Imaging;
using VP.NET.GUI.Models;
using LibVLCSharp.Shared;
using AnimatedImage.Avalonia;
using System.Diagnostics;
using System.Linq;
using VP.NET.GUI.Views;
using static System.Net.Mime.MediaTypeNames;
using SkiaSharp;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;

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
        private string extension = "";
        private VpFileEntryViewModel? item = null;
        private TextViewModel? textVM = null;
        private TextView? textDialog = null;

        private CancellationTokenSource? _cts = null;

        [ObservableProperty]
        internal int mediaVolume = 100;

        public PreviewerViewModel()
        {
            try
            {
                if (MainWindowViewModel.settings.PreviewerLibVlcViewer)
                {
                    _libVlc = new LibVLC();
                    _mediaPlayerVlc = new MediaPlayer(_libVlc);
                    _mediaPlayerVlc.Volume = 100;
                }
                else
                {
                    Error = "LibVLC is disabled in settings.";
                }
            }
            catch (Exception ex)
            {
                Error = "LibVLC is not avalible";
                if(Utils.IsLinux)
                {
                    Error += ". On linux you need to install the \"vlc\" and \"libvlc-dev\" packages.";
                }
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.Constructor", ex);
            }
        }

        public void EnableVLCLate()
        {
            try
            {
                if (_libVlc == null && _mediaPlayerVlc == null)
                {
                    _libVlc = new LibVLC();
                    _mediaPlayerVlc = new MediaPlayer(_libVlc);
                    _mediaPlayerVlc.Volume = 100;
                }
            }
            catch (Exception ex)
            {
                Error = "LibVLC is not avalible";
                if (Utils.IsLinux)
                {
                    Error += ". On linux you need to install the \"vlc\" and \"libvlc-dev\" packages.";
                }
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.EnableVLCLate", ex);
            }
        }


        public void Reset()
        {
            _cts?.Cancel();
            InfoFile = "";
            BarVisible = false;
            Error = "";
            extension = "";
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
            item = null;
        }

        public async void StartPreview(VpFileEntryViewModel item, string vpPath)
        {
            if (!MainWindowViewModel.settings.PreviewerEnabled)
                return;
            Reset();
            if (item.vpFile == null || item.vpFile.type != VPFileType.File || item.IsNewFile)
                return;
            BarVisible = true;
            _vpPath = vpPath;
            this.item = item;
            _previewStream = new MemoryStream();
            _cts = new CancellationTokenSource();
            try
            {
                Filename = item.Name;
                extension = item.extension;
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
                    case "ani":
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
                        if (MainWindowViewModel.settings.PreviewerLibVlcViewer && _libVlc != null && _mediaPlayerVlc != null)
                        {
                            await item.vpFile.ReadToStream(_previewStream);
                            VLCPlayback(extension);
                        }
                        else 
                        {
                            Error = "LibVLC is needed for this format";
                        }
                        break;

                    /* Text */
                    case "lua":
                    case "tbl":
                    case "tbm":
                    case "eff":
                    case "fs2":
                    case "fc2":
                        await item.vpFile.ReadToStream(_previewStream);
                        TextLoader();
                        break;
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

        private void TextLoader()
        {
            if (!MainWindowViewModel.settings.PreviewerTextViewer)
                return;
            if (_previewStream == null)
                return;
            if(textVM == null)
                textVM = new TextViewModel();
            if (textDialog == null)
            {
                textDialog = new TextView();
                textDialog.DataContext = textVM;
            }
            using (var reader = new StreamReader(_previewStream, Encoding.ASCII))
            {
                _previewStream.Position = 0;
                textVM.Text = reader.ReadToEnd();
                _previewStream.Position = 0;
            }
            try
            {
                textDialog.Show(MainWindow.Instance!);
            }catch (InvalidOperationException)
            {
                textDialog = new TextView();
                textDialog.DataContext = textVM;
                textDialog.Show(MainWindow.Instance!);
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
            if(ext == "ani")
            {
                var ani = ANIHelper.ReadANI(_previewStream!);
                if (ani != null)
                {
                    int f = 0;
                    var localCts = _cts;
                    Task.Factory.StartNew(async () => {
                        do
                        {
                            if (localCts?.IsCancellationRequested == true)
                                break;
                            ImageSource = ani.bitmapFrames[f];
                            await Task.Delay(1000 / ani.header.fps);
                            f++;
                            if (f >= ani.header.numFrames)
                                f = 0;
                        } while (true);

                        ani.Dispose();
                    });
                }
                else
                    Error = "An error has ocurred while parsing the file";
            }
        }

        private void VLCPlayback(string ext)
        {
            InfoFile = "LibVLC";
            if (ext != "wav" && ext != "mp3" && ext != "aac")
            {
                Error = "Do not close the video window";
            }
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
                MediaPaused = false;
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
            VLCPlayback(extension);
        }

        internal async void OpenExternally()
        {
            try
            {
                if(_previewStream == null)
                    return;
                if(_previewStream.Length == 0)
                {
                    await item!.vpFile!.ReadToStream(_previewStream);
                }

                Directory.CreateDirectory(Utils.GetCacheFolderPath());
                var dest = Path.Combine(Utils.GetCacheFolderPath(), Filename);
                StopVLC();

                using (var fileStream = File.Create(dest, 8192))
                {
                    _previewStream?.Seek(0, SeekOrigin.Begin);
                    _previewStream?.CopyTo(fileStream);
                }

                var customExternalApp = MainWindowViewModel.settings.ExternalExtensions.FirstOrDefault(x=>x.Extension.ToLower() == extension);

                if (customExternalApp == null)
                {
                    Utils.OpenExternal(dest);
                }
                else
                {
                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = customExternalApp.Path;
                        process.StartInfo.Arguments = customExternalApp.Arguments.Replace("[FILEPATH]", dest);
                        process.Start();
                    }
                }
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
