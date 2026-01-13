namespace ByTools.ChangeTMP.Editor
{
    using System.Text.RegularExpressions;
    using TMPro;
    using UnityEditor;
    using UnityEngine;

    public class ChangeTMPParameter : EditorWindow
    {
        [MenuItem("ByTools/🧩 TMP参数修改 %#2", false, 1006)]
        public static void ShowWindow()
        {
            GetWindow<ChangeTMPParameter>("TMP添加组件");
        }

        private MonoScript componentToAdd;
        private bool enableAutoSize = false;
        private bool enableWrapping = false;
        private Vector2Int SetRange = new Vector2Int();
        private DefaultAsset targetFolder;

        void OnGUI()
        {
            GUILayout.Label("选择添加到TextMeshPro对象的脚本", EditorStyles.boldLabel);

            componentToAdd = EditorGUILayout.ObjectField("添加的脚本", componentToAdd, typeof(MonoScript), false) as MonoScript;
            enableAutoSize = EditorGUILayout.Toggle("Set Auto Size", enableAutoSize);
            enableWrapping = EditorGUILayout.Toggle("Set Wrapping", enableWrapping);
            SetRange       = EditorGUILayout.Vector2IntField("Set MinMax", SetRange);
            targetFolder   = EditorGUILayout.ObjectField("目标文件夹", targetFolder, typeof(DefaultAsset), false) as DefaultAsset;

            if (GUILayout.Button("将组件添加至TextMeshPro"))
            {
                if (componentToAdd != null)
                {
                    if (targetFolder != null)
                    {
                        AddComponentToAllTMPPrefabs();
                    }
                    else
                    {
                        Debug.LogWarning("请选择目标文件夹");
                    }
                }
                else
                {
                    Debug.LogWarning("请选择要添加的脚本");
                }
            }
        }

        /// <summary>
        /// 包含中文添加
        /// </summary>
        private void AddComponentToAllTMPPrefabs()
        {
            string folderPath = AssetDatabase.GetAssetPath(targetFolder);
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            int prefabCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    TextMeshPro[] tmps = prefabInstance.GetComponentsInChildren<TextMeshPro>(true);
                    TextMeshProUGUI[] tmpUguis = prefabInstance.GetComponentsInChildren<TextMeshProUGUI>(true);

                    foreach (TextMeshPro tmp in tmps)
                    {
                        if (ContainsChinese(tmp.text))
                        {
                            Debug.Log($"TextMeshPro '{tmp.gameObject.name}' 预制物 '{prefab.name}' 包含中文字符.");
                            AddComponentToGameObject(tmp.gameObject);
                            tmp.enableAutoSizing   = enableAutoSize;
                            tmp.enableWordWrapping = enableWrapping;

                            tmp.fontSizeMin = SetRange.x != 0 ? SetRange.x : tmp.fontSizeMin;
                            tmp.fontSizeMax = SetRange.y != 0 ? SetRange.y : tmp.fontSize;
                        }
                    }

                    foreach (TextMeshProUGUI tmpUGUI in tmpUguis)
                    {
                        if (ContainsChinese(tmpUGUI.text))
                        {
                            Debug.Log($"TextMeshProUGUI '{tmpUGUI.gameObject.name}' 预制物 '{prefab.name}' 包含中文字符.");
                            AddComponentToGameObject(tmpUGUI.gameObject);
                            tmpUGUI.enableAutoSizing   = enableAutoSize;
                            tmpUGUI.enableWordWrapping = enableWrapping;

                            tmpUGUI.fontSizeMin = SetRange.x != 0 ? SetRange.x : tmpUGUI.fontSizeMin;
                            tmpUGUI.fontSizeMax = SetRange.y != 0 ? SetRange.y : tmpUGUI.fontSize;
                        }
                    }

                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                    DestroyImmediate(prefabInstance);
                    prefabCount++;
                }
            }

            Debug.Log($"TextMeshPro objects {prefabCount} SetAutoSize: {enableAutoSize} Min: {SetRange.x} Max: {SetRange.y}.");
        }

        private void AddComponentToGameObject(GameObject go)
        {
            System.Type componentType = componentToAdd.GetClass();
            if (componentType != null && !go.GetComponent(componentType))
            {
                go.AddComponent(componentType);
            }
        }

        /// <summary>
        /// 是否包含中文检测
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool ContainsChinese(string text)
        {
            return Regex.IsMatch(text, @"[\u4e00-\u9fa5]");
        }
    }
}