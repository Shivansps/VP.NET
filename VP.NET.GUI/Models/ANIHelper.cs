using Avalonia;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace VP.NET.GUI.Models
{
    public static class ANIHelper
    {
        //https://wiki.hard-light.net/index.php/ANI_formal_definition

        public class AniFile : IDisposable
        {
            public AniHeader header;                    // Ani header
            public List<AniFrame> frames = new();       // Frames in raw format
            public List<AniKey> keys = new();           // Key Frames
            public int endCount;                        // Not sure what this is

            public void Dispose()
            {
                frames.Clear();
                keys.Clear();
            }

            /// <summary>
            /// Allocates a new WriteableBitmap matching this ANI's canvas, suitable
            /// as the reusable target for <see cref="AniFrame.RenderTo"/>.
            /// </summary>
            public WriteableBitmap CreateMatchingBitmap() =>
                new WriteableBitmap(
                    new PixelSize(header.width, header.height),
                    new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Rgba8888,
                    Avalonia.Platform.AlphaFormat.Unpremul);
        }

        public struct AniHeader
        {
            public short shouldBeZero;  // always 0
            public short version;       // >= 2
            public short fps;           // played with <fps> frames per second
            public byte r, g, b;        // Transparent color fallback
            public short width, height; // width, height
            public short numFrames;     // number of frames
            public byte packerCode;     // code used for compressed (repeated) bytes
            public byte[] palette;      // 256 * 3 = 768 bytes
            public short numKeys;
        }

        public struct AniKey
        {
            public short twoByte;
            public short startCount;
        }

        public class AniFrame
        {
            public byte[]? buffer;

            public Bitmap ToBitmap(ref AniHeader header)
            {
                // Create a bitmap of the correct size
                var bitmap = new WriteableBitmap(new PixelSize(header.width, header.height), new Vector(96, 96), Avalonia.Platform.PixelFormat.Rgb32);

                // Build color palette (256 colors)
                Color[] palette = new Color[256];
                for (int i = 0; i < 256; i++)
                {
                    byte r = header.palette[i * 3];
                    byte g = header.palette[i * 3 + 1];
                    byte b = header.palette[i * 3 + 2];
                    palette[i] = Color.FromArgb(r, g, b);
                }

                // Create a writable pixel buffer (use PixelFormat to handle transparency)
                var pixels = new byte[header.width * header.height * 4]; // 4 bytes per pixel (RGBA)

                int pixelIndex = 0;
                for (int y = 0; y < header.height; y++)
                {
                    for (int x = 0; x < header.width; x++)
                    {
                        byte pixel = buffer![pixelIndex++];
                        int colorIndex = pixel;

                        // Check for transparency
                        if (pixel == 254)
                        {
                            // Transparent pixel, make it fully transparent
                            pixels[(y * header.width + x) * 4] = 0;       // R
                            pixels[(y * header.width + x) * 4 + 1] = 0;   // G
                            pixels[(y * header.width + x) * 4 + 2] = 0;   // B
                            pixels[(y * header.width + x) * 4 + 3] = 0;   // A
                        }
                        else
                        {
                            // Use the color from the palette
                            var color = palette[colorIndex];
                            pixels[(y * header.width + x) * 4] = color.R;
                            pixels[(y * header.width + x) * 4 + 1] = color.G;
                            pixels[(y * header.width + x) * 4 + 2] = color.B;
                            pixels[(y * header.width + x) * 4 + 3] = 255; // Fully opaque
                        }
                    }
                }

                // Write pixels into the Avalonia Bitmap
                using (var frameBuffer = bitmap.Lock())
                {
                    Marshal.Copy(pixels, 0, frameBuffer.Address, pixels.Length);
                }

                return bitmap;
            }

            // ─────────────────────────────────────────────────────────────────────
            //  Render this frame into an existing WriteableBitmap.
            //  Avoids allocating a new WriteableBitmap (+ its W*H*4 buffer)
            //  every frame. The target MUST be sized header.width x header.height
            //  and use PixelFormat.Rgba8888 / AlphaFormat.Unpremul.
            //  After calling this, invalidate the visual displaying the bitmap
            //  so the new pixels are repainted.
            // ─────────────────────────────────────────────────────────────────────
            public void RenderTo(WriteableBitmap target, ref AniHeader header)
            {
                if (target == null) throw new ArgumentNullException(nameof(target));
                if (buffer == null) throw new InvalidOperationException("Frame buffer is null.");
                if (target.PixelSize.Width  != header.width ||
                    target.PixelSize.Height != header.height)
                {
                    throw new ArgumentException(
                        $"Target size {target.PixelSize} doesn't match frame " +
                        $"({header.width}x{header.height}).", nameof(target));
                }

                int w = header.width;
                int h = header.height;
                byte[] palette = header.palette;

                // Write directly into the locked framebuffer to avoid the temp byte[].
                using var fb = target.Lock();
                int stride = fb.RowBytes;
                IntPtr basePtr = fb.Address;

                // We'll build one row of RGBA bytes and Marshal.Copy it per row to
                // handle stride padding cleanly.
                byte[] row = new byte[w * 4];
                int pixelIndex = 0;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        byte p = buffer[pixelIndex++];
                        int dst = x * 4;
                        if (p == 254)
                        {
                            row[dst]     = 0;
                            row[dst + 1] = 0;
                            row[dst + 2] = 0;
                            row[dst + 3] = 0;
                        }
                        else
                        {
                            int pi = p * 3;
                            row[dst]     = palette[pi];
                            row[dst + 1] = palette[pi + 1];
                            row[dst + 2] = palette[pi + 2];
                            row[dst + 3] = 255;
                        }
                    }
                    Marshal.Copy(row, 0, basePtr + y * stride, row.Length);
                }
            }
        }

        public static AniFile? ReadANI(Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                using var br = new BinaryReader(stream, new UTF8Encoding(), true);

                AniFile ani = new();
                ani.header = ReadHeader(br);

                if (ani.header.shouldBeZero != 0 || ani.header.version < 2 || ani.header.numFrames <= 0)
                    throw new Exception("Invalid Ani file");

                // Read keyframes
                for (int i = 0; i < ani.header.numKeys; i++)
                {
                    ani.keys.Add(new AniKey
                    {
                        twoByte = br.ReadInt16(),
                        startCount = br.ReadInt16()
                    });
                }

                // no idea what is this
                ani.endCount = br.ReadInt32();

                // Decode all frames
                DecodeFrames(br, ani);

                return ani;
            }
            catch (Exception ex)
            {
                Log.Add(Log.LogSeverity.Error, "ANIHelper.ReadANI", ex);
            }
            return null;
        }

        private static AniHeader ReadHeader(BinaryReader br)
        {
            var header = new AniHeader
            {
                shouldBeZero = br.ReadInt16(),
                version = br.ReadInt16(),
                fps = br.ReadInt16(),
                r = br.ReadByte(),
                g = br.ReadByte(),
                b = br.ReadByte(),
                width = br.ReadInt16(),
                height = br.ReadInt16(),
                numFrames = br.ReadInt16(),
                packerCode = br.ReadByte()
            };

            header.palette = br.ReadBytes(256 * 3); // 768 bytes
            header.numKeys = br.ReadInt16();

            return header;
        }

        private static void DecodeFrames(BinaryReader br, AniFile ani)
        {
            int width = ani.header.width;
            int height = ani.header.height;

            // Pad to 4-byte alignment
            int paddedWidth = width;
            if (paddedWidth % 4 != 0)
                paddedWidth += (4 - (paddedWidth % 4));

            int frameSize = paddedWidth * height;

            byte[] curFrame = new byte[frameSize];
            byte[] lastFrame = new byte[frameSize];

            // Fill lastFrame with RGB color assuming grayscale/8-bit index
            byte fill = ani.header.r;
            Array.Fill(lastFrame, fill);

            for (int frameIndex = 0; frameIndex < ani.header.numFrames; frameIndex++)
            {
                br.ReadByte(); // framebyte, unused

                int runcount = 0;
                byte runvalue = 0;
                int pos = 0;

                byte[] p = curFrame;
                byte[] p2 = frameIndex > 0 ? lastFrame : curFrame;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (runcount > 0)
                        {
                            runcount--;
                        }
                        else
                        {
                            runvalue = br.ReadByte();
                            if (runvalue == ani.header.packerCode)
                            {
                                runcount = br.ReadByte();
                                if (runcount < 2)
                                {
                                    runvalue = ani.header.packerCode;
                                }
                                else
                                {
                                    runvalue = br.ReadByte();
                                }
                            }
                        }

                        byte pixel = runvalue;

                        if (runvalue == 254)
                        {
                            // Transparent, use last frame pixel
                            pixel = p2[pos];
                        }

                        p[pos++] = pixel;
                    }

                    // Apply padding
                    pos += paddedWidth - width;
                }

                // Store frame
                var frameCopy = new byte[frameSize];
                Buffer.BlockCopy(curFrame, 0, frameCopy, 0, frameSize);
                ani.frames.Add(new AniFrame { buffer = frameCopy });

                // Copy curFrame to lastFrame
                Buffer.BlockCopy(curFrame, 0, lastFrame, 0, frameSize);
            }
        }
    }
}
