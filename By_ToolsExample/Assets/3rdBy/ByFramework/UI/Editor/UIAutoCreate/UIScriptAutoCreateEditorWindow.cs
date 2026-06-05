namespace _3rdBy.ByFramework.UI.Editor.UIAutoCreate
{
    using System.IO;
    using _3rdBy.ByFramework.Editor;
    using UnityEditor;
    using UnityEngine;

    public class UIScriptAutoCreateEditorWindow : EditorWindow
    {
        [Header("新UI预制体的名称")] private string _newUIName;
        [Header("UI 根节点")] private GameObject _uiRootGo;
        [Header("窗口的固定大小")] private static readonly Vector2 windowSize = new(800, 600);
        private Vector2 _scrollPos;
        [Header("窗口标题样式")] private static GUIStyle _titleStyle;
        [Header("窗口副标题样式")] private static GUIStyle _subTitleStyle;
        [Header("窗口功能模块标题样式")] private static GUIStyle _sectionTitleStyle;
        [Header("内容标签样式")] private static GUIStyle _contentStyle;
        [Header("提示信息样式")] private static GUIStyle _tipStyle;
        [Header("描述信息样式")] private static GUIStyle _descTitleStyle;
        [Header("功能模块的卡片区域样式")] private static GUIStyle _cardStyle;
        [Header("工具栏卡片区域样式")] private static GUIStyle _toolbarCardStyle;
        [Header("索引或简短描述样式")] private static GUIStyle _miniTagStyle;
        [Header("帮助框样式")] private static GUIStyle _helpBoxStyle;

        private static readonly Color headerColor = new(0.18f, 0.42f, 0.72f, 1f);
        private static readonly Color headerColorDark = new(0.12f, 0.28f, 0.48f, 1f);
        private static readonly Color cardColor = new(0.22f, 0.22f, 0.22f, 1f);
        private static readonly Color cardColorLight = new(0.78f, 0.78f, 0.78f, 1f);
        private static readonly Color accentColor = new(0.26f, 0.62f, 0.96f, 1f);
        private static readonly Color successColor = new(0.30f, 0.78f, 0.45f, 1f);
        private static readonly Color warningColor = new(1.00f, 0.70f, 0.25f, 1f);

        [MenuItem("ByTools/🧩 UI自动生成器 #&%U", false, 999)]
        private static void ShowEditor()
        {
            var window = GetWindow<UIScriptAutoCreateEditorWindow>();
            window.minSize           = windowSize;
            window.maxSize           = windowSize;
            window.titleContent.text = "UI自动生成器";
        }

        private void OnGUI()
        {
            InitStyles();

            DrawBackground();
            DrawHeader();

            GUILayout.BeginArea(new Rect(18, 88, windowSize.x - 36, windowSize.y - 106));
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);

            DrawPrefabCreateSection();
            GUILayout.Space(12);
            DrawCodeCreateSection();
            GUILayout.Space(12);
            DrawRuleSection();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 绘制窗口背景。
        /// </summary>
        private static void DrawBackground()
        {
            var bgColor = EditorGUIUtility.isProSkin
                              ? new Color(0.13f, 0.13f, 0.13f, 1f)
                              : new Color(0.88f, 0.88f, 0.88f, 1f);
            EditorGUI.DrawRect(new Rect(0, 0, windowSize.x, windowSize.y), bgColor);
        }

        /// <summary>
        /// 绘制顶部标题区域。
        /// </summary>
        private static void DrawHeader()
        {
            var headerRect = new Rect(0, 0, windowSize.x, 74);
            EditorGUI.DrawRect(headerRect, headerColorDark);
            EditorGUI.DrawRect(new Rect(0, 0, windowSize.x, 5), headerColor);
            EditorGUI.DrawRect(new Rect(0, 73, windowSize.x, 1), new Color(1f, 1f, 1f, 0.10f));

            GUILayout.BeginArea(new Rect(24, 12, windowSize.x - 48, 56));
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("🧩 UI 自动生成器", TitleStyle());
            GUILayout.Space(2);
            GUILayout.Label("快速生成 UI 预制体、View 代码与 MVC 代码", SubTitleStyle());
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Editor Tool", MiniTagStyle(), GUILayout.Height(24));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 绘制预制体生成区域。
        /// </summary>
        private void DrawPrefabCreateSection()
        {
            BeginCard();
            DrawSectionTitle("1", "预制体自动生成设置", "根据模板创建新的 UI 预制体");

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("新 UI 名字", GUILayout.Width(92));
            _newUIName          = EditorGUILayout.TextField(_newUIName, GUILayout.Height(22));
            GUI.backgroundColor = successColor;
            if (GUILayout.Button("Create", GUILayout.Width(96), GUILayout.Height(24)))
            {
                CreateUIPrefab();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            DrawInlineTip("新UI的名字需要以UI作为前缀，以保证自动生成的脚本唯一性，例如: UITest", warningColor);
            EndCard();
        }

        /// <summary>
        /// 绘制代码生成区域。
        /// </summary>
        private void DrawCodeCreateSection()
        {
            BeginCard();
            DrawSectionTitle("2", "自动生成代码设置", "拖入 View 根节点后生成对应代码");

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("View 根节点", GUILayout.Width(92));
            _uiRootGo = (GameObject)EditorGUILayout.ObjectField(_uiRootGo, typeof(GameObject), true, GUILayout.Height(22));

            GUI.backgroundColor = accentColor;
            if (GUILayout.Button("View 代码", GUILayout.Width(96), GUILayout.Height(24)))
            {
                CreateUIView();
            }

            GUI.backgroundColor = successColor;
            if (GUILayout.Button("MVC 代码", GUILayout.Width(96), GUILayout.Height(24)))
            {
                CreateMvc();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            DrawInlineTip("View 代码只生成 UIView；MVC 代码会依次生成 View、Model、Control。", warningColor);
            EndCard();
        }

        /// <summary>
        /// 绘制命名规则区域。
        /// </summary>
        private void DrawRuleSection()
        {
            BeginCard();
            DrawSectionTitle("3", "命名规则", "根据节点前缀自动识别组件类型");

            GUILayout.Space(8);
            GUILayout.Label("配置文件", DescTitleStyle());
            EditorGUILayout.ObjectField(
                AssetDatabase.LoadAssetAtPath<Object>(UIAutoCreatePathSetting.UIViewAutoCreateConfigPath),
                typeof(Object),
                false,
                GUILayout.Height(22));

            GUILayout.Space(10);
            GUILayout.BeginVertical(ToolbarCardStyle());
            DrawRuleRow(("Ts_", "Transform"), ("Sr_", "ScrollRect"), ("Dd_", "Dropdown"));
            DrawRuleRow(("If_", "InputField"), ("Txt_", "Text"), ("TMP_", "TextMeshProUGUI"));
            DrawRuleRow(("Btn_", "Button"), ("Img_", "Image"), ("Tog_", "Toggle"));
            DrawRuleRow(("Sli_", "Slider"), ("TogG_", "ToggleGroup"), ("TMPIf_", "TMP_InputField"));
            DrawRuleRow(("RectTs_", "RectTransform"), ("RawImg_", "RawImage"), (string.Empty, string.Empty));
            GUILayout.EndVertical();
            EndCard();
        }

        /// <summary>
        /// 绘制模块标题。
        /// </summary>
        private static void DrawSectionTitle(string index, string title, string desc)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(index, MiniTagStyle(), GUILayout.Width(28), GUILayout.Height(24));
            GUILayout.BeginVertical();
            GUILayout.Label(title, SectionTitleStyle());
            GUILayout.Label(desc, DescTitleStyle());
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制规则行。
        /// </summary>
        private static void DrawRuleRow(params (string prefix, string component)[] rules)
        {
            GUILayout.BeginHorizontal();
            foreach (var rule in rules)
            {
                if (string.IsNullOrEmpty(rule.prefix))
                {
                    GUILayout.Space(210);
                    continue;
                }

                GUILayout.Label($"[ {rule.prefix} ]", ContentStyle(), GUILayout.Width(72));
                GUILayout.Label($"= {rule.component}", DescTitleStyle(), GUILayout.Width(138));
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        /// <summary>
        /// 绘制行内提示。
        /// </summary>
        private static void DrawInlineTip(string content, Color color)
        {
            var oldColor = GUI.contentColor;
            GUI.contentColor = color;
            GUILayout.Label("• " + content, DescTitleStyle());
            GUI.contentColor = oldColor;
        }

        /// <summary>
        /// 开始绘制卡片区域。
        /// </summary>
        private static void BeginCard()
        {
            GUILayout.BeginVertical(CardStyle());
        }

        /// <summary>
        /// 结束绘制卡片区域。
        /// </summary>
        private static void EndCard()
        {
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 初始化 GUI 样式。
        /// </summary>
        private static void InitStyles()
        {
            _titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = Color.white }
            };

            _subTitleStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize  = 12,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = new Color(1f, 1f, 1f, 0.72f) }
            };

            _sectionTitleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.16f, 0.16f, 0.16f, 1f) }
            };

            _contentStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = accentColor }
            };

            _tipStyle ??= new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperLeft,
                wordWrap  = true,
                normal    = { textColor = EditorGUIUtility.isProSkin ? new Color(1f, 0.74f, 0.74f, 1f) : new Color(0.65f, 0.12f, 0.12f, 1f) }
            };

            _descTitleStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = EditorGUIUtility.isProSkin ? new Color(0.78f, 0.78f, 0.78f, 1f) : new Color(0.28f, 0.28f, 0.28f, 1f) }
            };

            _miniTagStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding   = new RectOffset(8, 8, 2, 2),
                normal    = { textColor = Color.white, background = MakeTex(1, 1, headerColor) }
            };

            _cardStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(14, 14, 12, 12),
                margin  = new RectOffset(0, 0, 0, 0),
                normal  = { background = MakeTex(1, 1, EditorGUIUtility.isProSkin ? cardColor : cardColorLight) }
            };

            _toolbarCardStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin  = new RectOffset(0, 0, 0, 0),
                normal  = { background = MakeTex(1, 1, EditorGUIUtility.isProSkin ? new Color(0.17f, 0.17f, 0.17f, 1f) : new Color(0.88f, 0.88f, 0.88f, 1f)) }
            };

            _helpBoxStyle ??= new GUIStyle(EditorStyles.helpBox);
        }

        private void CreateUIPrefab()
        {
            if (string.IsNullOrEmpty(_newUIName)) throw new System.Exception("请输入UI名字");

            const string strPrefab = ".prefab";
            const string strMeta   = ".meta";

            var newPrefabEditorPath    = UIAutoCreatePathSetting.PrefabCreatePath + _newUIName + strPrefab;
            var newPrefabResourcesPath = ByFrameworkPathUtility.ToFullPath(UIAutoCreatePathSetting.PrefabCreatePath);
            Debug.Log(newPrefabResourcesPath);
            //copy prefab
            //origin
            var originPrefabFullPath = ByFrameworkPathUtility.ToFullPath(UIAutoCreatePathSetting.PrefabTemplatePath + strPrefab);
            Debug.Log(originPrefabFullPath);
            //target
            var newPrefabFullPath = Path.Combine(newPrefabResourcesPath, _newUIName + strPrefab);

            //copy meta
            var originMetaFullPath = originPrefabFullPath + strMeta;
            var newMetaFullPath    = newPrefabFullPath + strMeta;

            var result = false;
            if (File.Exists(newPrefabFullPath))
            {
                if (EditorUtility.DisplayDialog("警告", "检测到UI预制体，是否覆盖", "确定", "取消"))
                {
                    File.Copy(originPrefabFullPath, newPrefabFullPath, true);
                    File.Copy(originMetaFullPath, newMetaFullPath, true);
                    result = true;
                }
            }
            else
            {
                CheckTargetPath(newPrefabResourcesPath);
                File.Copy(originPrefabFullPath, newPrefabFullPath);
                File.Copy(originMetaFullPath, newMetaFullPath);
                result = true;
            }

            if (result)
            {
                Debug.Log("UI创建成功: " + newPrefabFullPath);
                AssetDatabase.Refresh();

                _uiRootGo = AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabEditorPath);
            }
        }

        private void CreateMvc()
        {
            CreateUIView();
            CreateUIModel();
            CreateUIControl();
        }

        private void CreateUIView()
        {
            if (_uiRootGo == null) throw new System.Exception("请拖入需要生成的UI预制体");

            string       uiName     = GetUIName();
            var          tempPath   = UIAutoCreatePathSetting.TemplateFilePath + UIAutoCreatePathSetting.ViewTemplateName;
            string       targetPath = GetTargetGeneratePath(uiName);
            CheckTargetPath(targetPath);
            new UIViewAutoCreate().Create(uiName, _uiRootGo, tempPath, targetPath);
        }

        private void CreateUIControl()
        {
            if (_uiRootGo == null) throw new System.Exception("请拖入需要生成的UI预制体");

            var          uiName     = GetUIName();
            var          tempPath   = UIAutoCreatePathSetting.TemplateFilePath + UIAutoCreatePathSetting.ControlTemplateName;
            var          targetPath = GetTargetGeneratePath(uiName);
            CheckTargetPath(targetPath);
            new UIControlAutoCreate().Create(uiName, tempPath, targetPath);
        }

        private void CreateUIModel()
        {
            if (_uiRootGo == null) throw new System.Exception("请拖入需要生成的UI预制体");

            var uiName     = GetUIName();
            var tempPath   = UIAutoCreatePathSetting.TemplateFilePath + UIAutoCreatePathSetting.ModelTemplateName;
            var targetPath = GetTargetGeneratePath(uiName);
            CheckTargetPath(targetPath);
            UIModelAutoCreate.Create(uiName, tempPath, targetPath);
        }

        private static string GetTargetGeneratePath(string uiName)
        {
            return Path.Combine(UIAutoCreatePathSetting.GetUIGenerateCsFilePath(), $"UI{uiName}");
        }

        private static void CheckTargetPath(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
        }

        private string GetUIName()
        {
            var uiName = _uiRootGo.name.Replace("UI", "");
            return uiName;
        }

        /// <summary>
        /// 创建单色纹理。
        /// </summary>
        /// <param name="width">宽度。</param>
        /// <param name="height">高度。</param>
        /// <param name="color">颜色。</param>
        /// <returns>单色纹理。</returns>
        private static Texture2D MakeTex(int width, int height, Color color)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = color;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        /// <summary>
        /// 标题文字的样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle TitleStyle()
        {
            InitStyles();
            return _titleStyle;
        }

        /// <summary>
        /// 副标题文字样式。
        /// </summary>
        /// <returns>副标题文字样式。</returns>
        private static GUIStyle SubTitleStyle()
        {
            InitStyles();
            return _subTitleStyle;
        }

        /// <summary>
        /// 小标签样式。
        /// </summary>
        /// <returns>小标签样式。</returns>
        private static GUIStyle MiniTagStyle()
        {
            InitStyles();
            return _miniTagStyle;
        }

        /// <summary>
        /// 卡片样式。
        /// </summary>
        /// <returns>卡片样式。</returns>
        private static GUIStyle CardStyle()
        {
            InitStyles();
            return _cardStyle;
        }

        /// <summary>
        /// 规则容器样式。
        /// </summary>
        /// <returns>规则容器样式。</returns>
        private static GUIStyle ToolbarCardStyle()
        {
            InitStyles();
            return _toolbarCardStyle;
        }

        /// <summary>
        /// 模块标题样式。
        /// </summary>
        /// <returns>模块标题样式。</returns>
        private static GUIStyle SectionTitleStyle()
        {
            InitStyles();
            return _sectionTitleStyle;
        }

        /// <summary>
        /// 内容的样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle ContentStyle()
        {
            InitStyles();
            return _contentStyle;
        }


        /// <summary>
        /// 提示的样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle TipStyle()
        {
            InitStyles();
            return _tipStyle;
        }

        /// <summary>
        /// 说明文字标题样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle DescTitleStyle()
        {
            InitStyles();
            return _descTitleStyle;
        }
    }
}
