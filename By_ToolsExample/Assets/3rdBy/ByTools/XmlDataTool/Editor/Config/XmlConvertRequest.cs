/*
 * 作者：王柏雁
 * 日期：2024-8-3
 * 作用：获取文件夹下的xml文件类
 */

namespace _3rdBy.ByTools.XmlDataTool.Editor.Config
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// 获取Xml文件
    /// </summary>
    public static class XmlConvertRequest
    {
        /// <summary>
        /// 获取所有xml文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> GetAllFilesAtPath(string path)
        {
            var list = new List<string>();

            if (!Directory.Exists(path)) return null;

            foreach (var filePath in Directory.GetFiles(path))
            {
                if (filePath.Contains("~")) continue;

                var ext = Path.GetExtension(filePath);
                if (ValidExtension(ext))
                {
                    list.Add(filePath);
                }
            }

            foreach (var folderPath in Directory.GetDirectories(path))
            {
                var childList = GetAllFilesAtPath(folderPath);
                list.AddRange(childList);
            }

            return list;
        }

        /// <summary>
        /// 根据格式筛选文件
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static bool ValidExtension(string extension)
        {
            var extensionArray = new[] { ".xml" };
            return extensionArray.Any(extension.Equals);
        }
    }
}