using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;

namespace VP.NET.GUI.Models
{
    public class EFFHelper
    {
        public string? Type { get; private set; }
        public int FrameCount { get; private set; }
        public int Fps { get; private set; }
        public double FrameDurationMs => 1000.0 / Fps;
        public List<string>? FrameFiles { get; private set; }

        public static EFFHelper Parse(MemoryStream stream, string effFileName)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var anim = new EFFHelper();

            stream.Position = 0;
            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || !line.StartsWith("$")) continue;

                    int colon = line.IndexOf(':');
                    if (colon < 0) continue;

                    string key = line.Substring(1, colon - 1).Trim();
                    string value = line.Substring(colon + 1).Trim();

                    switch (key.ToLowerInvariant())
                    {
                        case "type":
                            anim.Type = value.TrimStart('.').ToLowerInvariant();
                            break;
                        case "frames":
                            anim.FrameCount = int.Parse(value, CultureInfo.InvariantCulture);
                            break;
                        case "fps":
                            anim.Fps = int.Parse(value, CultureInfo.InvariantCulture);
                            break;
                    }
                }
            }

            // Validate Eff
            if (string.IsNullOrEmpty(anim.Type))
                throw new InvalidDataException($"'{effFileName}': missing $Type");
            if (anim.FrameCount <= 0)
                throw new InvalidDataException($"'{effFileName}': invalid $Frames ({anim.FrameCount})");
            if (anim.Fps <= 0)
                throw new InvalidDataException($"'{effFileName}': insvalid $FPS ({anim.Fps})");

            string baseName = Path.GetFileNameWithoutExtension(effFileName);
            anim.FrameFiles = new List<string>(anim.FrameCount);

            for (int i = 0; i < anim.FrameCount; i++)
            {
                string expected = $"{baseName}_{i:D4}.{anim.Type}";
                anim.FrameFiles.Add(expected);
            }

            return anim;
        }
    }
}
