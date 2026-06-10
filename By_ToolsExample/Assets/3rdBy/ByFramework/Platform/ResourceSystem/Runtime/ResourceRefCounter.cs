//=====================================================
// 文件名称: ResourceRefCounter.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem Runtime 引用计数器。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System.Threading;

    /// <summary>
    /// 线程安全的资源引用计数器。
    /// </summary>
    public sealed class ResourceRefCounter
    {
        private int _count;

        /// <summary>
        /// 获取当前引用计数。
        /// </summary>
        public int CurrentCount => Volatile.Read(ref _count);

        /// <summary>
        /// 增加引用计数。
        /// </summary>
        public int Increment()
        {
            return Interlocked.Increment(ref _count);
        }

        /// <summary>
        /// 减少引用计数。
        /// </summary>
        public int Decrement()
        {
            int nextCount = Interlocked.Decrement(ref _count);
            if (nextCount >= 0)
            {
                return nextCount;
            }

            Interlocked.Exchange(ref _count, 0);
            return 0;
        }
    }
}
