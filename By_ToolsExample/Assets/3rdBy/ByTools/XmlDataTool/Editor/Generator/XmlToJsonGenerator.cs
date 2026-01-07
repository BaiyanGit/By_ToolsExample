/*
 * 作者：王柏雁
 * 日期：2024-8-2
 * 作用：Xml文件导出Json文件并保存至StreamingAssets、Resources文件夹内。
 */

namespace _3rdBy.ByTools.XmlDataTool.Editor.Generator
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using LitJson;

    /// <summary>
    /// Xml文件生成Json文件
    /// </summary>
    public class XmlToJsonGenerator
    {
        /// <summary>
        /// Xml文件生成Json
        /// </summary>
        /// <param name="xmlFilePath"></param>
        public void XmlGenerateToJson(string xmlFilePath, out string json, out string jsonFileName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);
            json = XmlConvertJson(xmlDoc);
            jsonFileName = Path.GetFileNameWithoutExtension(xmlFilePath);
        }

        /// <summary>
        /// Xml内容转换Json内容
        /// </summary>
        /// <param name="xmlDoc">XML文档对象</param>
        /// <returns>转换后的JSON字符串</returns>
        private string XmlConvertJson(XmlDocument xmlDoc)
        {
            XmlNode root = xmlDoc.DocumentElement;
            var jsonArray = new JsonData();

            if (root == null) return JsonMapper.ToJson(jsonArray); // 如果根节点为空，返回空的JSON数组
            foreach (XmlNode node in root.ChildNodes)
            {
                var jsonObject = ParseXmlNode(node); // 解析每个XML节点
                jsonArray.Add(jsonObject); // 将解析后的JSON对象添加到JSON数组中
            }

            // 将编码格式转换UTF-8
            return System.Text.RegularExpressions.Regex.Unescape(JsonMapper.ToJson(jsonArray));
        }

        /// <summary>
        /// 解析Xml内容节点
        /// </summary>
        /// <param name="node">要解析的XML节点</param>
        /// <returns>解析后的JSON对象</returns>
        private static JsonData ParseXmlNode(XmlNode node)
        {
            var jsonObject = new JsonData();

            if (node.Attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    // Debug.Log($"<color=cyan>{attribute.Name}</color>");
                    jsonObject[attribute.Name] = attribute.Value; // 将XML节点的属性添加到JSON对象中
                }
            }

            var childNodeGroups = new Dictionary<string, List<JsonData>>(); // 用于分组子节点

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (!childNodeGroups.ContainsKey(childNode.Name))
                {
                    // Debug.Log($"<color=red>{childNode.Name}</color>");
                    childNodeGroups[childNode.Name] = new List<JsonData>(); // 如果组不存在，创建新的组
                }

                if (childNode.HasChildNodes && childNode.FirstChild is XmlText)
                {
                    // Debug.Log($"<color=green>{childNode.Name}</color>");
                    childNodeGroups[childNode.Name].Add(childNode.InnerText); // 将子节点的文本内容添加到组中
                }
                else
                {
                    // Debug.Log($"<color=yellow>{childNode.Name}</color>");
                    childNodeGroups[childNode.Name].Add(ParseXmlNode(childNode)); // 递归解析子节点
                }
            }

            foreach (var group in childNodeGroups)
            {
                if (group.Value.Count == 1)
                {
                    jsonObject[group.Key] = group.Value[0]; // 如果组中只有一个子节点，直接添加到JSON对象中
                }
                else
                {
                    var jsonArray = new JsonData();
                    foreach (var item in group.Value)
                    {
                        jsonArray.Add(item); // 将组中的子节点列表转换为JSON数组
                    }

                    jsonObject[group.Key] = jsonArray; // 将JSON数组添加到JSON对象中
                }
            }

            return jsonObject; // 返回解析后的JSON对象
        }
    }
}