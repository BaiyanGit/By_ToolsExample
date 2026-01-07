namespace _3rdBy.MetaFramework.UI.Editor.UIAutoCreate
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
            var tempStr = tempTxt.text;

            var nameSpace = "UI.UI" + uiName;
            var className = "UI" + uiName;
            var modelClassName = "UIModel" + uiName;
            var viewClassName = "UIView" + uiName;

            tempStr = tempStr.Replace("{0}", nameSpace);
            tempStr = tempStr.Replace("{1}", className);
            tempStr = tempStr.Replace("{2}", modelClassName);
            tempStr = tempStr.Replace("{3}", viewClassName);

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

        public void SaveFile(string str, string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);

            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(str);
                }
            }
            Debug.Log("创建成功: " + filePath);
            AssetDatabase.Refresh();
        }
    }
}