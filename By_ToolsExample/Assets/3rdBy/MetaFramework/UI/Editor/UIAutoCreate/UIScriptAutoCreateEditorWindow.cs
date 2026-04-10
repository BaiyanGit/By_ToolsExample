namespace _3rdBy.MetaFramework.UI.Editor.UIAutoCreate
{
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class UIScriptAutoCreateEditorWindow : EditorWindow
    {
        private string _newUIName;

        private GameObject _uiRootGo;

        private static string _readMeText;

        private static readonly Vector2 windowSize = new(800, 600);

        [MenuItem("ByTools/🧩 UI自动生成器 #&%U", false, 999)]
        static void ShowEditor()
        {
            var window = GetWindow<UIScriptAutoCreateEditorWindow>();
            window.minSize           = windowSize;
            window.maxSize           = windowSize;
            window.titleContent.text = "UI自动生成器";

            var readMe = AssetDatabase.LoadAssetAtPath<TextAsset>(UIAutoCreatePathSetting.ReadMeFilePath);
            _readMeText = readMe.text;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, windowSize.x, windowSize.y));
            GUILayout.BeginVertical();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("UI 生成工具", TitleStyle());
            GUILayout.EndHorizontal();

            //============================自动生成预制体设置==============================
            GUILayout.Space(20);
            EditorGUILayout.LabelField("1、预制体自动生成设置", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("新UI名字:", GUILayout.Width(70));
            _newUIName = EditorGUILayout.TextField(_newUIName, GUILayout.Width(400));
            if (GUILayout.Button("Create", GUILayout.Width(70)))
            {
                CreateUIPrefab();
            }

            GUILayout.EndHorizontal();

            //=======================================================================

            //============================自动生成代码设置==============================
            GUILayout.Space(20);
            EditorGUILayout.LabelField("2、自动生成代码设置", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("View根节点:", GUILayout.Width(70));
            _uiRootGo = (GameObject)EditorGUILayout.ObjectField(_uiRootGo, typeof(GameObject), true,
                GUILayout.Width(400));
            if (GUILayout.Button("View代码", GUILayout.Width(70)))
            {
                CreateUIView();
            }

            if (GUILayout.Button("MVC代码", GUILayout.Width(70)))
            {
                CreateMvc();
            }

            GUILayout.EndHorizontal();


            //=======================================================================

            //============================说明==============================
            GUILayout.Space(20);
            var width = GUILayout.Width(200);
            GUILayout.Label("命令规则如下：", DescTitleStyle());
            EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<Object>(UIAutoCreatePathSetting.UIViewAutoCreateConfigPath), typeof(Object), false, GUILayout.Width(400));
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("[ Ts_ ] = Transform", ContentStyle(), width);
            GUILayout.Label("[ Sr_ ] = ScrollRect", ContentStyle(), width);
            GUILayout.Label("[ Dd_ ] = Dropdown", ContentStyle(), width);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("[ If_ ] = InputField", ContentStyle(), width);
            GUILayout.Label("[ Txt_ ] = Text", ContentStyle(), width);
            GUILayout.Label("[ TMP_ ] = TextMeshProUGUI", ContentStyle(), width);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("[ Btn_ ] = Button", ContentStyle(), width);
            GUILayout.Label("[ Img_ ] = Image", ContentStyle(), width);
            GUILayout.Label("[ Tog_ ] = Toggle", ContentStyle(), width);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("[ Sli_ ] = Slider", ContentStyle(), width);
            GUILayout.Label("[ TogG_ ] = ToggleGroup", ContentStyle(), width);
            GUILayout.Label("[ TMPIf_ ] = TMP_InputField", ContentStyle(), width);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("[ RectTs_ ] = RectTransform", ContentStyle(), width);
            GUILayout.Label("[ RawImg_ ] = RawImage", ContentStyle(), width);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label(_readMeText, TipStyle());

            GUILayout.EndVertical();
            //=======================================================================
            GUILayout.EndArea();
        }

        private void CreateUIPrefab()
        {
            if (string.IsNullOrEmpty(_newUIName)) throw new System.Exception("请输入UI名字");

            const string strPrefab = ".prefab";
            const string strMeta   = ".meta";

            var newPrefabEditorPath    = UIAutoCreatePathSetting.PrefabCreatePath + _newUIName + strPrefab;
            var newPrefabResourcesPath = Application.dataPath + UIAutoCreatePathSetting.PrefabCreatePath;
            newPrefabResourcesPath = newPrefabResourcesPath.Replace("/AssetsAssets", "/Assets");
            Debug.Log(newPrefabResourcesPath);
            //copy prefab
            //origin
            var originPrefabFullPath = Application.dataPath + UIAutoCreatePathSetting.PrefabTemplatePath + strPrefab;
            originPrefabFullPath = originPrefabFullPath.Replace("/AssetsAssets", "/Assets");
            Debug.Log(originPrefabFullPath);
            //target
            var newPrefabFullPath = newPrefabResourcesPath + _newUIName + strPrefab;

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

            string uiName     = GetUIName();
            string tempPath   = UIAutoCreatePathSetting.TemplateFilePath + UIAutoCreatePathSetting.ViewTemplateName;
            string targetPath = GetTargetGeneratePath(uiName);
            CheckTargetPath(targetPath);
            new UIViewAutoCreate().Create(uiName, _uiRootGo, tempPath, targetPath);
        }

        private void CreateUIControl()
        {
            if (_uiRootGo == null) throw new System.Exception("请拖入需要生成的UI预制体");

            var uiName     = GetUIName();
            var tempPath   = UIAutoCreatePathSetting.TemplateFilePath + UIAutoCreatePathSetting.ControlTemplateName;
            var targetPath = GetTargetGeneratePath(uiName);
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

        private string GetTargetGeneratePath(string uiName)
        {
            return UIAutoCreatePathSetting.GetUIGenerateCsFilePath() + "UI" + uiName + "/";
        }

        private void CheckTargetPath(string targetPath)
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
        /// 标题文字的样式
        /// </summary>
        /// <returns></returns>
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