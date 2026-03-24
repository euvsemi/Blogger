using Blogger;
using System.Windows;

namespace WinDiskBlogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var viewModel = new MainWindowViewModel("C:\\Users\\Value Lee\\Desktop\\C#");
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}