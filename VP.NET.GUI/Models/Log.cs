using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VP.NET.GUI.Models
{
    /// <summary>
    /// VP.NET logging system class
    /// </summary>
    public static class Log
    {
        public enum LogSeverity
        {
            Information,
            Warning,
            Error
        }

        public static string LogFilePath = Path.Combine(Utils.GetDataFolderPath(), "log.log");

        /// <summary>
        /// Write a log entry to console and file
        /// </summary>
        /// <param name="logSeverity"></param>
        /// <param name="from"></param>
        /// <param name="data"></param>
        public static void Add(LogSeverity logSeverity, string from, string data)
        {
            var logString = DateTime.Now.ToString() + " - *" + logSeverity.ToString() + "* : (" + from + ") " + data;

            Task.Run(async () => {
                try
                {
                    await WaitForFileAccess(LogFilePath);
                    using (var writer = new StreamWriter(LogFilePath, true))
                    {
                        writer.WriteLine(logString, Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    WriteToConsole(ex.Message);
                }
                WriteToConsole(logString);
            });      
        }

        /// <summary>
        /// Write a log entry to console and file
        /// </summary>
        /// <param name="logSeverity"></param>
        /// <param name="from"></param>
        /// <param name="exception"></param>
        public static void Add(LogSeverity logSeverity, string from, Exception exception)
        {
            Add(logSeverity, from, exception.Message);
            if (exception.InnerException != null)
            {
                Add(logSeverity, from, exception.InnerException.Message);
            }
        }

        /// <summary>
        /// Write a string to VS console
        /// </summary>
        /// <param name="data"></param>
        public static void WriteToConsole(string data)
        {
            try
            {
                if (Debugger.IsAttached)
                {
                    System.Diagnostics.Debug.WriteLine(data);
                }
            }
            catch { }
        }

        /// <summary>
        /// Wait for the log file being available for write
        /// Returns when the file is ready
        /// </summary>
        /// <param name="filename"></param>
        private static async Task WaitForFileAccess(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read))
                    {
                        inputStream.Close();
                        return;
                    }
                }
            }
            catch (IOException)
            {
                await Task.Delay(500);
                await WaitForFileAccess(filename);
            }
        }
    }
}
