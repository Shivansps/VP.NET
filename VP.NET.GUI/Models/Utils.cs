using System;
using System.IO;


namespace VP.NET.GUI.Models
{
    /// <summary>
    /// Utilitary Class for VP NET GUI
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Full path to VP NET GUI data folder
        /// </summary>
        /// <returns></returns>
        public static string GetDataFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VpNetGUI");
        }
    }
}
