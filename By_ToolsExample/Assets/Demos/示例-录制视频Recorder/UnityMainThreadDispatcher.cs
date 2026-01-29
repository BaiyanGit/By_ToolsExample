namespace Demos.示例_录制视频Recorder
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 简单的主线程调度器：允许从任意线程将 Action 投递到 Unity 主线程执行。
    /// 自动以单例方式存在（若场景中没有，会在第一次访问时创建 GameObject）。
    /// 用法：
    ///   UnityMainThreadDispatcher.Instance.Enqueue(() => { /* Unity API 调用 */ });
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;

        public static UnityMainThreadDispatcher Instance
        {
            get
            {
                if (_instance != null) return _instance;

                // 尝试在场景中查找
                _instance = FindObjectOfType<UnityMainThreadDispatcher>();
                if (_instance != null) return _instance;

                // 否则创建一个新的 GameObject（确保在主线程中调用）
                var go = new GameObject("UnityMainThreadDispatcher");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                return _instance;
            }
        }

        // 使用 ConcurrentQueue 以便线程安全地从后台线程 Enqueue
        private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

        // 若需要在下一帧执行（或者延迟执行），可以扩展为 List<Action> 等。
        // 目前我们在 Update 中一次性将队列中所有 Action 拿出来执行，避免频繁切换上下文。

        /// <summary>
        /// 将 action 投递到主线程执行。
        /// </summary>
        public void Enqueue(Action action)
        {
            if (action == null) return;
            _actions.Enqueue(action);
        }

        /// <summary>
        /// 同步尝试（最好不要在主线程外调用）。
        /// </summary>
        public void EnqueueToFront(Action action)
        {
            // 简单实现：直接 Enqueue（若需要放到队列前面，可改为锁 + List 插入）
            Enqueue(action);
        }

        private void Update()
        {
            if (_actions.IsEmpty) return;

            // 将当前队列全部取出到临时列表（减少在执行过程中队列变化的影响）
            var localList = new List<Action>();
            while (_actions.TryDequeue(out var act))
            {
                localList.Add(act);
            }

            foreach (var a in localList)
            {
                try
                {
                    a?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private void OnDestroy()
        {
            // 清理一下 instance 引用（下次访问会重新创建）
            if (_instance == this) _instance = null;
        }
    }
}