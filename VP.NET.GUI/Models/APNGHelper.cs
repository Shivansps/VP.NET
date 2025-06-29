using Metsys.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VP.NET.GUI.Models
{
    /*
     * Reads a stream to verify if it is a valid APNG file
     * Checks for acTL chuck presence. No other data loading is done.
     */
    public class APNGHelper
    {
        private static byte[] FrameSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public static bool IsApng(MemoryStream ms)
        {
            if (!IsBytesEqual(ReadBytes(ms,FrameSignature.Length), FrameSignature))
                throw new Exception("File signature incorrect.");

            var s = Encoding.ASCII.GetString(ReadBytes(ms, 5000));
            if(s.Contains("acTL"))
            {
                ms.Seek(0, SeekOrigin.Begin);
                return true;
            }
            ms.Seek(0, SeekOrigin.Begin);
            return false;
        }

        public static bool IsBytesEqual(byte[] byte1, byte[] byte2)
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

        public static byte[] ReadBytes(Stream ms, int count)
        {
            var buffer = new byte[count];

            if (ms.Read(buffer, 0, count) != count)
                throw new Exception("End reached.");

            return buffer;
        }
    }
}
