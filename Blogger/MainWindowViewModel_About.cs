using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Blogger
{
    public partial class MainWindowViewModel
    {
        private Assembly _assembly;
        public string Title => $"{AppName} - {AppVersion} (Author:{AppAuthor},{AppEmail})";
        public string AppName => _assembly.GetName().Name;
        public string AppVersion => _assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        public string AppAuthor => _assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(m => m.Key == "Author").Value;
        public string AppEmail => _assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(m => m.Key == "Email").Value;
        public string AppWeChat => _assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(m => m.Key == "WeChat").Value;
    }
}
