using System;
using System.IO;
using System.Text;

namespace VP.NET.GUI.Models
{
    /// <summary>
    /// Lazy APNG Helper to check if png file is a apng
    /// </summary>
    public class APNGHelper
    {
        private static byte[] FrameSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        /// <summary>
        ///  Reads a stream to verify if it is a valid APNG file
        ///  Checks for acTL chuck presence on the first 5000 bytes of data. No other data loading is done.
        ///  Dosent close or disposes the stream.
        ///  Throws a exception if the stream dosent contains a png file data
        /// </summary>
        /// <param name="pngStream"></param>
        /// <returns>true if file is apng</returns>
        /// <exception cref="Exception"></exception>
        public static bool IsApng(Stream pngStream)
        {
            if (!IsBytesEqual(ReadBytes(pngStream,FrameSignature.Length), FrameSignature))
                throw new Exception("File signature incorrect.");

            var bufferLength = pngStream.Length < 5000 ? (int)pngStream.Length : 5000;

            var s = Encoding.ASCII.GetString(ReadBytes(pngStream, bufferLength));
            if(s.Contains("acTL"))
            {
                pngStream.Seek(0, SeekOrigin.Begin);
                return true;
            }
            pngStream.Seek(0, SeekOrigin.Begin);
            return false;
        }

        private static bool IsBytesEqual(byte[] byte1, byte[] byte2)
        {
            if (byte1.Length != byte2.Length)
                return false;

            for (int i = 0; i < byte1.Length; i++)
            {
                if (byte1[i] != byte2[i])
                    return false;
            }
            return true;
        }

        private static byte[] ReadBytes(Stream ms, int count)
        {
            var buffer = new byte[count];

            if (ms.Read(buffer, 0, count) != count)
                throw new Exception("End reached.");

            return buffer;
        }
    }
}
