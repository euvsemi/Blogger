using System.Collections.ObjectModel;

namespace WinDiskBlogger
{
    public class FileSystemItem : BindableBase
    {
        private string _fullPath;
        private ItemType _itemType;
        private string _name;

        public FileSystemItem()
        {
            Items = new ObservableCollection<FileSystemItem>();
        }

        public string FullPath
        {
            get { return _fullPath; }
            set { SetProperty(ref _fullPath, value); }
        }

        public ObservableCollection<FileSystemItem> Items { get; set; }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public ItemType Type
        {
            get { return _itemType; }
            set { SetProperty(ref _itemType, value); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
    }
}