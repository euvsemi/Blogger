using System;
using System.Collections.Generic;
using System.Text;

namespace WinDiskBlogger
{
    internal class FileOrderManager
    {
        public static FileOrderManager Instance { get; private set; } = new FileOrderManager();

        private FileOrderManager()
        { }

        public const string ORDER_FILE_NAME = ".order";

        public Dictionary<string, int> LoadOrder(string directoryPath)
        {
            var orderDict = new Dictionary<string, int>();
            string orderFilePath = System.IO.Path.Combine(directoryPath, ORDER_FILE_NAME);
            if (System.IO.File.Exists(orderFilePath))
            {
                var lines = System.IO.File.ReadAllLines(orderFilePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string fileName = lines[i].Trim();
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        orderDict[fileName] = i;
                    }
                }
            }
            return orderDict;
        }

        public void SaveOrder(string directoryPath, IEnumerable<string> itemNames)
        {
            string orderFilePath = System.IO.Path.Combine(directoryPath, ORDER_FILE_NAME);
            System.IO.File.WriteAllLines(orderFilePath, itemNames);
        }
    }
}