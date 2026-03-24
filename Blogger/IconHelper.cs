using Svg;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace WinDiskBlogger
{
    public static class IconHelper
    {
        public enum ImageType
        {
            SVG,
            PNG
        }

        public static string CheckIconFrames(string filePath)
        {
            string result = "";
            using (Stream iconStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // 专门用于解析 .ico 文件的解码器
                IconBitmapDecoder decoder = new IconBitmapDecoder(iconStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                // Frames 的数量就是图标包含的尺寸数量
                result += $"该图标包含 {decoder.Frames.Count} 种尺寸：\n";
                foreach (var frame in decoder.Frames)
                {
                    result += $"- {frame.PixelWidth}x{frame.PixelHeight} (位深度: {frame.Format.BitsPerPixel})\n";
                }
            }
            return result;
        }

        public static void CreateSharpIcon(string inputPath, string outputPath, ImageType imageType)
        {
            if (imageType == ImageType.PNG)
            {
                CreateSharpIcon(inputPath, outputPath);
            }
            else if (imageType == ImageType.SVG)
            {
                ConvertSvgToIcon(inputPath, outputPath);
            }
        }

        #region PNG

        /// <summary>
        /// 将单张大图转换为包含多种尺寸的高清 ICO
        /// </summary>
        /// <param name="inputPath">源图片路径 (建议 512x512 以上的 PNG)</param>
        /// <param name="outputPath">输出 .ico 路径</param>
        public static void CreateSharpIcon(string inputPath, string outputPath)
        {
            int[] targetSizes = { 16, 32, 48, 256 };
            List<Bitmap> resizedBitmaps = new List<Bitmap>();

            using (var source = new Bitmap(inputPath))
            {
                foreach (int size in targetSizes)
                {
                    resizedBitmaps.Add(GenerateResizedImage(source, size, size));
                }
            }

            WriteIconFile(resizedBitmaps, outputPath);

            // 释放内存
            resizedBitmaps.ForEach(b => b.Dispose());
        }

        // 核心：高质量缩放算法
        private static Bitmap GenerateResizedImage(Image source, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            // 保持透明度
            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var g = Graphics.FromImage(destImage))
            {
                // 设置最高质量的缩放参数
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic; // 关键：高质量双三次插值
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        // 将多个 Bitmap 写入 ICO 容器
        private static void WriteIconFile(List<Bitmap> bitmaps, string outputPath)
        {
            using (var stream = new FileStream(outputPath, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)0);      // Reserved
                writer.Write((short)1);      // Type (Icon)
                writer.Write((short)bitmaps.Count);

                int offset = 6 + bitmaps.Count * 16;
                List<byte[]> imageData = new List<byte[]>();

                foreach (var bmp in bitmaps)
                {
                    using (var ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png); // 现代 ICO 内部使用 PNG
                        byte[] data = ms.ToArray();
                        imageData.Add(data);

                        writer.Write((byte)(bmp.Width >= 256 ? 0 : bmp.Width));
                        writer.Write((byte)(bmp.Height >= 256 ? 0 : bmp.Height));
                        writer.Write((byte)0); // Colors
                        writer.Write((byte)0); // Reserved
                        writer.Write((short)1); // Planes
                        writer.Write((short)32); // Bits per pixel
                        writer.Write(data.Length);
                        writer.Write(offset);
                        offset += data.Length;
                    }
                }

                foreach (var data in imageData) writer.Write(data);
            }
        }

        #endregion PNG

        #region SVG

        /// <summary>
        /// 将 SVG 文件转换为包含多尺寸的高清 ICO
        /// </summary>
        public static void ConvertSvgToIcon(string svgPath, string outputPath)
        {
            int[] targetSizes = { 16, 32, 48, 256 };
            List<Bitmap> renderedBitmaps = new List<Bitmap>();

            // 1. 加载 SVG 文档
            var svgDocument = SvgDocument.Open(svgPath);

            foreach (int size in targetSizes)
            {
                // 2. 渲染特定尺寸的位图 (SVG 的优势：拉伸不模糊)
                Bitmap bmp = svgDocument.Draw(size, size);
                renderedBitmaps.Add(bmp);
            }

            // 3. 调用我们之前的打包逻辑
            WriteIconFile2(renderedBitmaps, outputPath);

            // 释放资源
            renderedBitmaps.ForEach(b => b.Dispose());
        }

        private static void WriteIconFile2(List<Bitmap> bitmaps, string outputPath)
        {
            using (var stream = new FileStream(outputPath, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)0);      // Reserved
                writer.Write((short)1);      // Type (Icon)
                writer.Write((short)bitmaps.Count);

                int offset = 6 + bitmaps.Count * 16;
                List<byte[]> imageData = new List<byte[]>();

                foreach (var bmp in bitmaps)
                {
                    using (var ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png); // 内部存为 PNG 以保持高清透明
                        byte[] data = ms.ToArray();
                        imageData.Add(data);

                        writer.Write((byte)(bmp.Width >= 256 ? 0 : bmp.Width));
                        writer.Write((byte)(bmp.Height >= 256 ? 0 : bmp.Height));
                        writer.Write((byte)0); // Colors
                        writer.Write((byte)0); // Reserved
                        writer.Write((short)1); // Planes
                        writer.Write((short)32); // Bits per pixel
                        writer.Write(data.Length);
                        writer.Write(offset);
                        offset += data.Length;
                    }
                }
                foreach (var data in imageData) writer.Write(data);
            }
        }

        #endregion SVG
    }
}