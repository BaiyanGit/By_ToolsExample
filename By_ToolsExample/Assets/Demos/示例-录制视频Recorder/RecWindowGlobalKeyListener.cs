namespace Demos.示例_录制视频Recorder
{
    using Scripts.UISettings;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class RecorderGlobalKeyListener : MonoBehaviour
    {
        [Header("加载键")] public KeyCode recordKey = KeyCode.F12;
        [Header("组合键")] public KeyCode combineKey = KeyCode.LeftShift;

        private static GameObject _container;
        private const string SCENE_NAME = "录制视频Recorder";
        private bool _isSceneLoaded;

        private GameObject _recordWindow;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            if (_container != null) return;

            _container = new GameObject($"[🔴 Rec] {nameof(RecorderGlobalKeyListener)}");
            _container.AddComponent<RecorderGlobalKeyListener>();
            DontDestroyOnLoad(_container);
            // Debug.Log("全局按键监听器已启动。");
        }

        private void Update()
        {
            if (!Input.GetKey(combineKey))
            {
                return;
            }

            // 在这里检测你想要的按键，例如按下 F1 键时加载名为 "MyScene" 的场景
            if (Input.GetKeyDown(recordKey))
            {
                ShowOrHideRecordWindow();
            }
        }

        private void ShowOrHideRecordWindow()
        {
            // 只有一个自己的场景时，对象隐藏打开
            // 多个场景时，检测这个场景是否加载，如果加载，则卸载，否则加载

            int loadedSceneCount = SceneManager.sceneCount;
            if (loadedSceneCount == 1 && SceneManager.GetSceneByName(SCENE_NAME).isLoaded)
            {
                Debug.Log("不能卸载：只有一个自己的场景，激活或隐藏窗口...");
                if (!_recordWindow)
                {
                    var screenRecorder = FindFirstObjectByType<UIRecorderParamsSettings>(FindObjectsInactive.Include);
                    Debug.Log($"找到录制器：{screenRecorder}");
                    if (screenRecorder)
                    {
                        _recordWindow = screenRecorder.gameObject;
                    }
                }

                if (_recordWindow)
                {
                    Debug.Log("打开窗口...");
                    _recordWindow.SetActive(!_recordWindow.activeSelf);
                }
            }
            else
            {
                // 如果已经被加载，则卸载它；否则什么都不做。
                if (SceneManager.GetSceneByName(SCENE_NAME).isLoaded)
                {
                    Debug.Log("快捷键触发，准备卸载场景...");
                    SceneManager.UnloadSceneAsync(SCENE_NAME);
                }
                else
                {
                    Debug.Log("快捷键触发，准备加载场景...");
                    SceneManager.LoadScene(SCENE_NAME, LoadSceneMode.Additive);
                }
            }
        }

        // private static void PrintSceneInfo()
        // {
        //     int loadedSceneCount = SceneManager.sceneCount;
        //     for (int i = 0; i < loadedSceneCount; i++)
        //     {
        //         var scene = SceneManager.GetSceneAt(i);
        //         Debug.Log($"({loadedSceneCount}){i}. 场景：{scene.name}，已加载：{scene.isLoaded}");
        //     }
        // }
    }
}