#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LitJson;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 通用 Json -> 多CSV 导出工具
/// 规则：
/// 1. 任意 Json 都可解析
/// 2. 对象中的普通嵌套字段拍平为 a.b.c
/// 3. 标量数组/多维标量数组写成单元格字符串
/// 4. 对象数组、复杂数组自动拆成子表
/// 5. 每张表都用统一布局：
///    B3 = 表名
///    B4 = 字段名
///    B5 = 描述（预留空）
///    B6 = 类型
///    B7 开始 = 数据
/// </summary>
public class GenericJsonToCsvTool : EditorWindow
{
    private string jsonFilePath = "";
    private string outputFolder = "";

    [MenuItem("Tools/JSON To CSV (Generic)")]
    public static void OpenWindow()
    {
        GetWindow<GenericJsonToCsvTool>("JSON To CSV");
    }

    private void OnGUI()
    {
        GUILayout.Label("通用 JSON 转多CSV 工具（LitJSON）", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        jsonFilePath = EditorGUILayout.TextField("Json 文件", jsonFilePath);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("选择 Json 文件", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                jsonFilePath = path;

                if (string.IsNullOrEmpty(outputFolder))
                {
                    outputFolder = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_csv");
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField("输出目录", outputFolder);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("选择输出目录", Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                outputFolder = path;
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "真正通用的方案不是导出一张表，而是自动拆成多张CSV：\n" +
            "- 普通字段进 root 表\n" +
            "- 对象数组自动拆成子表\n" +
            "- 嵌套对象自动拍平\n" +
            "- 标量数组按 , | ; # 写入单元格\n" +
            "- 不同结构不会崩，不需要每次改代码",
            MessageType.Info);

        GUI.enabled = !string.IsNullOrEmpty(jsonFilePath) && !string.IsNullOrEmpty(outputFolder);
        if (GUILayout.Button("导出", GUILayout.Height(32)))
        {
            try
            {
                GenericJsonToCsvExporter.Export(jsonFilePath, outputFolder);
                Debug.Log("导出成功: " + outputFolder);
                EditorUtility.RevealInFinder(outputFolder);
            }
            catch (Exception ex)
            {
                Debug.LogError("导出失败：\n" + ex);
            }
        }

        GUI.enabled = true;
    }
}

/// <summary>
/// 通用 Json -> 多CSV 导出器
/// </summary>
public static class GenericJsonToCsvExporter
{
    private const int StartRow = 2; // 第3行
    private const int StartCol = 1; // 第2列(B列)

    /// <summary>
    /// 导出入口
    /// </summary>
    public static void Export(string jsonFilePath, string outputFolder)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException("Json 文件不存在：" + jsonFilePath);
        }

        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            throw new Exception("输出目录不能为空");
        }

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string   jsonText = File.ReadAllText(jsonFilePath, Encoding.UTF8);
        JsonData root     = JsonMapper.ToObject(jsonText);

        ExportContext ctx = new ExportContext();
        ctx.SourceFileName = Path.GetFileNameWithoutExtension(jsonFilePath);

        ProcessRoot(root, ctx);

        WriteAllTables(ctx, outputFolder);
        WriteManifest(ctx, outputFolder);
    }

    /// <summary>
    /// 处理根节点
    /// </summary>
    private static void ProcessRoot(JsonData root, ExportContext ctx)
    {
        CsvTable rootTable = ctx.GetOrCreateTable("root");

        // 标量根
        if (IsScalar(root))
        {
            CsvRow row = rootTable.CreateRow(ctx.NextRowId());
            row.Set("value", ScalarToString(root), GetScalarType(root));
            return;
        }

        // 对象根
        if (root != null && root.IsObject)
        {
            CsvRow row = rootTable.CreateRow(ctx.NextRowId());
            ProcessObjectIntoRow(root, rootTable.Name, row, "", ctx);
            return;
        }

        // 数组根
        if (root != null && root.IsArray)
        {
            ProcessArrayIntoTable(root, rootTable.Name, null, null, null, ctx);
            return;
        }

        // 兜底
        CsvRow fallbackRow = rootTable.CreateRow(ctx.NextRowId());
        fallbackRow.Set("value", "", "string");
    }

    /// <summary>
    /// 将对象展开到当前行；嵌套对象拍平，复杂数组拆子表
    /// </summary>
    private static void ProcessObjectIntoRow(JsonData obj, string currentTableName, CsvRow row, string prefix, ExportContext ctx)
    {
        if (obj == null || IsNone(obj) || !obj.IsObject)
        {
            return;
        }

        foreach (string key in obj.Keys)
        {
            JsonData value     = obj[key];
            string   fieldPath = string.IsNullOrEmpty(prefix) ? key : prefix + "." + key;

            if (IsScalar(value))
            {
                row.Set(fieldPath, ScalarToString(value), GetScalarType(value));
            }
            else if (value != null && value.IsObject)
            {
                ProcessObjectIntoRow(value, currentTableName, row, fieldPath, ctx);
            }
            else if (value != null && value.IsArray)
            {
                if (IsScalarOnlyArray(value))
                {
                    row.Set(fieldPath, SerializeScalarArray(value), BuildArrayTypeName(GetArrayDepth(value), InferArrayLeafType(value)));
                }
                else
                {
                    string childTableName = BuildChildTableName(currentTableName, fieldPath);
                    ProcessArrayIntoTable(value, childTableName, row.Id, currentTableName, fieldPath, ctx);
                }
            }
            else
            {
                row.Set(fieldPath, "", "string");
            }
        }
    }

    /// <summary>
    /// 将数组拆成一张表
    /// </summary>
    private static void ProcessArrayIntoTable(
        JsonData array,
        string tableName,
        int? parentId,
        string parentTable,
        string parentPath,
        ExportContext ctx)
    {
        if (array == null || IsNone(array) || !array.IsArray)
        {
            return;
        }

        CsvTable table = ctx.GetOrCreateTable(tableName);

        // 空数组：留空表，不报错
        if (array.Count == 0)
        {
            return;
        }

        for (int i = 0; i < array.Count; i++)
        {
            JsonData item = array[i];
            CsvRow   row  = table.CreateRow(ctx.NextRowId());

            if (parentId.HasValue)
            {
                row.Set("__parent_id", parentId.Value.ToString(CultureInfo.InvariantCulture), "int");
            }

            if (!string.IsNullOrEmpty(parentTable))
            {
                row.Set("__parent_table", parentTable, "string");
            }

            if (!string.IsNullOrEmpty(parentPath))
            {
                row.Set("__parent_path", parentPath, "string");
            }

            row.Set("__index", i.ToString(CultureInfo.InvariantCulture), "int");

            ProcessNodeIntoRow(item, tableName, row, "", ctx);
        }
    }

    /// <summary>
    /// 将任意节点写到某一行里；复杂数组继续拆子表
    /// </summary>
    private static void ProcessNodeIntoRow(JsonData node, string currentTableName, CsvRow row, string prefix, ExportContext ctx)
    {
        string fieldName = string.IsNullOrEmpty(prefix) ? "value" : prefix;

        if (IsScalar(node))
        {
            row.Set(fieldName, ScalarToString(node), GetScalarType(node));
            return;
        }

        if (node != null && node.IsObject)
        {
            ProcessObjectIntoRow(node, currentTableName, row, prefix, ctx);
            return;
        }

        if (node != null && node.IsArray)
        {
            if (IsScalarOnlyArray(node))
            {
                row.Set(fieldName, SerializeScalarArray(node), BuildArrayTypeName(GetArrayDepth(node), InferArrayLeafType(node)));
            }
            else
            {
                string childTableName = BuildChildTableName(currentTableName, string.IsNullOrEmpty(prefix) ? "item" : prefix);
                ProcessArrayIntoTable(node, childTableName, row.Id, currentTableName, fieldName, ctx);
            }

            return;
        }

        row.Set(fieldName, "", "string");
    }

    /// <summary>
    /// 是否是标量
    /// </summary>
    private static bool IsScalar(JsonData data)
    {
        if (data == null)
        {
            return true;
        }

        JsonType type = data.GetJsonType();
        return type == JsonType.None ||
               type == JsonType.String ||
               type == JsonType.Boolean ||
               type == JsonType.Int ||
               type == JsonType.Long ||
               type == JsonType.Double;
    }

    /// <summary>
    /// 是否是 JsonType.None
    /// </summary>
    private static bool IsNone(JsonData data)
    {
        return data == null || data.GetJsonType() == JsonType.None;
    }

    /// <summary>
    /// 是否是纯标量数组 / 纯多维标量数组
    /// 只要里面出现对象，就认为不是纯标量数组
    /// </summary>
    private static bool IsScalarOnlyArray(JsonData data)
    {
        if (data == null || IsNone(data) || !data.IsArray)
        {
            return false;
        }

        for (int i = 0; i < data.Count; i++)
        {
            JsonData child = data[i];

            if (IsScalar(child))
            {
                continue;
            }

            if (child != null && child.IsArray)
            {
                if (!IsScalarOnlyArray(child))
                {
                    return false;
                }

                continue;
            }

            // 出现对象或其他复杂类型
            return false;
        }

        return true;
    }

    /// <summary>
    /// 标量转字符串
    /// </summary>
    private static string ScalarToString(JsonData data)
    {
        if (data == null || IsNone(data))
        {
            return "";
        }

        if (data.IsBoolean)
        {
            return ((bool)data) ? "true" : "false";
        }

        if (data.IsInt)
        {
            return ((int)data).ToString(CultureInfo.InvariantCulture);
        }

        if (data.IsLong)
        {
            return ((long)data).ToString(CultureInfo.InvariantCulture);
        }

        if (data.IsDouble)
        {
            return ((double)data).ToString(CultureInfo.InvariantCulture);
        }

        if (data.IsString)
        {
            return (string)data;
        }

        return JsonMapper.ToJson(data);
    }

    /// <summary>
    /// 获取标量类型
    /// </summary>
    private static string GetScalarType(JsonData data)
    {
        if (data == null || IsNone(data))
        {
            return "string";
        }

        if (data.IsBoolean)
        {
            return "bool";
        }

        if (data.IsInt || data.IsLong)
        {
            return "int";
        }

        if (data.IsDouble)
        {
            return "double";
        }

        if (data.IsString)
        {
            return "string";
        }

        return "string";
    }

    /// <summary>
    /// 获取数组维度
    /// </summary>
    private static int GetArrayDepth(JsonData data)
    {
        if (data == null || IsNone(data) || !data.IsArray)
        {
            return 0;
        }

        int maxDepth = 0;
        for (int i = 0; i < data.Count; i++)
        {
            JsonData child = data[i];
            if (child != null && child.IsArray)
            {
                maxDepth = Math.Max(maxDepth, GetArrayDepth(child));
            }
        }

        return 1 + maxDepth;
    }

    /// <summary>
    /// 推断数组叶子类型
    /// </summary>
    private static string InferArrayLeafType(JsonData data)
    {
        HashSet<string> types = new HashSet<string>();
        CollectArrayLeafTypes(data, types);

        if (types.Count == 0)
        {
            return "unknown";
        }

        if (types.Count == 1)
        {
            foreach (string t in types)
            {
                return t;
            }
        }

        if (types.Count == 2 && types.Contains("int") && types.Contains("double"))
        {
            return "double";
        }

        return "mixed";
    }

    /// <summary>
    /// 收集数组叶子类型
    /// </summary>
    private static void CollectArrayLeafTypes(JsonData data, HashSet<string> types)
    {
        if (data == null || IsNone(data))
        {
            return;
        }

        if (data.IsArray)
        {
            for (int i = 0; i < data.Count; i++)
            {
                CollectArrayLeafTypes(data[i], types);
            }

            return;
        }

        if (IsScalar(data))
        {
            types.Add(GetScalarType(data));
        }
    }

    /// <summary>
    /// 数组序列化成单元格文本
    /// 1维 = ,
    /// 2维 = |
    /// 3维 = ;
    /// 4维及以上 = #
    /// </summary>
    private static string SerializeScalarArray(JsonData data)
    {
        if (data == null || IsNone(data))
        {
            return "";
        }

        if (!data.IsArray)
        {
            return EscapeArrayScalar(ScalarToString(data));
        }

        int    depth     = GetArrayDepth(data);
        string delimiter = GetDelimiterByDepth(depth);

        List<string> parts = new List<string>();
        for (int i = 0; i < data.Count; i++)
        {
            JsonData child = data[i];

            if (child != null && child.IsArray)
            {
                parts.Add(SerializeScalarArray(child));
            }
            else
            {
                parts.Add(EscapeArrayScalar(ScalarToString(child)));
            }
        }

        return string.Join(delimiter, parts);
    }

    /// <summary>
    /// 数组分隔符
    /// </summary>
    private static string GetDelimiterByDepth(int depth)
    {
        switch (depth)
        {
            case 1:  return ",";
            case 2:  return "|";
            case 3:  return ";";
            default: return "#";
        }
    }

    /// <summary>
    /// 数组字符串转义
    /// </summary>
    private static string EscapeArrayScalar(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return value
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace("|", "\\|")
            .Replace(";", "\\;")
            .Replace("#", "\\#")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    /// <summary>
    /// 构建数组类型名
    /// </summary>
    private static string BuildArrayTypeName(int dimension, string leafType)
    {
        string dimName;
        switch (dimension)
        {
            case 1:
                dimName = "一维数组";
                break;
            case 2:
                dimName = "二维数组";
                break;
            case 3:
                dimName = "三维数组";
                break;
            default:
                dimName = dimension + "维数组";
                break;
        }

        return dimName + "<" + leafType + ">";
    }

    /// <summary>
    /// 生成子表名称
    /// 例如 root + episodes.list -> root__episodes__list
    /// </summary>
    private static string BuildChildTableName(string currentTableName, string fieldPath)
    {
        string safe = fieldPath.Replace(".", "__");
        safe = SanitizeName(safe);
        return currentTableName + "__" + safe;
    }

    /// <summary>
    /// 写出全部表
    /// </summary>
    private static void WriteAllTables(ExportContext ctx, string outputFolder)
    {
        for (int i = 0; i < ctx.TableOrder.Count; i++)
        {
            string   tableName = ctx.TableOrder[i];
            CsvTable table     = ctx.Tables[tableName];

            string fileName = ctx.SourceFileName + "_" + tableName + ".csv";
            fileName = SanitizeName(fileName);

            string path = Path.Combine(outputFolder, fileName);
            WriteTableCsv(table, path);
        }
    }

    /// <summary>
    /// 写 manifest
    /// </summary>
    private static void WriteManifest(ExportContext ctx, string outputFolder)
    {
        List<List<string>> rows = new List<List<string>>();

        rows.Add(new List<string>());
        rows.Add(new List<string>());

        List<string> row3 = new List<string>() { "", "manifest" };
        rows.Add(row3);

        List<string> row4 = new List<string>() { "", "table_name", "file_name", "row_count" };
        rows.Add(row4);

        List<string> row5 = new List<string>() { "", "", "", "" };
        rows.Add(row5);

        List<string> row6 = new List<string>() { "", "string", "string", "int" };
        rows.Add(row6);

        for (int i = 0; i < ctx.TableOrder.Count; i++)
        {
            string   tableName = ctx.TableOrder[i];
            CsvTable table     = ctx.Tables[tableName];
            string   fileName  = SanitizeName(ctx.SourceFileName + "_" + tableName + ".csv");

            rows.Add(new List<string>()
            {
                "",
                tableName,
                fileName,
                table.Rows.Count.ToString(CultureInfo.InvariantCulture)
            });
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < rows.Count; i++)
        {
            sb.AppendLine(ToCsvLine(rows[i]));
        }

        string manifestPath = Path.Combine(outputFolder, "__manifest.csv");
        File.WriteAllText(manifestPath, sb.ToString(), new UTF8Encoding(true));
    }

    /// <summary>
    /// 写单张表 CSV
    /// </summary>
    private static void WriteTableCsv(CsvTable table, string csvFilePath)
    {
        List<string>               fieldOrder = ResolveColumnOrder(table);
        Dictionary<string, string> fieldTypes = ResolveColumnTypes(table, fieldOrder);

        List<List<string>> rows = new List<List<string>>();

        // 空行 1,2
        rows.Add(new List<string>());
        rows.Add(new List<string>());

        // B3 = 表名
        List<string> row3 = CreateOffsetRow(fieldOrder.Count);
        row3[1] = table.Name;
        rows.Add(row3);

        // B4 = 字段名
        List<string> row4 = CreateOffsetRow(fieldOrder.Count);
        for (int i = 0; i < fieldOrder.Count; i++)
        {
            row4[i + 1] = fieldOrder[i];
        }

        rows.Add(row4);

        // B5 = 描述
        List<string> row5 = CreateOffsetRow(fieldOrder.Count);
        rows.Add(row5);

        // B6 = 类型
        List<string> row6 = CreateOffsetRow(fieldOrder.Count);
        for (int i = 0; i < fieldOrder.Count; i++)
        {
            row6[i + 1] = fieldTypes[fieldOrder[i]];
        }

        rows.Add(row6);

        // B7 开始 = 数据
        for (int r = 0; r < table.Rows.Count; r++)
        {
            CsvRow       tableRow = table.Rows[r];
            List<string> dataRow  = CreateOffsetRow(fieldOrder.Count);

            for (int c = 0; c < fieldOrder.Count; c++)
            {
                string field = fieldOrder[c];
                if (tableRow.Cells.TryGetValue(field, out CsvCell cell))
                {
                    dataRow[c + 1] = cell.Value;
                }
                else
                {
                    dataRow[c + 1] = "";
                }
            }

            rows.Add(dataRow);
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < rows.Count; i++)
        {
            sb.AppendLine(ToCsvLine(rows[i]));
        }

        File.WriteAllText(csvFilePath, sb.ToString(), new UTF8Encoding(true));
    }

    /// <summary>
    /// 统一列顺序
    /// </summary>
    private static List<string> ResolveColumnOrder(CsvTable table)
    {
        List<string> ordered = new List<string>();

        string[] systemFields = { "__id", "__parent_id", "__parent_table", "__parent_path", "__index" };

        for (int i = 0; i < systemFields.Length; i++)
        {
            if (table.Columns.Contains(systemFields[i]))
            {
                ordered.Add(systemFields[i]);
            }
        }

        for (int i = 0; i < table.ColumnOrder.Count; i++)
        {
            string col = table.ColumnOrder[i];
            if (!ordered.Contains(col))
            {
                ordered.Add(col);
            }
        }

        return ordered;
    }

    /// <summary>
    /// 统一列类型
    /// </summary>
    private static Dictionary<string, string> ResolveColumnTypes(CsvTable table, List<string> fieldOrder)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        for (int i = 0; i < fieldOrder.Count; i++)
        {
            string          field   = fieldOrder[i];
            HashSet<string> typeSet = new HashSet<string>();

            for (int r = 0; r < table.Rows.Count; r++)
            {
                if (table.Rows[r].Cells.TryGetValue(field, out CsvCell cell))
                {
                    if (!string.IsNullOrEmpty(cell.Type))
                    {
                        typeSet.Add(cell.Type);
                    }
                }
            }

            if (typeSet.Count == 0)
            {
                result[field] = "string";
            }
            else if (typeSet.Count == 1)
            {
                foreach (string t in typeSet)
                {
                    result[field] = t;
                }
            }
            else if (typeSet.Count == 2 && typeSet.Contains("int") && typeSet.Contains("double"))
            {
                result[field] = "double";
            }
            else
            {
                result[field] = "mixed";
            }
        }

        return result;
    }

    /// <summary>
    /// 创建从B列开始的行
    /// </summary>
    private static List<string> CreateOffsetRow(int fieldCount)
    {
        List<string> row       = new List<string>();
        int          totalCols = fieldCount + 1; // A列留空，B列开始

        for (int i = 0; i < totalCols; i++)
        {
            row.Add("");
        }

        return row;
    }

    /// <summary>
    /// 行转 CSV
    /// </summary>
    private static string ToCsvLine(List<string> cells)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < cells.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(",");
            }

            sb.Append(EscapeCsv(cells[i]));
        }

        return sb.ToString();
    }

    /// <summary>
    /// CSV 转义
    /// </summary>
    private static string EscapeCsv(string value)
    {
        if (value == null)
        {
            value = "";
        }

        bool needQuote =
            value.Contains(",") ||
            value.Contains("\"") ||
            value.Contains("\r") ||
            value.Contains("\n");

        value = value.Replace("\"", "\"\"");
        return needQuote ? "\"" + value + "\"" : value;
    }

    /// <summary>
    /// 文件名安全处理
    /// </summary>
    private static string SanitizeName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "unnamed";
        }

        char[] invalid = Path.GetInvalidFileNameChars();
        for (int i = 0; i < invalid.Length; i++)
        {
            name = name.Replace(invalid[i], '_');
        }

        name = name.Replace(" ", "_");
        return name;
    }

    /// <summary>
    /// 导出上下文
    /// </summary>
    private sealed class ExportContext
    {
        public string SourceFileName = "json";
        public readonly Dictionary<string, CsvTable> Tables = new Dictionary<string, CsvTable>();
        public readonly List<string> TableOrder = new List<string>();

        private int nextRowId = 1;

        public int NextRowId()
        {
            return nextRowId++;
        }

        public CsvTable GetOrCreateTable(string tableName)
        {
            if (!Tables.TryGetValue(tableName, out CsvTable table))
            {
                table = new CsvTable(tableName);
                Tables.Add(tableName, table);
                TableOrder.Add(tableName);
            }

            return table;
        }
    }

    /// <summary>
    /// 表
    /// </summary>
    private sealed class CsvTable
    {
        public readonly string Name;
        public readonly List<CsvRow> Rows = new List<CsvRow>();
        public readonly HashSet<string> Columns = new HashSet<string>();
        public readonly List<string> ColumnOrder = new List<string>();

        public CsvTable(string name)
        {
            Name = name;
        }

        public CsvRow CreateRow(int id)
        {
            CsvRow row = new CsvRow(this, id);
            Rows.Add(row);
            row.Set("__id", id.ToString(CultureInfo.InvariantCulture), "int");
            return row;
        }

        public void RegisterColumn(string columnName)
        {
            if (!Columns.Contains(columnName))
            {
                Columns.Add(columnName);
                ColumnOrder.Add(columnName);
            }
        }
    }

    /// <summary>
    /// 行
    /// </summary>
    private sealed class CsvRow
    {
        private readonly CsvTable table;
        public readonly int Id;
        public readonly Dictionary<string, CsvCell> Cells = new Dictionary<string, CsvCell>();

        public CsvRow(CsvTable table, int id)
        {
            this.table = table;
            this.Id    = id;
        }

        public void Set(string field, string value, string type)
        {
            table.RegisterColumn(field);
            Cells[field] = new CsvCell(value ?? "", string.IsNullOrEmpty(type) ? "string" : type);
        }
    }

    /// <summary>
    /// 单元格
    /// </summary>
    private struct CsvCell
    {
        public string Value;
        public string Type;

        public CsvCell(string value, string type)
        {
            Value = value;
            Type  = type;
        }
    }
}
#endif