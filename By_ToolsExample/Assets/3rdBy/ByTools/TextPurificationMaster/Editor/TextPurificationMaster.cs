/*
 * TextPurificationMaster.cs
 * 文本净化大师
 * 作用：去除文本中的重复字符
 * 作者：王柏雁
 */

namespace _3rdBy.ByTools.TextPurificationMaster.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class TextPurificationMaster : EditorWindow
    {
        private static readonly Vector2 windowSize = new(550, 200);
        private string _filePath;
        private string _fileName;
        private string _successTips;


        [MenuItem("ByTools/🧩 文本净化大师")]
        private static void ShowEditor()
        {
            var window = GetWindow<TextPurificationMaster>();
            window.minSize = windowSize;
            window.maxSize = windowSize;
            window.titleContent.text = "文本净化大师(针对TextMeshPro字库清除相同文字)";
        }

        private static GUIStyle TipsLabelStyle()
        {
            var tipsGUILayout = new GUIStyle
            {
                fontStyle = FontStyle.Italic,
                fontSize = 12,
                normal = { textColor = Color.green }
            };
            return tipsGUILayout;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            SelectedTxtFile();
            GUILayout.Space(10);
            StartPurification();
            GUILayout.Space(30);
            GUILayout.EndVertical();
            GUILayout.Label(_successTips, TipsLabelStyle());
        }

        /// <summary>
        /// 选择txt文件
        /// </summary>
        private void SelectedTxtFile()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("选择文件.txt", EditorStyles.boldLabel, GUILayout.Width(90));
            GUILayout.TextField(_fileName, GUILayout.Width(300));

            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                _successTips = "";
                _filePath ??= Application.dataPath;
                _filePath = EditorUtility.OpenFilePanel("选择文件.txt", $"{_filePath}", "txt");
                _fileName = Path.GetFileName(_filePath);
            }

            if (GUILayout.Button("清除选择", GUILayout.Width(60)))
            {
                _filePath = "";
                _fileName = "";
                _successTips = "";
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(_filePath);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 开始净化
        /// </summary>
        private void StartPurification()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("开始净化（替换源文件）"))
            {
                Purification(true);
            }

            if (GUILayout.Button("开始净化（保留源文件）"))
            {
                Purification(false);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 文本净化
        /// </summary>
        /// <param name="txt"></param>
        private void Purification(bool isReplaceFile)
        {
            if (!File.Exists(_filePath))
            {
                EditorUtility.DisplayDialog("提示", "请选择文件", "OK");
                return;
            }

            var filePath = isReplaceFile
                ? _filePath
                : _filePath.Replace($"{_fileName}", $"{_fileName.Replace(".txt", "")}_temp.txt");
            var txt = File.ReadAllText(_filePath);
            var hashSet = new HashSet<char>();
            foreach (var charStr in txt.Where(charStr => !hashSet.Add(charStr)))
            {
                Debug.Log($"去重字符：{charStr}");
            }

            File.WriteAllText(filePath, string.Join("", hashSet));
            _successTips = $"净化完成\n文件路径：{filePath}";
            _successTips = _successTips.Insert(81, "\n");
            AssetDatabase.Refresh();
        }
    }
}