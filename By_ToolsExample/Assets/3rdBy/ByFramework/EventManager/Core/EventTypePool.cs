//=====================================================
// 文件名称: EventTypePool.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: EventManager 内部按类型复用对象的轻量对象池。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// EventManager 内部按类型复用对象的轻量对象池。
    /// </summary>
    public class EventTypePool : IDisposable
    {
        /// <summary>
        /// 对象池数据。纯逻辑类不引入 UnityEngine，因此不使用 HeaderAttribute。
        /// </summary>
        private readonly Dictionary<Type, Queue<object>> _pool = new Dictionary<Type, Queue<object>>();

        /// <summary>
        /// 获取全局对象池实例。纯逻辑类不引入 UnityEngine，因此不使用 HeaderAttribute。
        /// </summary>
        public static EventTypePool Instance = new EventTypePool();

        private EventTypePool()
        {
        }

        /// <summary>
        /// 获取指定类型的对象实例。
        /// </summary>
        /// <param name="type">需要获取的对象类型。</param>
        /// <returns>对象池中的对象；如果池中没有可复用对象，则创建新对象。</returns>
        public object Fetch(Type type)
        {
            if (!_pool.TryGetValue(type, out var queue))
            {
                return Activator.CreateInstance(type);
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }

            return queue.Dequeue();
        }

        /// <summary>
        /// 回收对象到对象池。
        /// </summary>
        /// <param name="obj">需要回收的对象实例。</param>
        public void Recycle(object obj)
        {
            var type = obj.GetType();
            if (!_pool.TryGetValue(type, out var queue))
            {
                queue = new Queue<object>();
                _pool.Add(type, queue);
            }

            queue.Enqueue(obj);
        }

        /// <summary>
        /// 清空对象池。
        /// </summary>
        public void Dispose()
        {
            _pool.Clear();
        }
    }
}
