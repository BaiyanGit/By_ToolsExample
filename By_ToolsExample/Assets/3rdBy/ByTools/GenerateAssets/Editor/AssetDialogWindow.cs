namespace _3rdBy.ByTools.GenerateAssets.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 自定义的弹出输入窗口
    /// </summary>
    public class AssetsInputDialogWindow : EditorWindow
    {
        private static AssetsInputDialogWindow _window;
        private static Dictionary<int, AssetGeneratorEditorWindow.ScriptableObjectInfo> _dictionary = new();
        private Action<string> _onConfirm;
        private static string _inputText = "";

        /// <summary>
        /// 弹出提示框
        /// </summary>
        /// <param name="inputText"> 输入框内容 </param>
        /// <param name="dir"> 文件名列表 </param>
        /// <param name="onConfirmCallback"> 确认回调 </param>
        /// <param name="title"> 标题 </param>
        public static void ShowDialog(string inputText, Dictionary<int, AssetGeneratorEditorWindow.ScriptableObjectInfo> dir, Action<string> onConfirmCallback)
        {
            // 带入参数
            _inputText  = inputText;
            _dictionary = dir;
            // 窗口设置
            _window              = CreateInstance<AssetsInputDialogWindow>();
            _window.titleContent = new GUIContent("资产名称重复");
            _window._onConfirm   = onConfirmCallback;
            _window.position     = new Rect((float)Screen.width / 2, (float)Screen.height / 2, 300, 100);
            // _window.ShowModal(); // 模式窗口，阻塞其他窗口
            // _window.ShowPopup(); // 弹出窗口，不阻塞其他窗口
            _window.Show(); // 弹出窗口，不阻塞其他窗口
        }

        private void RenameWindow()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("请输入新Assets资产名称:", EditorStyles.boldLabel);
            _inputText = EditorGUILayout.TextField(_inputText);

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("确定"))
            {
                if (string.IsNullOrEmpty(_inputText))
                {
                    EditorUtility.DisplayDialog("提示", "不能为空！", "确定");
                }
                else
                {
                    bool isRepeat = false; // 是否有重名文件
                    foreach (var (key, value) in _dictionary)
                    {
                        var soName = value.name;
                        if (soName.Equals(_inputText))
                        {
                            isRepeat = true;
                            break;
                        }
                    }

                    if (isRepeat == false)
                    {
                        _onConfirm?.Invoke(_inputText);
                        _window.Close();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("提示", "资产名称重复！", "确定");
                    }
                }
            }

            if (GUILayout.Button("取消"))
            {
                _window.Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            RenameWindow();
        }

        private void OnDestroy()
        {
            // 窗口关闭时清理引用
            if (_window) _window = null;
        }
    }
}