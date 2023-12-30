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

        /// <summary>
        /// Formats a filesize value into a string with the right B/KB/MB/GB/TB suffix
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>value in a string with a suffix or string.empty if fails</returns>
        public static string FormatBytes(long bytes)
        {
            try
            {
                string[] suffix = { "B", "KB", "MB", "GB", "TB" };
                bool minus = bytes < 0 ? true : false;
                if(minus)
                {
                    bytes = bytes * -1;
                }
                int i;
                double dblSByte = bytes;
                for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
                {
                    dblSByte = bytes / 1024.0;
                }
                if(minus)
                    return String.Format("-{0:0.##} {1}", dblSByte, suffix[i]);
                else
                    return String.Format("{0:0.##} {1}", dblSByte, suffix[i]);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
