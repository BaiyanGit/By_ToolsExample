using System;
using System.Collections.Generic;

namespace ZCustom
{
    public class TypePool : IDisposable
    {
        private readonly Dictionary<Type, Queue<object>> _pool = new Dictionary<Type, Queue<object>>();

        public static TypePool Instance = new TypePool();

        private TypePool()
        {
        }

        public object Fetch(Type type)
        {
            Queue<object> queue = null;
            if (!_pool.TryGetValue(type, out queue))
            {
                return Activator.CreateInstance(type);
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }

            return queue.Dequeue();
        }

        public void Recycle(object obj)
        {
            Type type = obj.GetType();
            Queue<object> queue = null;
            if (!_pool.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                _pool.Add(type, queue);
            }

            queue.Enqueue(obj);
        }

        public void Dispose()
        {
            this._pool.Clear();
        }
    }
}