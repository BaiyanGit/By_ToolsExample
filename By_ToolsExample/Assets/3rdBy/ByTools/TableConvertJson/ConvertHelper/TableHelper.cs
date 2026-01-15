namespace _3rdBy.ByTools.TableConvertJson.ConvertHelper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ByFunc.Extension;
    using UnityEditor;
    using UnityEngine;

    public enum SaveJsonPathType
    {
        [InspectorName("StreamingAssets")] StreamingAssets = 0,
        [InspectorName("Resources")] Resources = 1,
        [InspectorName("Both")] Both = 2
    }

    public enum ConvertFileType
    {
        Excel = 0,
        Xml = 1
    }

    public static class TableHelper
    {
        /// <summary>
        /// 获取一个txt扩展的文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string FindExcelDataClassTemplatePath(string fileName, string format = ".txt")
        {
            var fileGuidList     = AssetDatabase.FindAssets(fileName).ToList();
            var filePathList     = fileGuidList.Select(AssetDatabase.GUIDToAssetPath).ToList();
            var templatePathList = filePathList.FindAll(t => t.Contains($"{fileName}{format}"));
            switch (templatePathList.Count)
            {
                case 0:
                    Debug.LogError($"未找到 {fileName}{format} 文件");
                    return string.Empty;
                case 1:
                    return templatePathList[0];
                default:
                {
                    var log = filePathList.Aggregate(string.Empty, (current, path) => current + AssetDatabase.GUIDToAssetPath(path) + "\n");
                    Debug.LogError($"项目中存在多个{fileName}{format}模板文件\n{log}");
                    return string.Empty;
                }
            }
        }

        public static string GetGenerateScriptPath(ConvertFileType fileType)
        {
            string folderPath = $"{Application.dataPath}/Scripts/TableJson";
            var csDir = fileType switch
            {
                ConvertFileType.Excel => $"{folderPath}/Excel/",
                ConvertFileType.Xml   => $"{folderPath}/Xml/",
                _                     => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
            };

            if (!Directory.Exists(csDir)) Directory.CreateDirectory(csDir);

            return csDir;
        }

        public static string GetGenerateAssetPathPath(SaveJsonPathType pathType, ConvertFileType fileType)
        {
            string assetGeneratePath = pathType switch
            {
                SaveJsonPathType.Resources       => $"{Application.dataPath}/Resources",
                SaveJsonPathType.StreamingAssets => Application.streamingAssetsPath,
                _                                => ""
            };

            var assetFolderPath = fileType switch
            {
                ConvertFileType.Excel => $"{assetGeneratePath}/TableJson/ExcelData/",
                ConvertFileType.Xml   => $"{assetGeneratePath}/TableJson/XmlData/",
                _                     => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
            };

            if (!Directory.Exists(assetFolderPath)) Directory.CreateDirectory(assetFolderPath);
            return assetFolderPath;
        }
    }

    /// <summary>
    /// 枚举InspectorName属性
    /// </summary>
    public static class EnumInspectorNameAttribute
    {
        public static string[] GetInspectorNames<T>(this T enumType) where T : Enum
        {
            var type   = typeof(T);
            var values = Enum.GetValues(type);
            var names  = new List<string>();

            foreach (T value in values)
            {
                names.Add(value.GetInspectorName());
            }

            return names.ToArray();
        }
    }
}