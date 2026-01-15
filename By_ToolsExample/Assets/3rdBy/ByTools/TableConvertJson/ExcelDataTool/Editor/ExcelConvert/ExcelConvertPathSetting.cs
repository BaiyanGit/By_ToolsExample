namespace _3rdBy.ByTools.TableConvertJson.ExcelDataTool.Editor.ExcelConvert
{
    using System.IO;
    using ConvertHelper;
    using UnityEngine;

    public enum ExcelConvertPathType : int
    {
        Resources = 0,
        StreamingAssets = 1,
        Both = 2
    }

    public static class ExcelConvertPathSetting
    {
        /// <summary>
        /// 插件路径
        /// </summary>
        // public const string PluginPath = "Assets/3rdBy/ByTools/ExcelDataTool/";

        /// <summary>
        /// 模板路径
        /// </summary>
        // public const string ExcelTemplateFilePath = PluginPath + "Editor/ExcelConvert/Template/ExcelDataClassTemplate.txt";

        /// <summary>
        /// 代码生成路径
        /// </summary>
        // public const string GenerateCSFilePath = "Assets/Scripts/ExcelData/";

        /// <summary>
        /// asset数据生成路径
        /// </summary>
        // public const string ASSET_OUTPUT_PATH = "Assets/Resources/ExcelData/";

        /// <summary>
        /// asset数据生成在Streaming Assets下的路径
        /// </summary>
        // public const string ASSET_OUTPUT_StreamingPATH = "Assets/StreamingAssets/ExcelData/";


        /// <summary>
        /// Excel表格路径
        /// </summary>
        /// <returns></returns>
        public static string GetExcelPath()
        {
            // path: ../../design/config/
            string excelPath = Directory.CreateDirectory(Application.dataPath).Parent.FullName + "\\Excel\\";

            return excelPath;
        }

        /// <summary>
        /// Excel代码完整生成路径
        /// </summary>
        /// <returns></returns>
        // public static string GetExcelGenerateCSFilePath()
        // {
        //     string hotfixPath = Application.dataPath + GenerateCSFilePath;
        //     hotfixPath = hotfixPath.Replace("/AssetsAssets", "/Assets");
        //     return hotfixPath;
        // }

        /// <summary>
        /// asset数据完整生成路径
        /// </summary>
        /// <returns></returns>
        // public static string GetExcelGenerateAssetFilePath(SaveJsonPathType pathType)
        // {
        //     string assetGeneratePath = pathType switch
        //     {
        //         SaveJsonPathType.Resources       => Application.dataPath + ASSET_OUTPUT_PATH,
        //         SaveJsonPathType.StreamingAssets => Application.dataPath + ASSET_OUTPUT_StreamingPATH,
        //         _                                => ""
        //     };
        //
        //     assetGeneratePath = assetGeneratePath.Replace("/AssetsAssets", "/Assets");
        //     return assetGeneratePath;
        // }
    }
}