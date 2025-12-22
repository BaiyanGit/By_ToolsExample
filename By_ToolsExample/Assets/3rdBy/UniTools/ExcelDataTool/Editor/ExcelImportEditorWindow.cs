namespace _3rdBy.UniTools.ExcelDataTool.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using XFramework.ExcelData.Editor;

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
            window.titleContent.text = "Excel导入工具";
        }

        private int _selectIndex;
        private string[] _outputPath;

        private string _descStr0 = "1. Excel表需要以\"t_\"前缀开头，后缀格式为xlsm,xlsx.\n" +
                                   "2. 只取每个Excel表的第一个sheet.\n" +
                                   "3. 第4行：属性名，需要取有意义的名字，最好是英文。每个表必须定义ID属性。如果以‘#’号开头，表示不导出.\n" +
                                   "4. 第5行：注释.\n" +
                                   "5. 第6行：类型，目前支持的类型有：\"string\", \"int\", \"float\", \"bool\".\n" +
                                   "6. 第7行：从第七行往下是数据.\n" +
                                   "7. 集合数据的分隔符顺序（从低到高）： '|'  ','  ';',数组的类型为:int[], int[][], int[][][], string[], string[][], string[][][], \n" +
                                   "以此类推，目前仅支持3阶数组，后面需要可以再加.\n";

        private string _descStr1 = "校验格式方法：\n" +
                                   "打开Unity工程，选择ByTools/Excel导入工具（ Ctrl + Shift + Alt + E ）,点击导出所有表格，看日志查看错误信息";

        private void OnEnable()
        {
            _outputPath = Enum.GetNames(typeof(ExcelConvertPathType));
        }

        private void OnGUI()
        {
            #region GUIStyle 设置

            var fontColor = new Color(179f / 255f, 179f / 255f, 179f / 255f, 1f);

            //GUIStyle gl = "Toggle";
            //gl.margin = new RectOffset(0, 100, 0, 0);

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

            GUILayout.Space(10);
            GUILayout.Label("Excel表格生成Json工具", TitleStyle());
            GUILayout.Space(20);
            //====================导出位置=====================
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("导出位置", GUILayout.Width(80));
            _selectIndex = EditorGUILayout.Popup(_selectIndex, _outputPath);
            GUILayout.EndHorizontal();
            //================================================
            GUILayout.Space(10);
            //=====================导出单表标题=================
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("单个表格导出", titleStyle, GUILayout.Width(800));
            GUILayout.EndHorizontal();
            //================================================
            GUILayout.Space(10);
            //===================导出单表======================
            GUILayout.BeginHorizontal();
            GUILayout.Label("Excel单文件：", GUILayout.Width(90));
            _pathExcelFile = GUILayout.TextField(_pathExcelFile);
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                var path   = string.IsNullOrEmpty(_pathExcelFile) ? GetExcelFolder() : _pathExcelFile;
                var folder = Path.GetDirectoryName(path);
                _pathExcelFile =
                    EditorUtility.OpenFilePanel("Open Excel file", folder, "excel files;*.xls;*.xlsx;*.xlsm");
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("单个导出"))
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
                    new ExcelExportToAsset().Generate(_pathExcelFile, (ExcelConvertPathType)_selectIndex);

                    sw.Stop();
                    Debug.Log("generate excel complete, total time:" + sw.ElapsedMilliseconds);
                }
            }

            GUILayout.EndHorizontal();
            //================================================

            GUILayout.Space(50);
            //==================文件夹导出==============================
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("文件夹导出", titleStyle, GUILayout.Width(800));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Excel文件夹：", GUILayout.Width(80));
            _pathExcelFolder = GUILayout.TextField(_pathExcelFolder, GUILayout.Width(680));
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                var path   = string.IsNullOrEmpty(_pathExcelFolder) ? GetExcelFolder() : _pathExcelFolder;
                var folder = Path.GetDirectoryName(path);
                _pathExcelFolder = EditorUtility.OpenFolderPanel("Open Excel folder", folder, null);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("文件夹导出"))
            {
                if (string.IsNullOrEmpty(_pathExcelFolder))
                {
                    Debug.LogError("pathExcelFile is null");
                }
                else
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    new ExcelConvertRequest().GenerateAllClass(_pathExcelFolder);
                    new ExcelConvertRequest().GenerateAllAsset(_pathExcelFolder, (ExcelConvertPathType)_selectIndex);

                    sw.Stop();
                    Debug.Log("generate excel complete, total time:" + sw.ElapsedMilliseconds);
                }
            }

            GUILayout.EndHorizontal();
            //================================================

            GUILayout.Space(50);
            //================导出所有================================
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("导出所有表格", titleStyle, GUILayout.Width(800));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("导出所有表格"))
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                new ExcelConvertRequest().GenerateAllClass(ExcelConvertPathSetting.GetExcelPath());
                new ExcelConvertRequest().GenerateAllAsset(ExcelConvertPathSetting.GetExcelPath(),
                    (ExcelConvertPathType)_selectIndex);

                sw.Stop();
                Debug.Log("generate excel complete, total time:" + sw.ElapsedMilliseconds);
            }

            GUILayout.EndHorizontal();
            //================================================
            GUILayout.Space(10);
            GUILayout.Label("说明", DescTitleStyle());
            GUILayout.Label(_descStr0, ContentStyle());
            GUILayout.Label(_descStr1, TipStyle());

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
        /// 内容的样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle ContentStyle()
        {
            var labelStyle = new GUIStyle
            {
                fontSize  = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = Color.green }
            };
            return labelStyle;
        }


        /// <summary>
        /// 提示的样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle TipStyle()
        {
            var labelStyle = new GUIStyle
            {
                fontSize  = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = Color.red }
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