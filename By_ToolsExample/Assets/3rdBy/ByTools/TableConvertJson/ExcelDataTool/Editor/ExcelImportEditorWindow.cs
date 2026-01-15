namespace _3rdBy.ByTools.TableConvertJson.ExcelDataTool.Editor
{
    using System;
    using System.IO;
    using ConvertHelper;
    using ExcelConvert;
    using ExcelConvert.Convert;
    using UnityEditor;
    using UnityEngine;

    public class ExcelImportEditorWindow : EditorWindow
    {
        private static readonly Vector2 windowSize = new(800, 600);

        private string _pathExcelFile;
        private string _pathExcelFolder;

        [MenuItem("ByTools/🧩 Excel导入工具 #&%E", false, 998)]
        private static void ShowEditor()
        {
            var window = GetWindow<ExcelImportEditorWindow>();
            window.minSize           = windowSize;
            window.maxSize           = windowSize;
            window.titleContent.text = "Excel生成Json工具";
        }

        private int _selectIndex;
        private string[] _outputPath;

        private const string DescStr0 = "<color=yellow><b><size=12>使用方法：</size></b></color>\n" +
                                        "<color=green>1. Excel表需要以\"t_\"前缀开头，后缀格式为xlsm,xlsx.</color>\n" +
                                        "<color=green>2. 只取每个Excel表的第一个sheet.</color>\n" +
                                        "<color=green>3. 第4行：属性名，需要取有意义的名字，最好是英文。每个表必须定义ID属性。如果以‘#’号开头，表示不导出.</color>\n" +
                                        "<color=green>4. 第5行：注释.\n" +
                                        "<color=green>5. 第6行：类型，目前支持的类型有：\"string\", \"int\", \"float\", \"bool\".</color>\n" +
                                        "<color=green>6. 第7行：从第七行往下是数据.\n" +
                                        "<color=green>7. 集合数据的分隔符顺序（从低到高）： '|'  ','  ';',数组的类型为:int[], int[][], int[][][], string[], string[][], string[][][],</color>\n" +
                                        "<color=green>以此类推，目前仅支持3阶数组，后面需要可以再加.</color>\n";

        private const string DescStr1 = "<color=yellow><b><size=12>校验格式方法：</size></b></color>\n" +
                                        "<color=green>打开Unity工程，选择ByTools/Excel导入工具（ Ctrl + Shift + Alt + E ）,点击导出所有表格，看日志查看错误信息</color>\n";

        private void OnEnable()
        {
            _outputPath = Enum.GetNames(typeof(ExcelConvertPathType));
        }

        private void OnGUI()
        {
            #region GUIStyle 设置

            var fontColor = new Color(179f / 255f, 179f / 255f, 179f / 255f, 1f);

            var titleStyle = new GUIStyle { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            titleStyle.normal.textColor = fontColor;

            var sonTittleStyle = new GUIStyle { fontSize = 15, alignment = TextAnchor.MiddleCenter };
            sonTittleStyle.normal.textColor = fontColor;

            var leftStyle = new GUIStyle { fontSize = 15, alignment = TextAnchor.MiddleLeft };
            leftStyle.normal.textColor = fontColor;

            var littoleStyle = new GUIStyle { fontSize = 13, alignment = TextAnchor.MiddleCenter };
            littoleStyle.normal.textColor = fontColor;

            #endregion

            GUILayout.BeginArea(new Rect(0, 0, windowSize.x, windowSize.y));

            GUILayout.BeginVertical();
            var helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                richText = true
            };
            EditorGUILayout.LabelField($"{DescStr0}\n{DescStr1}", helpBoxStyle);
            GUILayout.Space(20);
            //====================导出位置=====================
            GUILayout.BeginHorizontal();
            GUILayout.Label("选择生成位置：", GUILayout.Width(90));
            _selectIndex = EditorGUILayout.Popup(_selectIndex, _outputPath);
            GUILayout.EndHorizontal();
            //================================================

            //===================导出单表======================
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Excel单文件：", GUILayout.Width(90));
            _pathExcelFile = GUILayout.TextField(_pathExcelFile);
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                var path   = string.IsNullOrEmpty(_pathExcelFile) ? GetExcelFolder() : _pathExcelFile;
                var folder = Path.GetDirectoryName(path);
                _pathExcelFile =
                    EditorUtility.OpenFilePanel("Open Excel file", folder, "excel files;*.xls;*.xlsx;*.xlsm");
            }

            if (GUILayout.Button("导出", GUILayout.Width(60)))
            {
                if (string.IsNullOrEmpty(_pathExcelFile))
                {
                    Debug.LogError("pathExcelFile is null");
                }
                else
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    new ExcelExportToClass().Generate(_pathExcelFile);
                    new ExcelExportToAsset().Generate(_pathExcelFile, (SaveJsonPathType)_selectIndex);

                    sw.Stop();
                    Debug.Log("generate excel complete, total time:" + sw.ElapsedMilliseconds);
                }
            }

            GUILayout.EndHorizontal();
            //===================文件夹======================
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Excel文件夹：", GUILayout.Width(90));
            _pathExcelFolder = GUILayout.TextField(_pathExcelFolder);
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                var path   = string.IsNullOrEmpty(_pathExcelFolder) ? GetExcelFolder() : _pathExcelFolder;
                var folder = Path.GetDirectoryName(path);
                _pathExcelFolder = EditorUtility.OpenFolderPanel("Open Excel folder", folder, null);
            }

            if (GUILayout.Button("导出", GUILayout.Width(60)))
            {
                if (string.IsNullOrEmpty(_pathExcelFolder))
                {
                    Debug.LogError("pathExcelFile is null");
                }
                else
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    new ExcelConvertRequest().GenerateAllClass(_pathExcelFolder);
                    new ExcelConvertRequest().GenerateAllAsset(_pathExcelFolder, (SaveJsonPathType)_selectIndex);

                    sw.Stop();
                    Debug.Log("generate excel complete, total time:" + sw.ElapsedMilliseconds);
                }
            }

            GUILayout.EndHorizontal();
            //================================================

            //================导出所有================================
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("导出所有表格", GUILayout.Height(30)))
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                new ExcelConvertRequest().GenerateAllClass(ExcelConvertPathSetting.GetExcelPath());
                new ExcelConvertRequest().GenerateAllAsset(ExcelConvertPathSetting.GetExcelPath(), (SaveJsonPathType)_selectIndex);

                sw.Stop();
                Debug.Log("generate excel complete, total time:" + sw.ElapsedMilliseconds);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private static string GetExcelFolder()
        {
            return ExcelConvertPathSetting.GetExcelPath();
            //return Application.dataPath.Replace("/Assets", "/[TableUtils]/Table-Game/");
        }

        private static GUIStyle TitleStyle()
        {
            var labelStyle = new GUIStyle
            {
                fontSize  = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white }
            };
            return labelStyle;
        }


        /// <summary>
        /// 说明文字标题样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle DescTitleStyle()
        {
            var labelStyle = new GUIStyle
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = Color.white }
            };
            return labelStyle;
        }
    }
}