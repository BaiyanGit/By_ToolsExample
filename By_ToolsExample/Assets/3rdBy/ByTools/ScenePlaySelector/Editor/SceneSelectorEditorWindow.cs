namespace _3rdBy.ByTools.ScenePlaySelector.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    // 可序列化的类，用于JSON保存/加载
    [Serializable]
    public class SceneDataJson
    {
        public int index;
        public string name;
        public string path;
        public bool show;
    }

    // 用于序列化列表的包装类
    [Serializable]
    public class SceneDataListWrapper
    {
        public List<SceneDataJson> sceneDataList;
    }

    /// <summary>
    /// ToolBar中选择场景配置窗口
    /// </summary>
    public class SceneSelectorEditorWindow : EditorWindow
    {
        private class SceneData
        {
            public int        index { get; set; }
            public string     path  { get; set; }
            public SceneAsset asset { get; set; }
            public bool       show  { get; set; }

            public SceneData(int index, string path, SceneAsset asset, bool show)
            {
                this.index = index;
                this.path  = path;
                this.asset = asset;
                this.show  = show;
            }
        }

        private enum SceneSource
        {
            BuildSettings,
            Project,
            // All
        }

        private int _sourceIndex = 0;
        private readonly string[] _sourceNames = { "BuildSettings", "ProjectAssets" };
        private Vector2 _sceneListScrollPos;
        private readonly List<SceneData> _sceneDataLists = new();
        private static string fileFullPath;

        private double _lastRefreshTime;
        private const double RefreshInterval = 1;
        private const string PrefSourceIndexKey = "SourceIndexKey";
        private const string PrefKeyFoldPath = "ScenePlaySelector_FoldPath";

        private const string HelpBoxMessage = "场景显示配置器说明\n" +
                                              "1. 场景来源：BuildSettings：编译设置中的所有场景  |  ProjectAssets：项目中的所有场景  \n" +
                                              "2. 更新：项目有新增或删除场景文件时使用\n" +
                                              "3. 保存：设置显示和隐藏场景在Toolbar栏中\n";

        #region 自动刷新

        private void OnEnable()
        {
            _sourceIndex = EditorPrefs.GetInt(PrefSourceIndexKey, 0);
            GetScenesWay();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            // 自动刷新
            /*var now = EditorApplication.timeSinceStartup;
            if (now - _lastRefreshTime >= RefreshInterval)
            {
                _lastRefreshTime = now;
                GetScenes((SceneSource)_sourceIndex);
            }*/
        }

        #endregion

        #region 创建GUI样式

        private GUIStyle _sceneTitleStyle;

        private void CreateGUIStyle()
        {
            _sceneTitleStyle ??= new GUIStyle()
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white
                },
            };
        }

        #endregion

        [MenuItem("ByTools/🧩 场景显示配置器")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneSelectorEditorWindow>("场景显示配置器");
            window.minSize = new Vector2(360, 90);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(HelpBoxMessage, MessageType.Info);
            DeleteJsonFile();
            CreateGUIStyle();
            OnSelectionSource();

            DrawScenesList();
        }

        private void DeleteJsonFile()
        {
            if (!string.IsNullOrEmpty(fileFullPath))
            {
                var configFile = Path.GetFileNameWithoutExtension(fileFullPath);
                if (File.Exists(fileFullPath))
                {
                    if (GUILayout.Button($"删除当前配置文件 [ {configFile}.json ]"))
                    {
                        if (File.Exists(fileFullPath))
                        {
                            File.Delete(fileFullPath);
                            AssetDatabase.Refresh();
                        }
                        else
                        {
                            Debug.LogError("文件不存在");
                        }

                        ScenePlaySelector.RefreshToolbar();
                    }
                }
            }
        }

        private void OnSelectionSource()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("场景来源：", GUILayout.Width(80));
            var sourceIndex = EditorGUILayout.Popup(_sourceIndex, _sourceNames, GUILayout.Width(200));
            if (_sourceIndex != sourceIndex)
            {
                _sourceIndex = sourceIndex;
                // Debug.Log("刷新");
                EditorPrefs.SetInt(PrefSourceIndexKey, _sourceIndex);
                GetScenesWay();
            }

            if (GUILayout.Button("更新"))
            {
                GetScenes();
            }

            if (GUILayout.Button("保存"))
            {
                if (_sceneDataLists.Count == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                var wrapper = new SceneDataListWrapper
                {
                    sceneDataList = new List<SceneDataJson>()
                };
                for (int i = 0; i < _sceneDataLists.Count; i++)
                {
                    if (!_sceneDataLists[i].asset) continue;
                    var sceneDataJson = new SceneDataJson
                    {
                        index = _sceneDataLists[i].index,
                        name  = _sceneDataLists[i].asset.name,
                        path  = _sceneDataLists[i].path,
                        show  = _sceneDataLists[i].show
                    };
                    wrapper.sceneDataList.Add(sceneDataJson);
                }

                string jsonString = JsonUtility.ToJson(wrapper, true); // true表示格式化输出
                SaveJsonFile(jsonString);
                ScenePlaySelector.RefreshToolbar();
            }

            if (GUILayout.Button("全选"))
            {
                foreach (var sceneData in _sceneDataLists)
                {
                    if (sceneData.asset == null) continue;
                    sceneData.show = true;
                }
            }

            if (GUILayout.Button("反选"))
            {
                foreach (var sceneData in _sceneDataLists)
                {
                    if (sceneData.asset == null) continue;
                    sceneData.show = !sceneData.show;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawScenesList()
        {
            _sceneListScrollPos = EditorGUILayout.BeginScrollView(_sceneListScrollPos);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("显示", _sceneTitleStyle, GUILayout.Width(30));
                EditorGUILayout.LabelField("序号", _sceneTitleStyle, GUILayout.Width(30));
                EditorGUILayout.LabelField("场景", _sceneTitleStyle);
                EditorGUILayout.EndHorizontal();

                foreach (var sceneData in _sceneDataLists)
                {
                    if (sceneData.asset == null) continue;
                    EditorGUILayout.BeginHorizontal();
                    sceneData.show = GUILayout.Toggle(sceneData.show, "", EditorStyles.radioButton, GUILayout.Width(30));
                    EditorGUILayout.LabelField($"{sceneData.index}. ", GUILayout.Width(30));
                    EditorGUILayout.ObjectField(sceneData.asset, typeof(SceneAsset), false);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        #region 辅助

        private void GetScenesWay()
        {
            fileFullPath = Path.Combine(CheckDirectory(), $"{_sourceNames[_sourceIndex]}.json");
            if (File.Exists(fileFullPath))
                ReadJsonFile();
            else
                GetScenes();
        }

        private void GetScenes()
        {
            switch ((SceneSource)_sourceIndex)
            {
                case SceneSource.BuildSettings:
                    var sceneBuildPaths = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
                    RefreshSceneDataLists(sceneBuildPaths);
                    break;

                case SceneSource.Project:
                    var sceneProjectPaths = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).ToArray();
                    RefreshSceneDataLists(sceneProjectPaths);
                    break;
            }
        }

        private void RefreshSceneDataLists(string[] scenePaths)
        {
            var sceneDataLists = new List<SceneData>();
            for (int i = 0; i < scenePaths.Length; i++)
            {
                var scenePath  = scenePaths[i];
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (_sceneDataLists.Any(s => s.path == scenePath))
                {
                    // 标记配置表中已经包含的场景
                    var sceneData = _sceneDataLists.Find(s => s.path == scenePath);
                    sceneDataLists.Add(sceneData);
                    // continue;
                }
                else
                {
                    // 标记配置表中未包含的场景
                    sceneDataLists.Add(new SceneData(i, scenePath, sceneAsset, true));
                }
            }

            _sceneDataLists.Clear();
            for (int i = 0; i < sceneDataLists.Count; i++)
            {
                var sceneData = sceneDataLists[i];
                sceneData.index = i;
                _sceneDataLists.Add(sceneData);
            }
        }

        private void SaveJsonFile(string jsonContent)
        {
            try
            {
                fileFullPath = Path.Combine(CheckDirectory(), $"{_sourceNames[_sourceIndex]}.json");

                if (File.Exists(fileFullPath)) File.Delete(fileFullPath);
                File.WriteAllText(fileFullPath, jsonContent, Encoding.UTF8);
            }
            catch (Exception e)
            {
                EditorGUILayout.EndHorizontal();
                throw new Exception($"保存失败:{e.Message}");
            }

            AssetDatabase.Refresh();
        }

        private void ReadJsonFile()
        {
            fileFullPath = Path.Combine(CheckDirectory(), $"{_sourceNames[_sourceIndex]}.json");

            if (!File.Exists(fileFullPath))
            {
                Debug.LogError("文件不存在");
                EditorGUILayout.EndHorizontal();
                return;
            }

            var jsonContent     = File.ReadAllText(fileFullPath);
            var dataListWrapper = JsonUtility.FromJson<SceneDataListWrapper>(jsonContent);
            // Debug.Log(string.Join("\n", dataListWrapper.sceneDataList.Select(s => s.name)));

            // 刷新数据
            _sceneDataLists.Clear();
            for (int i = 0; i < dataListWrapper.sceneDataList.Count; i++)
            {
                var data       = dataListWrapper.sceneDataList[i];
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(data.path);
                _sceneDataLists.Add(new SceneData(data.index, data.path, sceneAsset, data.show));
            }
        }

        private string CheckDirectory()
        {
            var monoScript     = MonoScript.FromScriptableObject(this);
            var scriptFullPath = AssetDatabase.GetAssetPath(monoScript);
            var folderPath     = GetUpperDirectory(scriptFullPath, 2, "ScenePlaySelectorFiles");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            EditorPrefs.SetString(PrefKeyFoldPath, folderPath);
            return folderPath;
        }

        // 获取上级目录
        private static string GetUpperDirectory(string path, int level, string lastFold = "")
        {
            var currentPath = path;

            for (var i = 0; i < level; i++)
            {
                currentPath = Path.GetDirectoryName(currentPath);

                if (string.IsNullOrEmpty(currentPath))
                    return null;
            }

            lastFold    = string.IsNullOrEmpty(lastFold) ? lastFold : $"{lastFold}/";
            currentPath = $"{currentPath.Replace('\\', '/')}/{lastFold}";
            return currentPath;
        }

        #endregion
    }
}