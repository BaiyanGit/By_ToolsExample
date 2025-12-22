/*
 * 作者：王柏雁
 * 日期：2024-8-2
 * 作用：文件保存路径管理类
 */

namespace _3rdBy.UniTools.XmlDataTool.Editor.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 导出方式
    /// </summary>
    public enum ExportWay : int
    {
        Single = 0,
        Folder = 1
    }

    /// <summary>
    /// 导出文件夹
    /// </summary>
    public enum XmlConvertFolderType : int
    {
        StreamingAssets = 0,
        Resources = 1,
        Both = 2
    }

    public static class XmlConvertPathSetting
    {
        private const string TemplateName = "XmlDataClassTemplate";

        /// <summary>
        /// 获取文件路径
        /// </summary>
        /// <returns></returns>
        public static string GetFilePath(string fileName, string format = ".txt")
        {
            var fileGuidList = AssetDatabase.FindAssets(fileName).ToList();
            var filePathList = fileGuidList.Select(AssetDatabase.GUIDToAssetPath).ToList();
            var xmlTemplatePathList = filePathList.FindAll(t => t.Contains($"{fileName}{format}"));
            switch (xmlTemplatePathList.Count)
            {
                case 0:
                    Debug.LogError($"未找到 {fileName}{format} 文件");
                    return string.Empty;
                case 1:
                    return xmlTemplatePathList[0];
                default:
                {
                    var log = filePathList.Aggregate(string.Empty,
                        (current, path) => current + AssetDatabase.GUIDToAssetPath(path) + "\n");
                    Debug.LogError($"项目中存在多个文件\n{log}");
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 获取存储xml文件的默认文件夹
        /// </summary>
        /// <returns></returns>
        public static string GetXmlFileDefaultFolder()
        {
            var xmlFolder = Directory.CreateDirectory(Application.dataPath).Parent + @"\Xml\";
            return xmlFolder;
        }

        /// <summary>
        /// 获取xml生成.cs脚本存放的文件夹
        /// </summary>
        /// <returns></returns>
        public static string GetXmlClassFolder()
        {
            var combine = Path.Combine(Application.dataPath, "Assets/Scripts/XmlData/");
            return combine;
        }

        /// <summary>
        /// 获取Xml转换Json资源保存文件夹
        /// </summary>
        /// <param name="folderType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetXmlConvertJsonSaveFolder(XmlConvertFolderType folderType)
        {
            var folder = folderType switch
            {
                XmlConvertFolderType.StreamingAssets => Path.Combine(Application.streamingAssetsPath, "XmlData/"),
                XmlConvertFolderType.Resources => Path.Combine(Application.dataPath, "Resources/XmlData/"),
                _ => throw new ArgumentOutOfRangeException(nameof(folderType), folderType, null)
            };
            return folder;
        }

        /// <summary>
        /// 生成类资源的路径
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string GetOutputPath(string className)
        {
            var folder = Path.Combine(Application.dataPath, "XmlScripts/");
            CreateFolder(folder);
            return $"{folder}{className}.cs";
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="directoryPath"></param>
        private static void CreateFolder(string directoryPath)
        {
            if (Directory.Exists(directoryPath)) return;
            Directory.CreateDirectory(directoryPath);
        }

        /// <summary>
        /// 删除已存在的文件(带有文件格式)
        /// </summary>
        /// <param name="filePath"></param>
        public static void DeletedExistsFile(string filePath)
        {
            if (!File.Exists(filePath)) return;
            File.Delete(filePath);
        }

        /// <summary>
        /// 获取类模板的内容
        /// </summary>
        public static string GetClassTemplateContent(string tempName = TemplateName)
        {
            var templateFilePath = GetFilePath(tempName);
            if (string.IsNullOrEmpty(templateFilePath)) return string.Empty;
            var templateContent = AssetDatabase.LoadAssetAtPath<TextAsset>(templateFilePath).text;
            // Debug.Log($"模板内容\n{templateContent}");
            return templateContent;
        }

        /// <summary>
        /// 保存Json文件至相关目录下
        /// </summary>
        /// <param name="jsonContent"></param>
        /// <param name="fileName"></param>
        /// <param name="folderType"></param>
        public static void SaveJsonToFile(string jsonContent, string fileName, XmlConvertFolderType folderType)
        {
            var savePaths = GetSaveJsonPath(folderType);
            fileName = $"{fileName}.json";
            foreach (var fileSavePath in savePaths.Select(path => Path.Combine(path, fileName)))
            {
                DeletedExistsFile(fileSavePath);
                File.WriteAllText(fileSavePath, jsonContent, Encoding.UTF8);
                // Debug.Log($"JSON saved to: {fileSavePath}");
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 获取保存Json文件的路径
        /// </summary>
        /// <param name="folderType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static List<string> GetSaveJsonPath(XmlConvertFolderType folderType)
        {
            List<string> pathList = new();

            switch (folderType)
            {
                case XmlConvertFolderType.Resources:
                {
                    addAndCheckPath(XmlConvertFolderType.Resources);
                    break;
                }
                case XmlConvertFolderType.StreamingAssets:
                {
                    addAndCheckPath(XmlConvertFolderType.StreamingAssets);
                    break;
                }
                case XmlConvertFolderType.Both:
                {
                    addAndCheckPath(XmlConvertFolderType.Resources);
                    addAndCheckPath(XmlConvertFolderType.StreamingAssets);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(folderType), folderType, null);
            }

            return pathList;

            void addAndCheckPath(XmlConvertFolderType type)
            {
                var path = GetXmlConvertJsonSaveFolder(type);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                pathList.Add(path);
            }
        }
    }
}