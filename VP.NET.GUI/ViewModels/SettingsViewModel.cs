using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.IO;
using VP.NET.GUI.Models;
using VP.NET.GUI.Views;
using System.Linq;

namespace VP.NET.GUI.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        [ObservableProperty]
        internal bool previewerEnabled = true;

        [ObservableProperty]
        internal bool previewerLiVlcEnabled = true;

        [ObservableProperty]
        internal bool previewerTextViewer = true;

        [ObservableProperty]
        internal ObservableCollection<ExternalPreviewApp> externalPreviewApps = new ObservableCollection<ExternalPreviewApp>();

        [ObservableProperty]
        internal string editExecutable = string.Empty;

        [ObservableProperty]
        internal string editArguments = "[FILEPATH]";

        [ObservableProperty]
        internal string editExtension = string.Empty;

        private string defaultArguments = "[FILEPATH]";

        [ObservableProperty]
        internal bool editing = false;

        public SettingsViewModel()
        {
            Load();
        }

        internal void Reset()
        {
            bool oldValue = MainWindowViewModel.settings.PreviewerEnabled;
            MainWindowViewModel.settings.Reset();
            ExternalPreviewApps.Clear();
            Load();
            if (oldValue != MainWindowViewModel.settings.PreviewerEnabled)
            {
                MainWindowViewModel.Instance?.UpdatePreviewerStatus();
            }
        }

        internal void OpenLog()
        {
            if(File.Exists(Log.LogFilePath)) 
                Utils.OpenExternal(Log.LogFilePath);
        }


        private void Load()
        {
            PreviewerEnabled = MainWindowViewModel.settings.PreviewerEnabled;
            PreviewerTextViewer = MainWindowViewModel.settings.PreviewerTextViewer;
            PreviewerLiVlcEnabled = MainWindowViewModel.settings.PreviewerLibVlcViewer;
            foreach (ExternalPreviewApp app in MainWindowViewModel.settings.ExternalExtensions)
            {
                ExternalPreviewApps.Add(app);
            }
        }

        public void Save()
        {
            bool oldValue = MainWindowViewModel.settings.PreviewerEnabled;
            bool oldVlcValue = MainWindowViewModel.settings.PreviewerLibVlcViewer;
            MainWindowViewModel.settings.PreviewerEnabled = PreviewerEnabled;
            MainWindowViewModel.settings.PreviewerTextViewer = PreviewerTextViewer;
            MainWindowViewModel.settings.PreviewerLibVlcViewer = PreviewerLiVlcEnabled;
            if (oldValue != MainWindowViewModel.settings.PreviewerEnabled)
            {
                MainWindowViewModel.Instance?.UpdatePreviewerStatus();
            }
            if (oldVlcValue == false && MainWindowViewModel.settings.PreviewerLibVlcViewer)
            {
                MainWindowViewModel.Instance?.PreviewerEnableVlcRuntime();
            }
            MainWindowViewModel.settings.ExternalExtensions.Clear();
            foreach (ExternalPreviewApp app in ExternalPreviewApps)
            {
                MainWindowViewModel.settings.ExternalExtensions.Add(app);
            }
            MainWindowViewModel.settings.Save();
        }

        internal async void OpenFile()
        {
            try
            {
                FilePickerOpenOptions options = new FilePickerOpenOptions();
                options.AllowMultiple = false;
                options.Title = "Select application executable";
                options.FileTypeFilter = new List<FilePickerFileType> {
                new("All files (*.*)") { Patterns = new[] { "*" } }
            };

                var result = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(options);

                if (result != null)
                {
                    var path = result[0].TryGetLocalPath();
                    if (path != null) 
                    {
                        EditExecutable = path;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "SettingsViewModel.OpenFile(2)", ex);
            }
        }

        internal void Add()
        {
            if (EditExecutable.Trim().Length > 0 && EditArguments.Trim().Length > 0 && EditExtension.Trim().Length > 0)
            {
                if (EditExtension.Contains("."))
                {
                    EditExtension = EditExtension.Replace(".","");
                }

                var exist = ExternalPreviewApps.FirstOrDefault(x => x.Extension.ToLower() == EditExtension.ToLower());
                if (!Editing && exist != null)
                {
                    MessageBox.Show(null, "This extension already exists in the list", "Extension exist", MessageBox.MessageBoxButtons.OK);
                    return;
                }

                if (!EditArguments.Contains(defaultArguments))
                {
                    EditExtension += " " + defaultArguments;
                }

                if(Editing && exist != null)
                {
                    exist.Arguments = EditArguments;
                    exist.Path = EditExecutable;
                    //ugly hack to update list
                    var old = ExternalPreviewApps.ToList();
                    ExternalPreviewApps.Clear();
                    foreach (ExternalPreviewApp app in old)
                    {
                        ExternalPreviewApps.Add(app);
                    }
                }
                else
                {
                    ExternalPreviewApps.Add(new ExternalPreviewApp(EditExecutable, EditArguments, EditExtension));
                }

                EditExecutable = "";
                EditArguments = defaultArguments;
                EditExtension = "";
                Editing = false;
            }
        }

        public void LoadEdit(ExternalPreviewApp item)
        {
            EditArguments = item.Arguments;
            EditExtension = item.Extension;
            EditExecutable = item.Path;
            Editing = true;
        }

        public void Delete()
        {
            var exist = ExternalPreviewApps.FirstOrDefault(x => x.Extension.ToLower() == EditExtension.ToLower());
            if (exist != null)
            {
                ExternalPreviewApps.Remove(exist);
            }
            EditExecutable = "";
            EditArguments = defaultArguments;
            EditExtension = "";
            Editing = false;
        }
    }
}
