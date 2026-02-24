using System.IO;
using Path = System.IO.Path;

namespace WinDiskBlogger
{
    public class FileOperator
    {
        public static readonly FileOperator Instance = new FileOperator();

        public FileOperator()
        {
        }

        public IList<string> GetMarkdownFiles(string path)
        {
            return Directory.GetFiles(path, "*.md", SearchOption.TopDirectoryOnly);
        }

        public IList<string> GetDirectories(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        }

        // 生成UUID和序列号的文件名，格式为 "ADEDA46B-AE38-45D2-B8F6-6305B62DD6F801. filename.md"
        public string CalculateNewFileName(string fullPath, int sequenceNumber)
        {
            var dir = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileName(fullPath);
            // 去除原来的序号
            if (fileName.Length >= 3)
            {
                if (char.IsDigit(fileName[0]) && char.IsDigit(fileName[1]) && fileName[2] == '.')
                {
                    fileName = fileName.Substring(3, fileName.Length - 3).Trim();
                }
            }
            // 添加新的序号
            fileName = sequenceNumber.ToString("D2") + ". " + fileName;
            // 添加UUID
            return Path.Combine(dir, Guid.NewGuid() + fileName);
        }

        // 去除UUID
        public string GenerateNewFileName(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var filename = Path.GetFileName(path);
            filename = filename.Trim().Substring(36, filename.Length - 36).Trim();
            return Path.Combine(dir, filename);
        }

        public string CalculateNewDirectoryName(string fullPath)
        {
            var dir = Path.GetDirectoryName(fullPath);
            var filename = Path.GetFileName(fullPath);
            filename = filename.Trim().Substring(36, filename.Length - 36).Trim();
            return Path.Combine(dir, filename);
        }

        public string CalculateNewDirectoryName(string fullPath, int sequenceNumber)
        {
            var parentDir = Path.GetDirectoryName(fullPath);
            var dirName = Path.GetFileName(fullPath);
            // 去除原来的序号
            if (dirName.Length >= 3)
            {
                if (char.IsDigit(dirName[0]) && char.IsDigit(dirName[1]) && dirName[2] == '.')
                {
                    dirName = dirName.Substring(3, dirName.Length - 3).Trim();
                }
            }
            // 添加新的序号
            dirName = sequenceNumber.ToString("D2") + ". " + dirName;
            // 添加UUID
            return Path.Combine(parentDir, Guid.NewGuid() + dirName);
        }
    }
}