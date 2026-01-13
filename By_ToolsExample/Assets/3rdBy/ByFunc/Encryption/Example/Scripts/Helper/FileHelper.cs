namespace Helper
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// 文件读取类
    /// </summary>
    public abstract class FileHelper
    {
        public enum PathType
        {
            [Description("桌面")] Desktop,
            [Description("[只读]设备上该项目数据文件夹的路径")] Data,
            [Description("StreamingAssets")] StreamingAssets,

            [Description(@"持久化数据_C:\Users\用户\AppData\LocalLow\开发公司名称\软件名称")]
            PersistentData,
        }

        public static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string StreamingAssetsPath => Application.streamingAssetsPath;
        public static string PersistentDataPath => Application.persistentDataPath;
        public static string DataPath => Application.dataPath;

        public static void DeletedFile(string fileName, PathType pathType)
        {
            fileName = $"{fileName}.txt";
            switch (pathType)
            {
                case PathType.Desktop:
                    DeletedFile(DesktopPath, fileName);
                    break;
                case PathType.StreamingAssets:
                    DeletedFile(StreamingAssetsPath, fileName);
                    break;
                case PathType.PersistentData:
                    DeletedFile(PersistentDataPath, fileName);
                    break;
                case PathType.Data:
                    DeletedFile(DataPath, fileName);
                    break;
            }
        }

        public static string ReadFile(string fileName, PathType pathType)
        {
            var content = string.Empty;
            fileName = $"{fileName}.txt";
            switch (pathType)
            {
                case PathType.Desktop:
                    content = ReadFile(DesktopPath, fileName);
                    break;
                case PathType.StreamingAssets:
                    content = ReadFile(StreamingAssetsPath, fileName);
                    break;
                case PathType.PersistentData:
                    content = ReadFile(PersistentDataPath, fileName);
                    break;
                case PathType.Data:
                    content = ReadFile(DataPath, fileName);
                    break;
            }

            return content;
        }

        public static void WriteFile(string fileName, string content, PathType pathType)
        {
            fileName = $"{fileName}.txt";
            switch (pathType)
            {
                case PathType.Desktop:
                    WriteFile(DesktopPath, fileName, content);
                    break;
                case PathType.StreamingAssets:
                    WriteFile(StreamingAssetsPath, fileName, content);
                    break;
                case PathType.PersistentData:
                    WriteFile(PersistentDataPath, fileName, content);
                    break;
                case PathType.Data:
                    WriteFile(DataPath, fileName, content);
                    break;
            }
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public static void WriteFile(string path, string fileName, string content)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, fileName);
            Debug.Log($"写入文件:{path}");
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        public static string ReadFile(string fileFullPath)
        {
            if (!File.Exists(fileFullPath)) return string.Empty;
            var content = File.ReadAllText(fileFullPath);
            return content;
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="path"></param>
        public static string ReadFile(string path, string fileName)
        {
            if (!Directory.Exists(path)) return string.Empty;

            path = Path.Combine(path, fileName);
            Debug.Log($"读取文件:{path}");
            if (!File.Exists(path)) return string.Empty;

            var content = File.ReadAllText(path);
            return content;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        public static void DeletedFile(string path, string fileName)
        {
            if (!Directory.Exists(path)) return;

            path = Path.Combine(path, fileName);
            Debug.Log($"删除文件:{path}");
            if (!File.Exists(path)) return;
            File.Delete(path);
        }

        /// <summary>
        /// 打开文件夹并选中某个文件
        /// </summary>
        /// <param name="folderPath">文件路径</param>
        /// <param name="fileName">文件夹或文件.格式</param>
        public static void ShowExplorer(string folderPath, string fileName)
        {
#if UNITY_STANDALONE_WIN
            var path = Path.Combine(folderPath, fileName);
            Process.Start("explorer.exe", "/select," + path);
#endif
        }
    }
}