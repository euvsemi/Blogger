using DryIoc.ImTools;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using WinDiskBlogger;

namespace Blogger
{
    public partial class MainWindowViewModel : IDropTarget
    {
        private string _rootDirectory;

        public MainWindowViewModel(string rootDirectory)
        {
            TreeRoots = new ObservableCollection<FileSystemItem>();
            BuildTree(rootDirectory);
            _assembly = Assembly.GetExecutingAssembly();
        }

        public ObservableCollection<NavItem> NavItems { get; set; }

        public ObservableCollection<FileSystemItem> TreeRoots { get; private set; }

        public void BuildTree(string rootDirectory)
        {
            TreeRoots.Clear();
            TreeRoots.Add(new FileSystemItem
            {
                Name = System.IO.Path.GetFileName(rootDirectory),
                FullPath = rootDirectory,
                Type = ItemType.Folder,
                IsExpanded = true
            });

            BuildDirectoryTree(TreeRoots[0]);
            //if (TreeRoots[0] != null && TreeRoots[0].Items != null)
            //{
            //    foreach (var item in TreeRoots[0].Items)
            //    {
            //        if (item.Type == ItemType.Folder)
            //        {
            //            item.IsExpanded = true;
            //        }
            //    }
            //}
            _rootDirectory = rootDirectory;
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
                // 必须是同一个文件夹内排序文件
                if (string.Compare(System.IO.Path.GetDirectoryName(sourceItem.FullPath), System.IO.Path.GetDirectoryName(targetItem.FullPath), true) == 0)
                {
                    var success = FindParent(sourceItem, TreeRoots[0], out var parent);
                    if (success && parent != null)
                    {
                        parent.Items.Move(parent.Items.IndexOf(sourceItem), dropInfo.InsertIndex);
                        FileOrderManager.Instance.SaveOrder(parent.FullPath, parent.Items.Select(x => x.FullPath));
                    }
                }

                //else if (sourceItem.Type == ItemType.Folder && targetItem.Type == ItemType.Folder)
                //{
                //    // 只能在同一层级内排序文件夹
                //    if (string.Compare(System.IO.Path.GetDirectoryName(sourceItem.FullPath), System.IO.Path.GetDirectoryName(targetItem.FullPath), true) == 0)
                //    {
                //        var success = FindParent(sourceItem, TreeRoots[0], out var parent);
                //        if (success && parent != null)
                //        {
                //            parent.Items.Move(parent.Items.IndexOf(sourceItem), dropInfo.InsertIndex);
                //            try
                //            {
                //                ChangeDirectoryName(parent.Items);
                //            }
                //            catch
                //            {
                //                MessageBox.Show($"文件夹被占用，无法排序。请关闭相关程序后重试。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                //                return;
                //            }
                //        }
                //    }
                //}
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
                var orderInfo = FileOrderManager.Instance.LoadOrder(parent.FullPath);

                var dirs = Directory.GetDirectories(parent.FullPath);
                var files = Directory.GetFiles(parent.FullPath).Remove(Path.Combine(parent.FullPath, FileOrderManager.ORDER_FILE_NAME));

                var items = dirs.Concat(files).ToList();
                items.Sort((x, y) =>
                {
                    bool xRet = orderInfo.TryGetValue(x, out var xOrder);
                    bool yRet = orderInfo.TryGetValue(y, out var yOrder);

                    if (xRet && yRet)
                    {
                        return xOrder - yOrder;
                    }
                    else if (xRet == false && yRet)
                    {
                        return -1;
                    }
                    else if (xRet && yRet == false)
                    {
                        return 1;
                    }
                    else
                    {
                        return string.Compare(x, y, true);
                    }
                });

                foreach (var item in items)
                {
                    if (dirs.Contains(item))
                    {
                        var dirInfo = new DirectoryInfo(item);
                        var newItem = new FileSystemItem
                        {
                            Name = dirInfo.Name,
                            FullPath = item,
                            Type = ItemType.Folder
                        };
                        parent.Items.Add(newItem);
                    }
                    else if (files.Contains(item))
                    {
                        var newItem = new FileSystemItem
                        {
                            Name = System.IO.Path.GetFileName(item),
                            FullPath = item,
                            Type = ItemType.File
                        };
                        parent.Items.Add(newItem);
                    }
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