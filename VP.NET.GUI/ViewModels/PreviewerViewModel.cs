using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ImageMagick;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VP.NET.GUI.Models;
using VP.NET.GUI.Views;

namespace VP.NET.GUI.ViewModels
{
    public partial class PreviewerViewModel : ViewModelBase
    {
        [ObservableProperty]
        internal string filename = "";

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
        internal List<Bitmap>? effFrameList = null;

        [ObservableProperty]
        internal int effFrameDelay = 100;

        [ObservableProperty]
        internal Stream? imageStream = null;

        [ObservableProperty]
        internal string? imageExt = null;

        private LibVLC? _libVlc = null;
        private MediaPlayer? _mediaPlayerVlc = null;
        private MemoryStream? _previewStream;
        private string _vpPath = "";
        private string extension = "";
        private VpFileEntryViewModel? item = null;
        private TextViewModel? textVM = null;
        private TextView? textDialog = null;
        private bool _vlcInitialized = false;

        private CancellationTokenSource? _cts = null;

        [ObservableProperty]
        internal int mediaVolume = 100;

        public PreviewerViewModel()
        {
            try
            {
                if (MainWindowViewModel.settings.PreviewerLibVlcViewer)
                {
                    Task.Factory.StartNew(() => { 
                        _libVlc = new LibVLC();
                        _mediaPlayerVlc = new MediaPlayer(_libVlc);
                        _mediaPlayerVlc.Volume = 100;
                        _vlcInitialized = true;
                    });
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
                    Error += ". On you need to install the \"vlc\" and \"libvlc-dev\" packages.";
                }
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.Constructor", ex);
                _vlcInitialized = true;
            }
        }

        public void EnableVLCLate()
        {
            try
            {
                if (_libVlc == null && _mediaPlayerVlc == null)
                {
                    Task.Factory.StartNew(() => {
                        _libVlc = new LibVLC();
                        _mediaPlayerVlc = new MediaPlayer(_libVlc);
                        _mediaPlayerVlc.Volume = 100;
                        _vlcInitialized = true;
                    });
                }
            }
            catch (Exception ex)
            {
                Error = "LibVLC is not avalible";
                if (Utils.IsLinux)
                {
                    Error += ". On you need to install the \"vlc\" and \"libvlc-dev\" packages.";
                }
                Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.EnableVLCLate", ex);
                _vlcInitialized = true;
            }
        }


        public void Reset()
        {
            try
            {
                _cts?.Cancel();
                InfoFile = "";
                BarVisible = false;
                Error = "";
                extension = "";
                if (_libVlc != null && _mediaPlayerVlc != null && ImageStream == null)
                    StopVLC();
                MediaButtonsVisible = false;
                Filename = "";
                MediaPaused = false;
                _vpPath = "";
                item = null;
                if(EffFrameList != null && EffFrameList.Count > 0)
                {
                    var oldEffL = EffFrameList;
                    EffFrameList = null;
                    EffFrameDelay = 100;
                    foreach(var b in oldEffL)
                        b?.Dispose();
                }
                var old = ImageStream;
                _previewStream?.Dispose();
                _previewStream = null;
                ImageStream = null;
                old?.Dispose();
                GC.Collect();
            }
            catch { }
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
                        if (!_previewStream.CanRead) return;
                        await item.vpFile!.ReadToStream(_previewStream);
                        ImageLoader(item.extension);
                        break;
                    /* Animations, maybe */
                    case "png":
                    case "apng":
                    case "ani":
                    case "eff":
                        if (!_previewStream.CanRead) return;
                        await item.vpFile!.ReadToStream(_previewStream);
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
                            if (!_previewStream.CanRead) return;
                            await item.vpFile!.ReadToStream(_previewStream);
                            VLCPlayback(extension);
                        }
                        else 
                        {
                            if(_vlcInitialized)
                                Error = "LibVLC is needed for this format";
                            else
                                Error = "LibVLC is still initializing";
                        }
                        break;

                    /* Text */
                    case "lua":
                    case "tbl":
                    case "tbm":
                    case "fs2":
                    case "fc2":
                        if (!_previewStream.CanRead) return;
                        await item.vpFile!.ReadToStream(_previewStream);
                        TextLoader();
                        break;
                    /* Default */
                    default:
                        Error = "Unsupported format";
                        break;
                }
            }
            catch(OperationCanceledException)
            {
                //silent
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
            using (var reader = new StreamReader(_previewStream, Encoding.ASCII, leaveOpen: true))
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
            if(_previewStream == null || !_previewStream.CanRead)
                return;
            if (_cts != null && _cts.IsCancellationRequested)
                return;
            using (var image = new MagickImage(_previewStream!))
            {
                image.Format = MagickFormat.Png;
                if(ext == "dds")
                    InfoFile = image.Compression.ToString();
                var st2 = new MemoryStream();
                image.Write(st2);
                st2.Position = 0;
                ImageExt = ext;
                ImageStream = st2;
            }
        }

        private async void AnimationLoader(string ext)
        {
            if (_previewStream == null || !_previewStream.CanRead)
                return;
            if (_cts != null && _cts.IsCancellationRequested)
                return;
            InfoFile = ext == "png" ? "APNG" : ext.ToUpper();
            if (ext == "png" || ext == "ani")
            {
                ImageExt = ext;
                ImageStream = _previewStream;
            }
            else if (ext == "eff")
            {
                TextLoader();
                var loadedVps = MainWindowViewModel.Instance?.WorkingFiles;
                if (loadedVps == null || !loadedVps.Any())
                {
                    Error = "No loaded VPs";
                    return;
                }

                if (_cts != null && _cts.IsCancellationRequested)
                    return;

                var effname = item?.vpFile?.info.name;
                if (string.IsNullOrEmpty(effname)) return;

                EFFHelper effFile;
                try
                {
                    _previewStream!.Position = 0;
                    effFile = EFFHelper.Parse(_previewStream!, effname.ToLower());
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                    return;
                }

                if (effFile?.FrameFiles == null || effFile.Type == null)
                {
                    Error = "Error parsing the eff file";
                    return;
                }

                if (_cts != null && _cts.IsCancellationRequested)
                    return;

                var token = _cts?.Token ?? CancellationToken.None;
                var frameCount = effFile.FrameCount;
                var isDds = effFile.Type.Equals("dds", StringComparison.OrdinalIgnoreCase);
                Bitmap[]? frames = null;

                try
                {
                    // Find each frame in open VPs
                    var frameSources = new VPFile[frameCount];
                    for (int i = 0; i < frameCount; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        var name = effFile.FrameFiles![i];
                        foreach (var vp in loadedVps)
                        {
                            if (vp.Files == null) continue;
                            foreach (var file in vp.Files)
                            {
                                if (file.VpFile == null) continue;
                                var found = file.VpFile.SearchForFileName(name);
                                if (found != null) { frameSources[i] = found; break; }
                            }
                            if (frameSources[i] != null) break;
                        }
                        if (frameSources[i] == null)
                        {
                            Error = $"Frame missing: {name}";
                            return;
                        }
                    }

                    // Read Frame bytes for each frame
                    var frameBytes = new byte[frameCount][];
                    for (int i = 0; i < frameCount; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        using var ms = new MemoryStream();
                        await frameSources[i]!.ReadToStream(ms);
                        frameBytes[i] = ms.ToArray();
                    }

                    // Convert DDS->PNG->Bitmap
                    frames = new Bitmap[frameCount];
                    await Task.Run(() =>
                    {
                        try
                        {
                            Parallel.For(0, frameCount, new ParallelOptions
                            {
                                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
                                CancellationToken = token
                            },
                            i =>
                            {
                                if (isDds)
                                {
                                    using var src = new MemoryStream(frameBytes[i]);
                                    using var image = new MagickImage(src);
                                    image.Format = MagickFormat.Png;
                                    using var dst = new MemoryStream();
                                    image.Write(dst);
                                    dst.Position = 0;
                                    frames[i] = new Bitmap(dst);
                                }
                                else
                                {
                                    using var src = new MemoryStream(frameBytes[i]);
                                    frames[i] = new Bitmap(src);
                                }
                                frameBytes[i] = null!; // liberar bytes ya consumidos
                            });
                        }
                        catch {}

                    }, token);

                    if (token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                    // Assign final frames
                    EffFrameDelay = (int)effFile.FrameDurationMs;
                    EffFrameList = frames.ToList();
                }
                catch (OperationCanceledException)
                {
                    if (frames != null)
                        foreach (var b in frames) b?.Dispose();
                }
                catch (Exception ex)
                {
                    if (frames != null)
                        foreach (var b in frames) b?.Dispose();
                    Error = "Error loading eff frames";
                    Log.Add(Log.LogSeverity.Error, "PreviewerViewModel.AnimationLoader.eff", ex);
                }
            }
            else
            {
                Error = "Animation file not supported.";
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
