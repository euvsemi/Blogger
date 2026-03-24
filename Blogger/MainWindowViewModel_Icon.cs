using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using WinDiskBlogger;

namespace Blogger
{
    public partial class MainWindowViewModel
    {

        public void ChoosePngOrSvg()
        {
            string tips = """
                请选择一个 PNG 或 SVG 文件，程序将自动生成 ICO 图标并保存在程序目录下的 Icons 文件夹中.

                1. PNG必须是1:1（正方形）的，且建议至少512x512像素，以保证生成的 ICO 在各种尺寸下都能保持清晰。
                2. SVG必须是1:1（正方形）的矢量图，且建议包含清晰的细节和轮廓，以确保在缩小到16x16像素时仍然可辨识。
                3. SVG 是生成高质量 ICO 的最佳起点，因为它是矢量格式，可以无损缩放到任何尺寸，而不会失去清晰度。
                4. .ICO 文件实际上是一个“图像包”，它可以包含多个不同尺寸和颜色深度的图像。这样，Windows 就可以根据需要选择最合适的图像来显示。
                Windows 图标通常包含多个尺寸的图像，以适应不同的显示需求和环境。常见的尺寸包括：
                (16x16	窗体左上角小图标、任务栏小图标、细节列表视图。)
                (32x32	传统的桌面图标、任务栏标准图标。)
                (48x48	资源管理器“中等图标”视图。)
                (256x256	资源管理器“特大图标”、高分屏（4K）缩放。)
                当你把 .ico 设为程序图标时，Windows 会根据当前的显示设置（比如桌面图标设为了“大图标”），自动从这个“包”里挑选最接近的一个尺寸来显示。
                """;

            MessageBox.Show(tips, "提示", MessageBoxButton.OK, MessageBoxImage.Information);

            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.Filter = "Image Files (*.png;*.svg)|*.png;*.svg";
            if (dialog.ShowDialog() == true && string.IsNullOrWhiteSpace(dialog.FileName) == false)
            {
                try
                {
                    var inputPath = dialog.FileName;
                    var outputDirectory = Path.Combine(AppContext.BaseDirectory, "Icons", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"));
                    Directory.CreateDirectory(outputDirectory);
                    var outputPath = System.IO.Path.Combine(outputDirectory, System.IO.Path.GetFileNameWithoutExtension(inputPath) + ".ico");
                    if (System.IO.Path.GetExtension(inputPath).Equals(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        IconHelper.CreateSharpIcon(inputPath, outputPath);
                    }
                    else if (System.IO.Path.GetExtension(inputPath).Equals(".svg", StringComparison.OrdinalIgnoreCase))
                    {
                        IconHelper.ConvertSvgToIcon(inputPath, outputPath);
                    }
                    File.Copy(inputPath, System.IO.Path.Combine(outputDirectory, System.IO.Path.GetFileName(inputPath)), true);

                    Process.Start("explorer.exe", outputDirectory);
                    MessageBox.Show($"图标已生成: {outputPath}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show("生成图标时发生错误。请确保输入文件是有效的 PNG 或 SVG，并且程序有权限写入输出目录。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void CheckIconFrames()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.Filter = "Icon Files (*.ico)|*.ico";
            if (dialog.ShowDialog() == true && string.IsNullOrWhiteSpace(dialog.FileName) == false)
            {
                try
                {
                    var result = IconHelper.CheckIconFrames(dialog.FileName);
                    MessageBox.Show(result, "图标信息", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show("读取图标信息时发生错误。请确保选择的文件是有效的 ICO 图标文件，并且程序有权限访问该文件。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
