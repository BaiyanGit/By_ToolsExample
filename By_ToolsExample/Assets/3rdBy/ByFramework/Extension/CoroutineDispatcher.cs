namespace _3rdBy.ByFramework.Guide
{
    using System;
    using System.Collections;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// 协程调度器（全局静态单例）
    /// 作用：提供非MonoBehaviour脚本启动/停止协程的统一入口
    /// 与 ThreadDispatcher 保持框架命名风格统一
    /// </summary>
    public static class CoroutineDispatcher
    {
        /// <summary>
        /// 内部协程宿主组件
        /// 用于真正承载Unity协程运行
        /// </summary>
        private class InnerCoroutineHost : MonoBehaviour
        {
        }

        /// <summary>
        /// 全局唯一的协程宿主实例
        /// </summary>
        private static readonly InnerCoroutineHost coroutineHost;

        /// <summary>
        /// 静态构造：初始化协程调度器
        /// 创建宿主对象并设置跨场景不销毁
        /// </summary>
        static CoroutineDispatcher()
        {
            coroutineHost = new GameObject("CoroutineDispatcher").AddComponent<InnerCoroutineHost>();
            Object.DontDestroyOnLoad(coroutineHost);
        }

        /// <summary>
        /// 启动一个协程任务
        /// </summary>
        /// <param name="routine">协程迭代器</param>
        /// <returns>协程对象</returns>
        public static Coroutine StartCoroutineTask(IEnumerator routine)
        {
            return coroutineHost.StartCoroutine(routine);
        }

        /// <summary>
        /// 内部协程包装器：协程执行完成后触发回调
        /// </summary>
        private static IEnumerator StartInnerCoroutine(IEnumerator routine, Action<object> callback)
        {
            yield return StartCoroutineTask(routine);
            callback?.Invoke(routine.Current);
        }

        /// <summary>
        /// 启动带完成回调的协程任务
        /// </summary>
        /// <param name="routine">协程迭代器</param>
        /// <param name="callback">执行完成回调</param>
        public static void StartCoroutineTask(IEnumerator routine, Action<object> callback)
        {
            StartCoroutineTask(StartInnerCoroutine(routine, callback));
        }

        /// <summary>
        /// 根据协程对象停止指定协程
        /// </summary>
        public static void StopOtherCoroutine(Coroutine c)
        {
            coroutineHost.StopCoroutine(c);
        }

        /// <summary>
        /// 根据迭代器停止指定协程
        /// </summary>
        public static void StopCoroutineTask(IEnumerator routine)
        {
            coroutineHost.StopCoroutine(routine);
        }

        /// <summary>
        /// 停止所有正在运行的协程任务
        /// </summary>
        public static void StopAllCoroutineTask()
        {
            coroutineHost.StopAllCoroutines();
        }
    }
}