//=====================================================
// 文件名称: DispatcherCoroutine.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-05
// 描    述: 全局协程调度器，允许在非 MonoBehaviour 中启动/停止协程
//=====================================================

namespace _3rdBy.ByFramework.Guide
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// 协程调度器
    /// 与 ThreadDispatcher 机制保持统一
    /// </summary>
    public class DispatcherCoroutine : MonoBehaviour
    {
        public static DispatcherCoroutine Current { get; private set; } // 单例实例

        /// <summary>
        /// 场景加载前自动初始化协程调度器
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (Current != null)
                return;

            // 查找场景中已存在的调度器
            var existing = FindFirstObjectByType<DispatcherCoroutine>();
            if (existing != null)
            {
                Current = existing;
                DontDestroyOnLoad(existing.gameObject);
                return;
            }

            // 创建新 GameObject
            var container = new GameObject("[DspCoroutine]");
            Current = container.AddComponent<DispatcherCoroutine>();

            // 自动挂到 [ByFramework] 节点下（和 ThreadDispatcher 一致）
            var parentGo = GameObject.Find("[ByFramework]");
            if (parentGo != null)
            {
                container.transform.SetParent(parentGo.transform);
            }

            // 跨场景不销毁
            DontDestroyOnLoad(container);
        }

        /// <summary>
        /// 重置静态状态（防止重载域异常）
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Current = null;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        /// <summary>
        /// 启动协程
        /// </summary>
        public static Coroutine StartCoroutineTask(IEnumerator routine)
        {
            return Current.StartCoroutine(routine);
        }

        /// <summary>
        /// 启动带完成回调的协程
        /// </summary>
        public static void StartCoroutineTask(IEnumerator routine, Action<object> callback)
        {
            StartCoroutineTask(StartInnerCoroutine(routine, callback));
        }

        /// <summary>
        /// 协程执行完毕后触发回调
        /// </summary>
        private static IEnumerator StartInnerCoroutine(IEnumerator routine, Action<object> callback)
        {
            yield return StartCoroutineTask(routine);
            callback?.Invoke(routine.Current);
        }

        /// <summary>
        /// 根据协程对象停止协程
        /// </summary>
        public static void StopOtherCoroutine(Coroutine c)
        {
            Current.StopCoroutine(c);
        }

        /// <summary>
        /// 根据迭代器停止协程
        /// </summary>
        public static void StopCoroutineTask(IEnumerator routine)
        {
            Current.StopCoroutine(routine);
        }

        /// <summary>
        /// 停止所有协程
        /// </summary>
        public static void StopAllCoroutineTask()
        {
            Current.StopAllCoroutines();
        }
    }
}