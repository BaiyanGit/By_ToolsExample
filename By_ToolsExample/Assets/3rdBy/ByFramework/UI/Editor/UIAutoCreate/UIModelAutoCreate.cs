namespace _3rdBy.ByFramework.UI.Editor.UIAutoCreate
{
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public abstract class UIModelAutoCreate
    {
        public static void Create(string uiName, string templatePath, string targetPath)
        {
            var tempTxt = AssetDatabase.LoadAssetAtPath<TextAsset>(templatePath);
            var tempStr = tempTxt.text;

            // Tips：暂时不支持使用命名空间
            // var modelSpaceName = "UIMetaFramework" + uiName;
            // tempStr = tempStr.Replace("{0}", modelSpaceName);
            var modelClassName = "UIModel" + uiName;
            tempStr = tempStr.Replace("{0}", modelClassName);

            var filePath = targetPath + modelClassName + ".cs";

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