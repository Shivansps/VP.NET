using Avalonia.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using VP.NET.GUI.ViewModels;

namespace VP.NET.GUI.Models
{
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
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "Settings.Load()", ex);
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public void Save(bool writeIni = true)
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
