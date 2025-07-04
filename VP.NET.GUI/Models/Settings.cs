using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace VP.NET.GUI.Models
{
    public class ExternalPreviewApp
    {
        public string Path { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;

        public ExternalPreviewApp(string path, string arguments, string extension)
        {
            Path = path;
            Arguments = arguments;
            Extension = extension;
        }
    }

    /// <summary>
    /// Class to store VP Net GUI settings
    /// </summary>
    public class Settings
    {
        public string? LastAddFilesPath { get; set; } = null;
        public string? LastVPLoadPath { get; set; } = null;
        public string? LastFileExtractionPath { get; set; } = null;
        public string? ToolLastLZ41FileDecompressionOpenPath { get; set; } = null;
        public string? ToolLastLZ41FileDecompressionDestinationPath { get; set; } = null;
        public string? ToolLastLZ41FileCompressionOpenPath { get; set; } = null;
        public string? ToolLastLZ41FileCompressionDestinationPath { get; set; } = null;
        public string? ToolLastVPDecompressionOpenPath { get; set; } = null;
        public string? ToolLastVPDecompressionDestinationPath { get; set; } = null;
        public string? ToolLastVPCompressionOpenPath { get; set; } = null;
        public string? ToolLastVPCompressionDestinationPath { get; set; } = null;
        public string? ToolLastFolderToVPFolderPath { get; set; } = null;
        public string? ToolLastFolderToVPVPSavePath { get; set; } = null;
        public bool PreviewerEnabled { get; set; } = true;
        public bool PreviewerTextViewer { get; set; } = true;
        public bool PreviewerLibVlcViewer { get; set; } = true;
        public List<ExternalPreviewApp> ExternalExtensions { get; set; } = new List<ExternalPreviewApp>();

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public void Load()
        {
            try
            {
                if (File.Exists(Path.Combine(Utils.GetDataFolderPath(), "settings.json")))
                {
                    using FileStream jsonFile = File.OpenRead(Path.Combine(Utils.GetDataFolderPath(), "settings.json"));
                    var tempSettings = JsonSerializer.Deserialize<Settings>(jsonFile)!;
                    jsonFile.Close();
                    if (tempSettings != null)
                    {
                        LastFileExtractionPath = tempSettings.LastFileExtractionPath;
                        LastVPLoadPath = tempSettings.LastVPLoadPath;
                        ToolLastLZ41FileDecompressionOpenPath = tempSettings.ToolLastLZ41FileDecompressionOpenPath;
                        ToolLastLZ41FileDecompressionDestinationPath = tempSettings.ToolLastLZ41FileDecompressionDestinationPath;
                        ToolLastLZ41FileCompressionOpenPath = tempSettings.ToolLastLZ41FileCompressionOpenPath;
                        ToolLastLZ41FileCompressionDestinationPath = tempSettings.ToolLastLZ41FileCompressionDestinationPath;
                        ToolLastVPDecompressionOpenPath = tempSettings.ToolLastVPDecompressionOpenPath;
                        ToolLastVPDecompressionDestinationPath = tempSettings.ToolLastVPDecompressionDestinationPath;
                        ToolLastVPCompressionOpenPath = tempSettings.ToolLastVPCompressionOpenPath;
                        ToolLastVPCompressionDestinationPath = tempSettings.ToolLastVPCompressionDestinationPath;
                        PreviewerEnabled = tempSettings.PreviewerEnabled;
                        LastAddFilesPath = tempSettings.LastAddFilesPath;
                        PreviewerTextViewer = tempSettings.PreviewerTextViewer;
                        ExternalExtensions = tempSettings.ExternalExtensions;
                        PreviewerLibVlcViewer = tempSettings.PreviewerLibVlcViewer;
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "Settings.Load()", ex);
            }
        }

        public void Reset()
        {
            LastFileExtractionPath = null;
            LastVPLoadPath = null;
            ToolLastLZ41FileDecompressionOpenPath = null;
            ToolLastLZ41FileDecompressionDestinationPath = null;
            ToolLastLZ41FileCompressionOpenPath = null;
            ToolLastLZ41FileCompressionDestinationPath = null;
            ToolLastVPDecompressionOpenPath = null;
            ToolLastVPDecompressionDestinationPath = null;
            ToolLastVPCompressionOpenPath = null;
            ToolLastVPCompressionDestinationPath = null;
            PreviewerEnabled = true;
            PreviewerTextViewer = true;
            LastAddFilesPath = null;
            ExternalExtensions.Clear();
            Save();
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(Path.Combine(Utils.GetDataFolderPath(), "settings.json"), json, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "Settings.Save()", ex);
            }
        }
    }
}
