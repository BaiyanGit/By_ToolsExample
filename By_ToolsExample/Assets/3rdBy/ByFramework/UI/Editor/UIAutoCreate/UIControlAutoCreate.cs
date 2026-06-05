namespace _3rdBy.ByFramework.UI.Editor.UIAutoCreate
{
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public class UIControlAutoCreate
    {
        public void Create(string uiName, string templatePath, string targetPath)
        {
            var tempTxt = AssetDatabase.LoadAssetAtPath<TextAsset>(templatePath);
            if (tempTxt == null)
            {
                Debug.LogError(
                    $"[ByFramework][UIAutoCreate] Control 代码生成失败：无法加载 Control 模板。\n实际路径：{templatePath}\n修复建议：确认 UIControlTemplate.txt 存在，并检查 UIAutoCreatePathSetting.TemplateFilePath。");
                return;
            }

            var tempStr = tempTxt.text;

            // var nameSpace = "UIMetaFramework" + uiName;
            var className      = "UI" + uiName;
            var modelClassName = "UIModel" + uiName;
            var viewClassName  = "UIView" + uiName;

            // Tips：暂时不支持使用命名空间
            // tempStr = tempStr.Replace("{0}", nameSpace);
            // tempStr = tempStr.Replace("{1}", className);
            // tempStr = tempStr.Replace("{2}", modelClassName);
            // tempStr = tempStr.Replace("{3}", viewClassName);


            tempStr = tempStr.Replace("{0}", className);
            tempStr = tempStr.Replace("{1}", modelClassName);
            tempStr = tempStr.Replace("{2}", viewClassName);

            var filePath = targetPath + className + ".cs";

            if (File.Exists(filePath))
            {
                if (EditorUtility.DisplayDialog("警告", "检测到脚本，是否覆盖", "确定", "取消"))
                {
                    SaveFile(tempStr, filePath);
                }
            }
            else
            {
                SaveFile(tempStr, filePath);
            }
        }

        private static void SaveFile(string str, string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);

            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(str);
                }
            }

            Debug.Log("创建成功: " + filePath);
            AssetDatabase.Refresh();
        }
    }
}