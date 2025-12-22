using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFramework.UI.Editor
{
    public static class UIAutoCreatePathSetting
    {
        /// <summary>
        /// 插件路径
        /// </summary>
        public const string PluginPath = "Assets/3rdBy/MetaFramework/";

        /// <summary>
        /// 说明文档路径
        /// </summary>
        public const string ReadMeFilePath = PluginPath + "UI/Editor/UIAutoCreate/readme.txt";

        /// <summary>
        /// 配置路径
        /// </summary>
        public const string UIConfigPath = PluginPath + "UI/Resources/UIConfig.asset";

        /// <summary>
        /// 预制体模板路径
        /// </summary>
        public const string PrefabTemplatePath = PluginPath + "UI/Resources/UITemplate";

        /// <summary>
        /// 预制体生成路径
        /// </summary>
        public const string PrefabCreatePath = "Assets/Resources/Prefab/UI/";
        
        /// <summary>
        /// View代码生成配置文件路径
        /// </summary>
        public const string UIViewAutoCreateConfigPath = PluginPath + "UI/Editor/UIAutoCreate/Template/UIViewAutoCreateConfig.asset";

        /// <summary>
        /// 代码模板路径
        /// </summary>
        public const string TemplateFilePath = PluginPath + "UI/Editor/UIAutoCreate/Template/";
        public const string ModelTemplateName = "UIModelTemplate.txt";
        public const string ViewTemplateName = "UIViewTemplate.txt";
        public const string ControlTemplateName = "UIControlTemplate.txt";

        /// <summary>
        /// 代码生成路径
        /// </summary>
        public const string GenerateCsFilePath = "Assets/Scripts/UI/";

        /// <summary>
        /// UI代码完整生成路径
        /// </summary>
        /// <returns></returns>
        public static string GetUIGenerateCsFilePath()
        {
            string hotfixPath = Application.dataPath + GenerateCsFilePath;
            hotfixPath = hotfixPath.Replace("/AssetsAssets", "/Assets");
            return hotfixPath;
        }
    }
}