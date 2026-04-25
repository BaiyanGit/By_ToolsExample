namespace _3rdBy.ByTools.GenerateAssets.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 通用资产编辑窗口。
    /// 基于 Unity 默认 Inspector 绘制，可用于编辑任意 UnityEngine.Object。
    /// </summary>
    public class UniversalAssetEditorWindow : EditorWindow
    {
        [Header("当前编辑目标")] private Object _targetObject;
        [Header("缓存编辑器")] private Editor _cachedEditor;
        [Header("滚动位置")] private Vector2 _scrollPos;
        [Header("保存回调")] private System.Action<Object> _onSave;
        [Header("窗口说明")] private string _description;
        [Header("关闭时销毁目标")] private bool _destroyTargetOnClose;
        [Header("持久化资产自动保存")] private bool _autoSavePersistentAsset;

        /// <summary>
        /// 打开编辑窗口
        /// </summary>
        /// <param name="targetObject">目标资产对象</param>
        /// <param name="title">窗口标题</param>
        /// <param name="onSave">保存回调</param>
        /// <param name="destroyTargetOnClose">关闭窗口时是否销毁目标</param>
        /// <param name="autoSavePersistentAsset">是否自动保存持久化资产</param>
        /// <param name="description">顶部说明</param>
        public static void ShowWindow(
            Object targetObject,
            string title, System.Action<Object> onSave = null,
            bool destroyTargetOnClose = false,
            bool autoSavePersistentAsset = false,
            string description = "")
        {
            var window = CreateInstance<UniversalAssetEditorWindow>();
            window.titleContent = new GUIContent(string.IsNullOrEmpty(title) ? "资产编辑器" : title);
            window.minSize = new Vector2(450, 520);
            window._targetObject = targetObject;
            window._onSave = onSave;
            window._destroyTargetOnClose = destroyTargetOnClose;
            window._autoSavePersistentAsset = autoSavePersistentAsset;
            window._description = description;
            window.Show();
        }

        private void OnGUI()
        {
            if (_targetObject == null)
            {
                EditorGUILayout.HelpBox("当前没有可编辑的资产对象。", MessageType.Warning);
                if (GUILayout.Button("关闭", GUILayout.Height(28)))
                {
                    Close();
                }

                return;
            }

            if (!string.IsNullOrEmpty(_description))
            {
                EditorGUILayout.HelpBox(_description, MessageType.Info);
                EditorGUILayout.Space(4);
            }

            Editor.CreateCachedEditor(_targetObject, null, ref _cachedEditor);
            if (_cachedEditor == null)
            {
                EditorGUILayout.HelpBox("当前对象无法创建 Inspector 编辑器。", MessageType.Warning);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUI.BeginChangeCheck();
            _cachedEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_targetObject);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(6);
            DrawBottomToolbar();
        }

        /// <summary>
        /// 绘制底部工具栏
        /// </summary>
        private void DrawBottomToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("保存", GUILayout.Height(30)))
            {
                SaveCurrentTarget();
            }

            if (EditorUtility.IsPersistent(_targetObject) && GUILayout.Button("定位", GUILayout.Height(30)))
            {
                EditorGUIUtility.PingObject(_targetObject);
                Selection.activeObject = _targetObject;
            }

            if (GUILayout.Button("关闭", GUILayout.Height(30)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 保存当前目标对象
        /// </summary>
        private void SaveCurrentTarget()
        {
            if (_targetObject == null) return;

            _onSave?.Invoke(_targetObject);
            EditorUtility.SetDirty(_targetObject);

            if (EditorUtility.IsPersistent(_targetObject))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Repaint();
        }

        private void OnDisable()
        {
            // 临时对象关闭时，也自动回写一次草稿，避免用户忘记点保存。
            if (_targetObject != null && EditorUtility.IsPersistent(_targetObject) == false)
            {
                _onSave?.Invoke(_targetObject);
            }

            if (_targetObject != null && EditorUtility.IsPersistent(_targetObject) && _autoSavePersistentAsset)
            {
                _onSave?.Invoke(_targetObject);
                EditorUtility.SetDirty(_targetObject);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (_cachedEditor != null)
            {
                DestroyImmediate(_cachedEditor);
                _cachedEditor = null;
            }

            if (_destroyTargetOnClose && _targetObject != null && EditorUtility.IsPersistent(_targetObject) == false)
            {
                DestroyImmediate(_targetObject);
            }
        }
    }
}
