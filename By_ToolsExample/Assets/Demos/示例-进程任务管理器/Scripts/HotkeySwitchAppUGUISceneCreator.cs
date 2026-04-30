#if UNITY_EDITOR
namespace Demos.示例_快捷键切换应用.Editor
{
    using System.IO;
    using Demos.示例_快捷键切换应用.Scripts.KeyPadAPI;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// UGUI 场景创建工具。
    /// 
    /// 菜单路径：
    /// ByTools/快捷键切换应用/创建 UGUI 示例场景
    /// 
    /// 执行后会在 Assets/Demos/示例-快捷键切换应用/Scenes 下创建：
    /// HotkeySwitchApp_UGUI.unity
    /// </summary>
    public static class HotkeySwitchAppUGUISceneCreator
    {
        private const string SceneFolder = "Assets/Demos/示例-快捷键切换应用/Scenes";
        private const string ScenePath = SceneFolder + "/HotkeySwitchApp_UGUI.unity";

        [MenuItem("ByTools/快捷键切换应用/创建 UGUI 示例场景")]
        public static void CreateUGUIScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "HotkeySwitchApp_UGUI";

                CreateCanvas();
                CreateEventSystem();
                CreateController();

                EnsureFolder(SceneFolder);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("创建完成", "UGUI 示例场景已创建：\n" + ScenePath + "\n\n运行场景后会自动生成 UGUI 面板。", "确定");
            }
        }

        private static void CreateCanvas()
        {
            GameObject canvasObject = new GameObject("HotkeySwitchApp_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void CreateController()
        {
            GameObject controller = new GameObject("HotkeySwitchApp_UGUI_Controller");
            controller.AddComponent<WindowControllerUGUI>();
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] parts = folder.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
