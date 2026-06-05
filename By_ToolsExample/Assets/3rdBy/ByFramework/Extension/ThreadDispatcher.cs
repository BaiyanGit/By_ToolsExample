/*
 * 作用：线程之间调度任务，将任务从子线程调度回主线程。
 * 作者：王柏雁
 * 日期：2024/8/20
 * Tips: 最大允许的并发线程数可以设置为 CPU 核心数的 2 倍左右
 */

namespace _3rdBy.ByFramework.Extension
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;

    /// <summary>
    /// 线程调度器
    /// </summary>
    public class ThreadDispatcher : MonoBehaviour
    {
        public static int maxThreads = 6; // 最大允许的并发线程数
        public static ThreadDispatcher Current { get; private set; } // 当前的Loom实例（单例模式）

        // 在场景加载前初始化Loom
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            maxThreads = Environment.ProcessorCount; // 设置最大线程数为处理器核心数
            Debug.Log($"Loom 线程数量, {maxThreads}");

            // 创建一个新的GameObject来承载Loom脚本
            var go = new GameObject
            {
                name = "Loom"
            };
            Current = go.AddComponent<ThreadDispatcher>();
            // 确保该对象在场景切换时不会被销毁
            DontDestroyOnLoad(go);
        }

        /// <summary>
        /// 延迟执行任务的结构体
        /// </summary>
        public struct DelayedQueueItem
        {
            public float time; // 任务执行时间
            public Action action; // 任务操作
        }

        private readonly List<DelayedQueueItem> _delayed = new(); // 延迟任务列表
        private readonly List<Action> _actions = new(); // 主线程任务列表
        private readonly List<Action> _curActions = new(); // 临时列表，保存当前要执行的主线程任务
        private readonly List<DelayedQueueItem> _curDelayeds = new(); // 临时列表，保存当前要执行的延迟任务
        private static int _numThreads; // 当前活动的线程数
        private int _count; // 用于在Update中执行的主线程任务计数

        /// <summary>
        /// 将任务加入主线程队列中执行
        /// </summary>
        /// <param name="action"></param>
        public static void QueueOnMainThread(Action action)
        {
            QueueOnMainThread(action, 0f);
        }

        // 将带参数的任务加入主线程队列中执行
        public static void QueueOnMainThread(Action<object> action, object p)
        {
            QueueOnMainThread(action, p, 0f);
        }

        /// <summary>
        /// 将任务加入主线程队列，并可以设置延迟时间
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        public static void QueueOnMainThread(Action action, float time)
        {
            if (time > 0f)
            {
                // 如果有延迟，将任务加入延迟队列
                lock (Current._delayed)
                {
                    Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
                }
            }
            else
            {
                // 没有延迟，直接加入主线程任务列表
                lock (Current._actions)
                {
                    Current._actions.Add(action);
                }
            }
        }

        /// <summary>
        /// 将带参数的任务加入主线程队列，并可以设置延迟时间
        /// </summary>
        /// <param name="action"></param>
        /// <param name="p"></param>
        /// <param name="time"></param>
        public static void QueueOnMainThread(Action<object> action, object p, float time)
        {
            QueueOnMainThread(() => action(p), time);
        }

        /// <summary>
        /// 在线程池中异步执行任务
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Thread RunAsync(Action a)
        {
            // 如果当前活动线程数超过最大值，等待空闲线程
            while (_numThreads >= maxThreads)
            {
                Thread.Sleep(1);
            }

            Interlocked.Increment(ref _numThreads); // 增加活动线程计数
            ThreadPool.QueueUserWorkItem(RunAction, a); // 将任务加入线程池
            return null;
        }

        /// <summary>
        /// 线程池执行的任务
        /// </summary>
        /// <param name="action"></param>
        private static void RunAction(object action)
        {
            try
            {
                ((Action)action)(); // 执行任务
            }
            catch (Exception e)
            {
                Debug.LogError(e); // 捕获并记录异常
            }
            finally
            {
                Interlocked.Decrement(ref _numThreads); // 减少活动线程计数
            }
        }

        /// <summary>
        /// 每帧更新时执行主线程任务
        /// </summary>
        private void Update()
        {
            // 处理主线程任务列表
            lock (_actions)
            {
                if (_actions.Count > 0)
                {
                    _curActions.AddRange(_actions);
                    _actions.Clear();
                }
            }

            // 执行当前帧的主线程任务
            if (_curActions.Count > 0)
            {
                foreach (var a in _curActions)
                {
                    try
                    {
                        a();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                _curActions.Clear();
            }

            // 处理延迟任务列表
            lock (_delayed)
            {
                var i = _delayed.Count - 1;
                while (i >= 0)
                {
                    var item = _delayed[i];
                    if (item.time <= Time.time)
                    {
                        _curDelayeds.Add(item);
                        _delayed.RemoveAt(i);
                    }

                    i--;
                }
            }

            // 执行当前帧的延迟任务
            if (_curDelayeds.Count > 0)
            {
                foreach (var item in _curDelayeds)
                {
                    try
                    {
                        item.action();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                _curDelayeds.Clear();
            }
        }
    }
}