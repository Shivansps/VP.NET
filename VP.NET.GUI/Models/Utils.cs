using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VP.NET.GUI.Models
{
    /// <summary>
    /// Utilitary Class for VP NET GUI
    /// </summary>
    public static class Utils
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static void OpenExternal(string path)
        {
            try
            {
                using (var process = new Process())
                {
                    if (IsWindows)
                    {
                        process.StartInfo.FileName = "cmd";
                        process.StartInfo.Arguments = $"/c start {path}";
                    }
                    else if (IsLinux)
                    {
                        process.StartInfo.FileName = "xdg-open";
                        process.StartInfo.Arguments = path;
                    }
                    else if (IsMacOS)
                    {
                        process.StartInfo.FileName = "open";
                        process.StartInfo.Arguments = path;
                    }
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "Utils.OpenExternal()", ex);
            }
        }

        public static string GetCacheFolderPath()
        {
            return Path.Combine(GetDataFolderPath(), "cache");
        }

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
                Log.Add(Log.LogSeverity.Error, "Utils.FormatBytes()", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the complete size of all files in a folder and subdirectories in bytes
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns>size in bytes or 0 if failed</returns>
        public static async Task<long> GetSizeOfFolderInBytes(string folderPath)
        {
            return await Task<long>.Run(() =>
            {
                try
                {
                    if (Directory.Exists(folderPath))
                    {
                        return Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories).Sum(fileInfo => new FileInfo(fileInfo).Length);
                    }
                }
                catch (Exception ex)
                {
                    Log.Add(Log.LogSeverity.Warning, "KnUtils.GetSizeOfFolderInBytes", ex);
                }
                return 0;
            });
        }
    }
}
