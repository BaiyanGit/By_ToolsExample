namespace _3rdBy.ByTools.JsonConvertTool.Editor.ConvertHelper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
            var fileGuidList = AssetDatabase.FindAssets(fileName).ToList();


            var filePathList     = fileGuidList.Select(AssetDatabase.GUIDToAssetPath).ToList();
            var templatePathList = filePathList.FindAll(t => t.Contains($"{fileName}{format}"));

            var templatePath = new List<string>();
            // 若文件开头包含~$符号则忽略
            foreach (var filePath in templatePathList)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                if (fileNameWithoutExtension.StartsWith("~$"))
                {
                    continue;
                }

                templatePath.Add(filePath);
            }

            switch (templatePath.Count)
            {
                case 0:
                    Debug.LogError($"未找到 {fileName}{format} 文件");
                    return string.Empty;
                case 1:
                    return templatePath.First();//[0];
                default:
                {
                    var log = filePathList.Aggregate(string.Empty, (current, path) => current + AssetDatabase.GUIDToAssetPath(path) + "\n");
                    Debug.LogError($"项目中存在多个{fileName}{format}模板文件\n{log}");
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 获取生成脚本的路径
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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

        /// <summary>
        /// 获取生成的json文件的路径
        /// </summary>
        /// <param name="pathType"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
}