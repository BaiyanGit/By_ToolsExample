//=====================================================
// 文件名称: FrameworkEntry.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-05
// 描    述: ByFramework 统一启动入口，负责编排框架根节点与渐进接入模块的生命周期。
//=====================================================

namespace _3rdBy.ByFramework.Core
{
    using _3rdBy.ByFramework.Extension;
    using UnityEngine;

    /// <summary>
    /// ByFramework 统一启动入口。
    /// 负责创建持久化框架根节点，并编排已完成接入验证的模块生命周期。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FrameworkEntry : MonoBehaviour
    {
        /// <summary>
        /// 获取当前 FrameworkEntry 实例。
        /// </summary>
        public static FrameworkEntry Instance { get; private set; }

        [Header("FrameworkEntry 是否已完成第一阶段初始化")]
        private bool _isInitialized;

        [Header("FrameworkEntry 是否已完成关闭编排")]
        private bool _isShutdown;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
            {
                return;
            }

            FrameworkEntry existingEntry = FindFirstObjectByType<FrameworkEntry>();
            if (existingEntry != null)
            {
                Instance = existingEntry;
                DontDestroyOnLoad(existingEntry.gameObject);
                existingEntry.Initialize();
                return;
            }

            GameObject frameworkRoot = new("[ByFramework]");
            frameworkRoot.AddComponent<FrameworkEntry>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartCoreEarlyModules()
        {
            if (Instance == null)
            {
                return;
            }

            Instance.StartThreadDispatcher();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Shutdown();
                Instance = null;
            }
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            Debug.Log("[ByFramework][FrameworkEntry] Initialization started.");

            InitializeCoreModules();
            InitializeServiceModules();
            InitializeSceneModules();

            Debug.Log("[ByFramework][FrameworkEntry] Initialization completed.");
        }

        private void InitializeCoreModules()
        {
            DispatcherThread.RegisterForFrameworkEntry();
            Debug.Log("[ByFramework][FrameworkEntry] Core module initialization phase ready.");
        }

        private void InitializeServiceModules()
        {
            Debug.Log("[ByFramework][FrameworkEntry] Service module initialization phase ready.");
        }

        private void InitializeSceneModules()
        {
            Debug.Log("[ByFramework][FrameworkEntry] Scene module initialization phase ready.");
        }

        private void StartThreadDispatcher()
        {
            DispatcherThread.InitializeForFrameworkEntry(transform);
            DispatcherThread.StartForFrameworkEntry();
        }

        private void Shutdown()
        {
            if (_isShutdown)
            {
                return;
            }

            _isShutdown = true;
            DispatcherThread.StopForFrameworkEntry();
            DispatcherThread.ShutdownForFrameworkEntry();
            Debug.Log("[ByFramework][FrameworkEntry] Shutdown completed.");
        }
    }
}
