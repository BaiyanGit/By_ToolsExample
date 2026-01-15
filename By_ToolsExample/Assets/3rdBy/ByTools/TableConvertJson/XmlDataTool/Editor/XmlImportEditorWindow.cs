/*
 * 作者：王柏雁
 * 日期：2024-8-2
 * 作用：把XML生成Json，并且生成对应的类的工具窗口。
 */

namespace _3rdBy.ByTools.TableConvertJson.XmlDataTool.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using _3rdBy.ByTools.GuiEditor.Editor;
    using Config;
    using ConvertHelper;
    using Generator;
    using UnityEditor;
    using UnityEngine;

    public class XmlImportEditorWindow : EditorWindow
    {
        private int _exportFolderIndex;                        // 导出至文件夹索引
        private string[] _exportFolderType;                    // 导出至文件夹种类
        private string _xmlFilePath;                           // xml文件路径
        private string _xmlFileName;                           // xml文件名称
        private string _pathFileFolder;                        // xml文件夹
        private Dictionary<string, string> _xmlFileDictionary; //已经选择的xml文件
        private ToggleManager _exportWayManager;               // 导出方式选择
        private ExportWay _exportWay;                          //导出方式选择
        private bool _isExportClass;                           // 是否导出类

        [MenuItem("ByTools/🧩 Xml导入工具")]
        public static void ShowWindow()
        {
            var screenRes = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height); // 获取当前屏幕的分辨率
            var pos       = new Vector2(screenRes.x / 2 - 300, screenRes.y / 2 - 300);                    // 计算窗口的中心位置
            var window    = GetWindow<XmlImportEditorWindow>("Xml导入工具");
            window.minSize  = Vector2.one * 700;
            window.maxSize  = Vector2.one * 700;
            window.position = new Rect(pos.x, pos.y, 700, 700);
            window.Show();
        }

        private void OnEnable()
        {
            _exportFolderType = Enum.GetNames(typeof(XmlConvertPathType));

            _exportWayManager = new ToggleManager();
            var singleFile = _exportWayManager.AddToggle("单个文件", 100, true);
            var folderFile = _exportWayManager.AddToggle("文件夹", 100);

            singleFile.onValueChanged.AddListener(_ => { ExportWay(singleFile); });
            folderFile.onValueChanged.AddListener(_ => { ExportWay(folderFile); });
        }

        /// <summary>
        /// 导出方式
        /// </summary>
        private void ExportWay(GUIToggle tog)
        {
            if (!tog.isOn) return;

            _exportWay = tog.togName switch
            {
                "单个文件" => Config.ExportWay.Single,
                "文件夹"  => Config.ExportWay.Folder,
                _      => _exportWay
            };
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Xml表生成Json工具", GUILayoutStyle.TitleStyle());

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            ExportLocation();              // 导出位置
            _exportWayManager.OnDrawAll(); // 单个文件 | 文件夹
            DrawExportClassSelected();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            DrawSingleOrFolderButton();

            // Tips: 绘制选中的xml文件
            GUILayout.Space(10);
            DrawSelectXmlFile();

            // Tips: 生成Xml相关资源
            GUILayout.Space(10);
            GenerateXmlAssets();
        }

        /// <summary>
        /// 是否导出类选择
        /// </summary>
        private void DrawExportClassSelected()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("是否导出类", GUILayout.Width(80));
            _isExportClass = EditorGUILayout.Toggle("", _isExportClass, GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制单个或文件选择按钮
        /// </summary>
        private void DrawSingleOrFolderButton()
        {
            if (_exportWay == Config.ExportWay.Single)
                SelectXmlFile();
            if (_exportWay == Config.ExportWay.Folder)
                SelectXmlFolder();
        }

        /// <summary>
        /// 导出位置
        /// </summary>
        private void ExportLocation()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("导出位置", GUILayout.Width(80));
            _exportFolderIndex = EditorGUILayout.Popup(_exportFolderIndex, _exportFolderType, GUILayout.Width(200));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 生成Xml相关资源
        /// </summary>
        private void GenerateXmlAssets()
        {
            if (_xmlFileDictionary is { Count: <= 0 }) return;
            if (!GUILayout.Button("开始导出", GUILayout.Height(30))) return;


            var folderType = (SaveJsonPathType)_exportFolderIndex;
            var sw         = new System.Diagnostics.Stopwatch();
            sw.Start();
            foreach (var file in _xmlFileDictionary)
            {
                // new XmlExportToAsset().XmlGenerateToJson(file.Value, out var json, out var fileName);
                // var jsonTxt = $"{{\n \"dataList\":{json} \n}}";
                // XmlConvertPathSetting.SaveJsonToFile(jsonTxt, fileName, folderType);
                //
                // if (_isExportClass)
                // {
                //     new XmlExportToClass().GenerateClassesFromJson(json, fileName);
                // }
            }

            sw.Stop();
            AssetDatabase.Refresh();

            var buildTime = sw.ElapsedMilliseconds > 1000 ? sw.ElapsedMilliseconds / 1000 : sw.ElapsedMilliseconds;
            Debug.Log($"生成xml完成，总时间:{buildTime}ms");
        }

        /// <summary>
        /// 选择Xml文件
        /// </summary>
        private void SelectXmlFile()
        {
            if (_xmlFileDictionary is { Count: > 0 }) return;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("添加Xml文件"))
            {
                AddXmlFile();
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        private void SelectXmlFolder()
        {
            if (_xmlFileDictionary is { Count: > 0 }) return;

            if (!GUILayout.Button("文件夹导出")) return;
            var path = string.IsNullOrEmpty(_pathFileFolder)
                           ? XmlConvertPathSetting.GetXmlFileDefaultFolder()
                           : _pathFileFolder;
            var folder = Path.GetDirectoryName(path);
            _pathFileFolder = EditorUtility.OpenFolderPanel("Open Excel folder", folder, null);

            if (string.IsNullOrEmpty(_pathFileFolder)) return; //没有做出选择

            var filesPath = XmlConvertRequest.GetAllFilesAtPath(_pathFileFolder);
            if (filesPath.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "文件夹没有Xml文件", "确定");
                return;
            }

            _xmlFileDictionary ??= new Dictionary<string, string>();
            foreach (var filePath in filesPath)
            {
                var xmlName = Path.GetFileNameWithoutExtension(filePath);
                _xmlFileDictionary.TryAdd(xmlName, filePath);
            }
        }

        /// <summary>
        /// 绘制选中的xml文件
        /// </summary>
        private void DrawSelectXmlFile()
        {
            _xmlFileDictionary ??= new Dictionary<string, string>();

            if (_xmlFileDictionary.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("序列", GUILayout.Width(100));
                GUILayout.Label("文件", GUILayout.Width(300));
                GUILayout.Label("     路径", GUILayout.Width(60));
                GUILayout.Label("     操作", GUILayout.Width(60));
                GUILayout.EndHorizontal();
            }

            var index = 0;
            foreach (var file in _xmlFileDictionary)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(index.ToString(), GUILayout.Width(100));
                GUILayout.Label(file.Key, GUILayout.Width(300));

                if (GUILayout.Button("查看", GUILayout.Width(60)))
                {
                    EditorUtility.DisplayDialog("查看XMl文件", $"文件：\n{file.Key}\n\n路径：\n{file.Value}", "确定");
                }

                if (GUILayout.Button("移除", GUILayout.Width(60)))
                {
                    if (_xmlFileDictionary.ContainsKey(file.Key))
                    {
                        _xmlFileDictionary.Remove(file.Key);

                        // Tips:此时字典已经改变，进行重新循环
                        GUILayout.EndHorizontal();
                        break;
                    }
                }

                GUILayout.EndHorizontal();
                index++;
            }

            if (_xmlFileDictionary is { Count: <= 0 }) return;
            if (GUILayout.Button("添加Xml文件"))
            {
                AddXmlFile();
            }
        }

        /// <summary>
        /// 选择Xml文件
        /// </summary>
        private void AddXmlFile()
        {
            var folder = string.IsNullOrEmpty(_xmlFilePath)
                             ? XmlConvertPathSetting.GetXmlFileDefaultFolder()
                             : _xmlFilePath;

            _xmlFilePath = EditorUtility.OpenFilePanel("选择Xml文件", folder, "xml");

            if (string.IsNullOrEmpty(_xmlFilePath)) return; //没有做出选择

            var fileName = Path.GetFileNameWithoutExtension(_xmlFilePath);
            _xmlFileDictionary.TryAdd(fileName, _xmlFilePath);
        }
    }
}