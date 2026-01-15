namespace _3rdBy.ByTools.TableConvertJson.Editor.Excel.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using UnityEngine;

    public class ExcelMiddleData
    {
        [Header("Excel路径")] public string excelPath;
        [Header("表名")] public string sheetName;
        [Header("实际行数")] private int _realRowCount;
        [Header("实际列数")] private int _realColumnCount;

        [Header("类型")] public List<string> types;
        [Header("属性")] public List<string> props;
        [Header("注释")] public List<string[]> notes;
        [Header("实际列")] public List<int> realColumns;
        [Header("数据")] private List<SortedDictionary<string, string>> _excelMiddleDatas;

        [Header("开始行")] private const int ExcelRowIndexStart = 3;
        [Header("注释行")] private const int ExcelRowIndexNote = 4;
        [Header("类型行")] private const int ExcelRowIndexType = 5;
        [Header("内容开始行")] private const int ExcelRowIndexContentStart = 6;
        [Header("开始列")] private const int ExcelColumnIndexStart = 1;

        public void Init(DataTable sheet, string excelFilePath)
        {
            excelPath         = excelFilePath;
            sheetName         = sheet.TableName;
            _realRowCount     = sheet.Rows.Count;
            _realColumnCount  = sheet.Columns.Count;
            types             = new List<string>();
            props             = new List<string>();
            notes             = new List<string[]>();
            realColumns       = new List<int>();
            _excelMiddleDatas = new List<SortedDictionary<string, string>>();

            bool haveIdProp = false;

            // 初始化标题
            for (int i = 0; i < _realColumnCount; i++)
            {
                var prop = sheet.Rows[ExcelRowIndexStart][i].ToString();
                if (string.IsNullOrEmpty(prop)) continue;
                if (prop.StartsWith("#")) continue;
                if (prop.ToLower().Equals("id")) haveIdProp = true;

                var note      = sheet.Rows[ExcelRowIndexNote][i].ToString();
                var noteArray = note.Split('\n');

                var type = sheet.Rows[ExcelRowIndexType][i].ToString();
                if (string.IsNullOrEmpty(type))
                    throw new Exception("type is null:【" + prop + "】检测到" + (ExcelRowIndexType + 1) + "行类型为空（可以通知程序加上）, path: " + excelFilePath);
                if (!CheckTypeValid(type))
                    throw new Exception("type error:【" + type + "】检测到类型未定义, path: " + excelFilePath);

                //if (props.Equals("DIYData")) type = GetFormatType(type);

                types.Add(type);
                props.Add(prop);
                notes.Add(noteArray);
                realColumns.Add(i);
            }

            if (!haveIdProp) throw new Exception("id not define,未包含Id字段, path:" + excelFilePath);

            // 转换数据
            for (int i = ExcelRowIndexContentStart; i < _realRowCount; i++)
            {
                if (string.IsNullOrEmpty(sheet.Rows[i][ExcelColumnIndexStart].ToString())) continue;

                var dic = new SortedDictionary<string, string>();
                for (int j = 0; j < realColumns.Count; j++)
                {
                    var realColumn = realColumns[j];
                    var propTitle  = props[j];
                    var propType   = types[j];
                    var propValue  = sheet.Rows[i][realColumn].ToString();

                    // 空值错误
                    if (string.IsNullOrEmpty(propValue))
                    {
                        Debug.LogWarning($"值为空,title:【{propTitle}】, row:【{i + 1}】, path:" + excelFilePath);
                        //throw new Exception($"值为空,title:【{propTitle}】, row:【{i + 1}】, path:" + excelPath);
                    }

                    // 空值处理
                    if (propValue.Equals("-")) propValue = "";

                    // 数组类型
                    if (propType.Contains("[]"))
                    {
                        try
                        {
                            propValue = propValue.Equals("-") ? "" : ArrayFormatConvert(propValue, propType);
                        }
                        catch
                        {
                            Debug.LogError($"array error,数组错误 title:【{propTitle}】, row:【{i + 1}】, value:【{propValue}】, path:【{excelFilePath}】");
                        }
                    }

                    if (!dic.TryAdd(propTitle, propValue))
                    {
                        Debug.LogError("title repeat:【" + propTitle + "】title重复定义, path: " + excelFilePath);
                    }
                }

                _excelMiddleDatas.Add(dic);
            }
        }

        /// <summary>
        /// 校验数据类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool CheckTypeValid(string type)
        {
            string[] validType = { "string", "int", "float", "bool" };
            for (int i = 0; i < validType.Length; i++)
            {
                if (validType[i].Equals(type)) return true;
            }

            //if (type.Contains("CustomED")) return true;
            return type.Contains("[]");
        }

        /// <summary>
        /// 将Excel表中的格式转为标准Json格式
        /// </summary>
        /// <param name="originStr"></param>
        /// <returns></returns>
        private string ArrayFormatConvert(string originStr, string originType)
        {
            var finalString = SplitWithChar(originStr, originType, ';', str => { return SplitWithChar(str, originType, ',', strValue => SplitWithChar(strValue, originType, '|', null)); });
            return finalString;

            //StringBuilder sb = new StringBuilder();

            //var strs1 = originStr.Split(',');
            //if (strs1.Length > 1) sb.Append('[');

            //for (int i = 0; i < strs1.Length; i++)
            //{
            //    var str2 = strs1[i].Split('|');
            //    if (str2.Length > 1) sb.Append('[');

            //    for (int j = 0; j < str2.Length; j++)
            //    {
            //        sb.Append(str2[j]);
            //        if (j < str2.Length - 1) sb.Append(',');
            //    }

            //    if (str2.Length > 1) sb.Append(']');
            //    if (i < strs1.Length - 1) sb.Append(',');
            //}

            //if (strs1.Length > 1) sb.Append(']');

            //return sb.ToString();
        }

        private string SplitWithChar(string originStr, string originType, char splitChar, Func<string, string> funcSplit)
        {
            var sb = new StringBuilder();

            var strs = originStr.Split(splitChar);
            if (strs.Length > 1 || CheckTypeAndSplit(originType, splitChar)) sb.Append('[');

            for (int i = 0; i < strs.Length; i++)
            {
                string endStr = null;
                if (funcSplit == null)
                {
                    var type = originType.Replace("[]", "");
                    endStr = GetJsonTypeValue(type, strs[i]);
                }
                else
                {
                    endStr = funcSplit(strs[i]);
                }

                if (!string.IsNullOrEmpty(endStr)) sb.Append(endStr);
                if (i < strs.Length - 1) sb.Append(',');
            }

            if (strs.Length > 1 || CheckTypeAndSplit(originType, splitChar)) sb.Append(']');
            return sb.ToString();
        }

        private bool CheckTypeAndSplit(string propType, char splitChar)
        {
            if (propType.Contains("[][][]"))
            {
                if (splitChar.Equals('|')
                    || splitChar.Equals(',')
                    || splitChar.Equals(';'))
                {
                    return true;
                }
            }
            else if (propType.Contains("[][]"))
            {
                if (splitChar.Equals('|')
                    || splitChar.Equals(','))
                {
                    return true;
                }
            }
            else if (propType.Contains("[]"))
            {
                if (splitChar.Equals('|')) return true;
            }

            return false;
        }


        /// <summary>
        /// 转为json
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            var sb = new StringBuilder();
            sb.Append("{\"dataList\":[");

            for (int i = 0; i < _excelMiddleDatas.Count; i++)
            {
                sb.Append("{");

                var dicData = _excelMiddleDatas[i];
                for (int j = 0; j < realColumns.Count; j++)
                {
                    var propTitle = props[j];
                    var propType  = types[j];
                    var propValue = dicData[propTitle];
                    propValue = GetJsonTypeValue(propType, propValue);

                    sb.Append($"\"{propTitle}\": {propValue}");
                    if (j < realColumns.Count - 1) sb.Append(",");
                }

                sb.Append("}");
                if (i < _excelMiddleDatas.Count - 1) sb.Append(",");
            }

            sb.Append("]}");

            return sb.ToString();
        }

        private string GetJsonTypeValue(string type, string value)
        {
            if (type.Equals("string")) return $"\"{value}\"";

            return string.IsNullOrEmpty(value) ? "null" : value;
        }
    }
}