namespace _3rdBy.ByTools.FindHierarchyTag.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 对打开的场景，根据标签查找使用对象
    /// </summary>
    public class FindHierarchyTagEditorWindow : EditorWindow
    {
        private enum ViewType
        {
            TagView,
            ObjView
        }

        private static readonly List<string> tags = new();
        private static readonly Dictionary<string, List<GameObject>> objects = new();
        private int _selectedOption; //列表中选中的索引
        private int _curSelectIndex; //当前选择标签的索引
        private static string _tagName = "";

        private static GUILayoutOption _btnWidth;
        private Vector2 _tagsScrollPosition = Vector2.zero;
        private Vector2 _objsScrollPosition = Vector2.zero;

        [MenuItem("ByTools/🏷️ 标签Tag查找对象")]
        public static void ShowWindow()
        {
            var window = GetWindow<FindHierarchyTagEditorWindow>("Find With Tag");
            window.minSize = Vector2.one * 400;
            window.maxSize = Vector2.one * 400;
        }

        private void OnEnable()
        {
            tags.Clear();
            objects.Clear();
            ClearConsole();
            //查找场景中所有物体所用的标签
            var sceneAllObjects = FindObjectsOfType<GameObject>(true);
            foreach (var item in sceneAllObjects)
            {
                var tag = item.tag;
                if (tags.Contains(tag) || tag == "Untagged") continue;

                tags.Add(tag);
            }

            // tags.Sort((a, b) => a.Length < b.Length ? 1 : -1);

            //使用相同标签对象归类
            foreach (var tag in tags)
            {
                List<GameObject> objs = new();
                foreach (var obj in sceneAllObjects)
                {
                    if (!obj.CompareTag(tag) || objs.Contains(obj)) continue;
                    objs.Add(obj);
                }

                objects.Add(tag, objs);
            }
        }

        private void OnGUI()
        {
            _btnWidth = GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 50);

            GUILayout.BeginHorizontal();

            SetView(ViewType.TagView, "标签");
            SetView(ViewType.ObjView, "对象");

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 设置视图
        /// </summary>
        /// <param name="viewType"></param>
        /// <param name="titleName"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void SetView(ViewType viewType, string titleName)
        {
            var viewWidth = GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 10);
            //################## 显示视图 Start ##############
            GUILayout.BeginVertical(Background(), _btnWidth);

            //================= Title Start ==================
            GUILayout.BeginHorizontal(_btnWidth);
            GUILayout.Label(titleName, TitleStyle());
            GUILayout.EndHorizontal();
            //================= Title  End  ==================

            //----------------- ScrollView Start -------------
            switch (viewType)
            {
                case ViewType.TagView:
                    _tagsScrollPosition = GUILayout.BeginScrollView(_tagsScrollPosition, viewWidth);
                    TagsView();
                    break;
                case ViewType.ObjView:
                    _objsScrollPosition = GUILayout.BeginScrollView(_objsScrollPosition, viewWidth);
                    ObjsView();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(viewType), viewType, null);
            }

            GUILayout.EndScrollView();
            //----------------- ScrollView   End -------------

            GUILayout.EndVertical();
            //################## 显示视图 End   ##############
        }

        private void TagsView()
        {
            //显示所有标签
            _selectedOption = GUILayout.SelectionGrid(_selectedOption, tags.ToArray(), 1);
            if (_curSelectIndex == _selectedOption) return;
            _curSelectIndex = _selectedOption;
            _tagName = tags[_curSelectIndex]; //当前标签名字
            //显示右边视图
        }

        /// <summary>
        /// 显示选择此标签中的所有对象
        /// </summary>
        private static void ObjsView()
        {
            objects.TryGetValue(_tagName, out var objs);
            if (objs == null) return;
            for (var i = 0; i < objs.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"{i}. {objs[i].name}", ObjStyle()))
                {
                    EditorGUIUtility.PingObject(objs[i]);
                }

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 标题样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle TitleStyle()
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            return labelStyle;
        }

        /// <summary>
        /// 视图背景
        /// </summary>
        /// <returns></returns>
        private static GUIStyle Background()
        {
            var bgStyle = new GUIStyle(GUI.skin.label)
            {
                normal =
                {
                    background = CreateColorTexture(Color.gray),
                },
            };
            return bgStyle;
        }

        /// <summary>
        /// 对象文本样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle ObjStyle()
        {
            Color color = new(0.345f, 0.345f, 0.345f, 1);
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = Color.white,
                    background = CreateColorTexture(color),
                },
                hover =
                {
                    textColor = Color.white,
                    background = CreateColorTexture(color),
                },
                active =
                {
                    textColor = Color.yellow,
                    background = CreateColorTexture(color),
                }
            };

            return labelStyle;
        }

        /// <summary>
        /// 按钮颜色
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private static Texture2D CreateColorTexture(Color color)
        {
            Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static void GetTag(string tag)
        {
            var index = tag.IndexOf('.');
            var str = tag[..(index + 1)];
            Debug.Log(str);
        }

        /// <summary>
        /// 清除控制台
        /// </summary>
        private static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var logEntries = assembly.GetType("UnityEditor.LogEntries");
            var clearMethod = logEntries.GetMethod("Clear");
            clearMethod?.Invoke(new object(), null);
        }
    }
}