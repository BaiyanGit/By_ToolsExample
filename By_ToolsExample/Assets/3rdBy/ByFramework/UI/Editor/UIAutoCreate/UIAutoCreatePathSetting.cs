//=====================================================
// 文件名称: UIAutoCreatePathSetting.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: UI 自动生成工具路径配置。
//=====================================================

namespace _3rdBy.ByFramework.UI.Editor.UIAutoCreate
{
    using _3rdBy.ByFramework.Editor;

    /// <summary>
    /// UI 自动生成工具路径配置。
    /// 框架内部路径通过 ByFrameworkPathUtility 动态定位，避免依赖固定安装目录。
    /// </summary>
    public static class UIAutoCreatePathSetting
    {
        /// <summary>
        /// 获取框架根目录路径。
        /// </summary>
        public static string PluginPath => ByFrameworkPathUtility.GetFrameworkRootPath() + "/";

        /// <summary>
        /// 获取 UI 自动生成说明文档路径。
        /// </summary>
        public static string ReadMeFilePath => CombineFromFrameworkRoot("UI/Editor/UIAutoCreate/readme.txt");

        /// <summary>
        /// 获取 UI 配置资源路径。
        /// </summary>
        public static string UIConfigPath => CombineFromFrameworkRoot("UI/Resources/UIConfig.asset");

        /// <summary>
        /// 获取 UI 预制体模板路径，不包含扩展名。
        /// </summary>
        public static string PrefabTemplatePath => CombineFromFrameworkRoot("UI/Resources/UITemplate");

        /// <summary>
        /// UI 预制体生成路径。该路径是业务项目输出位置，不属于框架安装目录。
        /// </summary>
        public const string PrefabCreatePath = "Assets/Resources/Prefab/UI/";

        /// <summary>
        /// 获取 View 代码生成配置文件路径。
        /// </summary>
        public static string UIViewAutoCreateConfigPath => CombineFromFrameworkRoot("UI/Editor/UIAutoCreate/Template/UIViewAutoCreateConfig.asset");

        /// <summary>
        /// 获取代码模板目录路径。
        /// </summary>
        public static string TemplateFilePath => CombineFromFrameworkRoot("UI/Editor/UIAutoCreate/Template") + "/";

        /// <summary>
        /// Model 代码模板文件名。
        /// </summary>
        public const string ModelTemplateName = "UIModelTemplate.txt";

        /// <summary>
        /// View 代码模板文件名。
        /// </summary>
        public const string ViewTemplateName = "UIViewTemplate.txt";

        /// <summary>
        /// Control 代码模板文件名。
        /// </summary>
        public const string ControlTemplateName = "UIControlTemplate.txt";

        /// <summary>
        /// UI 代码生成路径。该路径是业务项目输出位置，不属于框架安装目录。
        /// </summary>
        public const string GenerateCsFilePath = "Assets/Scripts/UI/";

        /// <summary>
        /// 获取 UI 代码完整生成路径。
        /// </summary>
        /// <returns>UI 代码生成目录的本机完整文件系统路径。</returns>
        public static string GetUIGenerateCsFilePath()
        {
            return ByFrameworkPathUtility.ToFullPath(GenerateCsFilePath);
        }

        private static string CombineFromFrameworkRoot(string relativePath)
        {
            return ByFrameworkPathUtility.CombineAssetPath(ByFrameworkPathUtility.GetFrameworkRootPath(), relativePath);
        }
    }
}
