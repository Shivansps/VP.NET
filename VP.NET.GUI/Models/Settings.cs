using Avalonia.Controls;
using System;
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
        public string? LastVPLoadPath { get; set; } = null;
        public string? LastFileExtractionPath { get; set; } = null;


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
                    }

                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

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
            }
        }
    }
}
