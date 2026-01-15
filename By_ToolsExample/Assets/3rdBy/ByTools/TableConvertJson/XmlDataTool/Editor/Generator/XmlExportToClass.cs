/*
 * 作者：王柏雁
 * 日期：2024-8-13
 * 作用：Json文件导出数据类文件并保存至StreamingAssets、Resources文件夹内。
 */

namespace _3rdBy.ByTools.TableConvertJson.XmlDataTool.Editor.Generator
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ConvertHelper;
    using LitJson;
    using UnityEditor;
    using UnityEngine;

    public class XmlExportToClass
    {
        private readonly Dictionary<string, List<string>> _tableData = new();

        private string _classRootName;
        private string _classDataHead;

        public void GenerateClassesFromJson(string json, string className)
        {
            var jsonData = JsonMapper.ToObject(json);
            className      = Path.GetFileNameWithoutExtension(className);
            _classDataHead = className;
            _classRootName = FirstClassNameUpper(className);
            GenerateClassContent(jsonData, className);

            var rootContent  = "";
            var childContent = "";

            foreach (var (key, value) in _tableData)
            {
                if (key.Equals(_classRootName))
                {
                    rootContent = $"{string.Join("\n", value)}";
                }
                else
                {
                    childContent += $"\n[Serializable]\npublic class {key} \n{{\n{string.Join("\n", value)} \n}}\n";
                }
            }

            var rootTemplate = GetClassTemplateContent();
            rootTemplate = rootTemplate.Replace("{0}", className + "Data");
            rootTemplate = rootTemplate.Replace("{1}", className + "Table");
            rootTemplate = rootTemplate.Replace("{2}", rootContent);
            rootTemplate = rootTemplate.Replace("{3}", string.IsNullOrEmpty(childContent) ? "" : childContent);
            SaveClassToFile(className, rootTemplate);
        }


        private static string GetClassTemplateContent(string tempName = "XmlDataClassTemplate")
        {
            var templateFilePath = TableHelper.FindExcelDataClassTemplatePath(tempName);
            if (string.IsNullOrEmpty(templateFilePath)) return string.Empty;
            var templateContent = AssetDatabase.LoadAssetAtPath<TextAsset>(templateFilePath).text;
            // Debug.Log($"模板内容\n{templateContent}");
            return templateContent;
        }

        private void GenerateClassContent(JsonData jsonData, string className)
        {
            foreach (JsonData item in jsonData)
            {
                if (!item.IsObject) continue;
                NodeHandle(item, className);
            }
        }

        private void NodeHandle(JsonData jsonData, string className)
        {
            List<string> dataFields = new();
            foreach (var key in jsonData.Keys)
            {
                var node     = jsonData[key];
                var nodeType = GetTypeFromJsonData(node);
                // Debug.Log(
                //     $"<color=yellow> Type:</color>{nodeType}" +
                //     $"<color=yellow> Key:</color>{key}" +
                //     $"<color=yellow> 值:</color>{node}" +
                //     $"<color=yellow> 类:</color>{node.IsObject}");
                string dataField;
                if (!node.IsObject)
                {
                    if (node.IsArray)
                    {
                        if (IsArray(node))
                        {
                            dataField = $"    public List<string> {key};";
                        }
                        else
                        {
                            // 表示不是数组，是一个表(类)
                            dataField = $"    public List<{FirstClassNameUpper(key)}> {key};";
                            GenerateClassContent(node, key);
                        }
                    }
                    else
                    {
                        dataField = $"    public {nodeType} {key};";
                    }
                }
                else
                {
                    // Tips:是表(类) 则创建类
                    dataField = $"    public {FirstClassNameUpper(key)} {key};"; // 类
                    NodeHandle(node, key);
                }

                if (!dataFields.Contains(dataField) && !string.IsNullOrEmpty(dataField))
                    dataFields.Add(dataField);
            }

            className = FirstClassNameUpper(className);

            if (_tableData.TryAdd(className, dataFields)) return;
            {
                var tempDataFields = _tableData[className];
                foreach (var dataField in dataFields.Where(dataField => !tempDataFields.Contains(dataField)))
                {
                    tempDataFields.Add(dataField);
                }

                _tableData[className] = tempDataFields;
            }
        }

        private static bool IsArray(JsonData jsonData)
        {
            // 只是数组结构，不含其它属性
            return !jsonData.Cast<JsonData>().Any(item => item.IsObject);
        }

        private static string GetTypeFromJsonData(JsonData data)
        {
            if (data.IsInt) return "int";
            if (data.IsLong) return "long";
            if (data.IsDouble) return "double";
            if (data.IsBoolean) return "bool";
            return data.IsString ? "string" : "object";
        }

        private string FirstClassNameUpper(string className)
        {
            return string.IsNullOrEmpty(className)
                       ? "没有类名"
                       : $"Xml{_classDataHead}_{char.ToUpper(className[0])}{className[1..]}";
        }

        private static void SaveClassToFile(string fileName, string classContent)
        {
            string csDir = TableHelper.GetGenerateScriptPath(ConvertFileType.Xml);
            if (!Directory.Exists(csDir)) Directory.CreateDirectory(csDir);
            var filePath = $"{csDir}{fileName}.cs";
            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllText(filePath, classContent, Encoding.UTF8);
        }
    }
}