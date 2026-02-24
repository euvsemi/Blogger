using System.Reflection;
using System.Windows;

namespace WinDiskBlogger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShowAssemblyInfo();
        }

        public static void ShowAssemblyInfo()
        {
            // 获取当前执行的程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 1. 程序集名称
            string name = assembly.GetName().Name;

            // 2. 版本号 (AssemblyVersion)
            string version = assembly.GetName().Version?.ToString();

            // 3. 文件版本 (AssemblyFileVersion)
            string fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

            // 4. 产品信息版本 (InformationalVersion / Product Version)
            // 这通常是我们在 .csproj 中定义的 <Version> 标签内容
            string productVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            // 5. 公司名称
            string company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

            // 6. 产品名称
            string product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;

            // 7. 版权信息
            string copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

            // 8. 关于“作者”和“邮箱”
            string description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

            var metadataAttributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            string author = metadataAttributes.FirstOrDefault(attr => attr.Key == "Author")?.Value;
            string email = metadataAttributes.FirstOrDefault(attr => attr.Key == "Email")?.Value;
            string wechat = metadataAttributes.FirstOrDefault(attr => attr.Key == "WeChat")?.Value;
        }
    }
}