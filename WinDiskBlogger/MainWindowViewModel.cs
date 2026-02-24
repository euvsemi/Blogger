using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace WinDiskBlogger
{
    public class MainWindowViewModel : IDropTarget
    {
        private readonly FileOperator _fileOperator;
        private string _rootDirectory;

        public MainWindowViewModel(string rootDirectory)
        {
            TreeRoots = new ObservableCollection<FileSystemItem>();
            BuildTree(rootDirectory);
            _fileOperator = FileOperator.Instance;
        }

        public string AppName => Assembly.GetExecutingAssembly().GetName().Name;

        public string AppVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public string AppAuthor => Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(m => m.Key == "Author").Value;
        public string AppEmail => Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(m => m.Key == "Email").Value;
        public string AppWeChat => Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(m => m.Key == "WeChat").Value;

        public string Title => $"{AppName} - {AppVersion} (Author:{AppAuthor},{AppEmail})";
        public ObservableCollection<FileSystemItem> TreeRoots { get; private set; }

        public void BuildTree(string rootDirectory)
        {
            TreeRoots.Clear();
            TreeRoots.Add(new FileSystemItem
            {
                Name = System.IO.Path.GetFileName(rootDirectory),
                FullPath = rootDirectory,
                Type = ItemType.Folder
            });

            BuildDirectoryTree(TreeRoots[0]);
            _rootDirectory = rootDirectory;
        }

        public void ChoosePngOrSvg()
        {
            string tips = """
                请选择一个 PNG 或 SVG 文件，程序将自动生成 ICO 图标并保存在程序目录下的 Icons 文件夹中.
                PNG必须是1:1（正方形）的，且建议至少512x512像素，以保证生成的 ICO 在各种尺寸下都能保持清晰。
                SVG必须是1:1（正方形）的矢量图，且建议包含清晰的细节和轮廓，以确保在缩小到16x16像素时仍然可辨识。
                SVG 是生成高质量 ICO 的最佳起点，因为它是矢量格式，可以无损缩放到任何尺寸，而不会失去清晰度。

                .ICO 文件实际上是一个“图像包”，它可以包含多个不同尺寸和颜色深度的图像。这样，Windows 就可以根据需要选择最合适的图像来显示。
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

        public void ChooseNewFolder()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true && string.IsNullOrWhiteSpace(dialog.SelectedPath) == false)
            {
                BuildTree(dialog.SelectedPath);
            }
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            // 如果不写这两行，鼠标会显示禁止图标，且无法触发拖动视觉效果
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as FileSystemItem;
            var targetItem = dropInfo.TargetItem as FileSystemItem;
            if (sourceItem != null && targetItem != null)
            {
                if (sourceItem.Type == ItemType.File && targetItem.Type == ItemType.File)
                {
                    // 必须是同一个文件夹内排序文件
                    if (string.Compare(System.IO.Path.GetDirectoryName(sourceItem.FullPath), System.IO.Path.GetDirectoryName(targetItem.FullPath), true) == 0)
                    {
                        var success = FindParent(sourceItem, TreeRoots[0], out var parent);
                        if (success && parent != null)
                        {
                            parent.Items.Move(parent.Items.IndexOf(sourceItem), dropInfo.InsertIndex);
                            if (CanChangeFileName(parent.Items))
                            {
                                ChangeFileName(parent.Items);
                            }
                            else
                            {
                                MessageBox.Show("部分文件被占用，无法排序。请关闭相关程序后重试。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                else if (sourceItem.Type == ItemType.Folder && targetItem.Type == ItemType.Folder)
                {
                    // 只能在同一层级内排序文件夹
                    if (string.Compare(System.IO.Path.GetDirectoryName(sourceItem.FullPath), System.IO.Path.GetDirectoryName(targetItem.FullPath), true) == 0)
                    {
                        var success = FindParent(sourceItem, TreeRoots[0], out var parent);
                        if (success && parent != null)
                        {
                            parent.Items.Move(parent.Items.IndexOf(sourceItem), dropInfo.InsertIndex);
                            try
                            {
                                ChangeDirectoryName(parent.Items);
                            }
                            catch
                            {
                                MessageBox.Show($"文件夹被占用，无法排序。请关闭相关程序后重试。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public void OpenFile(FileSystemItem item)
        {
            if (item.Type == ItemType.File && string.IsNullOrWhiteSpace(item.FullPath) == false)
            {
                try
                {
                    // 1. 如果是本地路径，先检查文件是否存在
                    if (File.Exists(item.FullPath) == false)
                    {
                        throw new FileNotFoundException("找不到指定的文件。", item.FullPath);
                    }

                    // 2. 根据操作系统执行不同的启动指令
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Windows 下必须设置 UseShellExecute 为 true 才能调用默认程序
                        Process.Start(new ProcessStartInfo(item.FullPath) { UseShellExecute = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", item.FullPath);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Process.Start("open", item.FullPath);
                    }
                    else
                    {
                        throw new PlatformNotSupportedException("不支持当前操作系统。");
                    }
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    // 常见场景：用户电脑上没有安装能打开该后缀名的软件
                    MessageBox.Show($"无法打开文件：没有关联的应用程序。错误信息: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // 其他未知错误（权限不足、路径非法等）
                    MessageBox.Show($"打开文件时发生异常: {ex.Message}");
                }
            }
        }

        private void BuildDirectoryTree(FileSystemItem parent)
        {
            try
            {
                var dirs = Directory.GetDirectories(parent.FullPath);
                foreach (var dir in dirs)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    var newItem = new FileSystemItem
                    {
                        Name = dirInfo.Name,
                        FullPath = dir,
                        Type = ItemType.Folder
                    };
                    parent.Items.Add(newItem);
                }
                var files = Directory.GetFiles(parent.FullPath);
                foreach (var file in files)
                {
                    var newItem = new FileSystemItem
                    {
                        Name = System.IO.Path.GetFileName(file),
                        FullPath = file,
                        Type = ItemType.File
                    };
                    parent.Items.Add(newItem);
                }

                foreach (var dir in parent.Items.Where(x => x.Type == ItemType.Folder))
                {
                    BuildDirectoryTree(dir); // 递归构建子目录树
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 处理访问被拒绝的情况（例如系统文件夹）
            }
        }

        private bool CanChangeFileName(ObservableCollection<FileSystemItem> items)
        {
            try
            {
                for (var i = 0; i < items.Count; i++)
                {
                    if (items[i].Type == ItemType.File)
                        File.Open(items[i].FullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None).Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void ChangeFileName(ObservableCollection<FileSystemItem> items)
        {
            int index = 0;

            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Type == ItemType.File)
                {
                    index++;
                    var newName = _fileOperator.CalculateNewFileName(items[i].FullPath, index);
                    File.Move(items[i].FullPath, newName);
                    items[i].FullPath = newName;
                }
            }

            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Type == ItemType.File)
                {
                    var newName = _fileOperator.GenerateNewFileName(items[i].FullPath);
                    File.Move(items[i].FullPath, newName);
                    items[i].FullPath = newName;
                    items[i].Name = Path.GetFileName(newName);
                }
            }
        }

        private void ChangeDirectoryName(ObservableCollection<FileSystemItem> items)
        {
            int index = 0;
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Type == ItemType.Folder)
                {
                    index++;
                    var newName = _fileOperator.CalculateNewDirectoryName(items[i].FullPath, index);
                    Directory.Move(items[i].FullPath, newName);
                    items[i].FullPath = newName;
                }
            }
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Type == ItemType.Folder)
                {
                    var newName = _fileOperator.CalculateNewDirectoryName(items[i].FullPath);
                    Directory.Move(items[i].FullPath, newName);
                    items[i].FullPath = newName;
                    items[i].Name = Path.GetFileName(newName);
                }
            }
            BuildTree(_rootDirectory);
        }

        private bool FindParent(FileSystemItem itemToFind, FileSystemItem root, out FileSystemItem parent)
        {
            if (root == itemToFind)
            {
                parent = null;
                return true;
            }
            foreach (var item in root.Items)
            {
                if (item == itemToFind)
                {
                    parent = root;
                    return true;
                }
            }
            foreach (var item in root.Items)
            {
                if (FindParent(itemToFind, item, out var target))
                {
                    parent = target;
                    return true;
                }
            }

            parent = null;
            return false;
        }
    }
}