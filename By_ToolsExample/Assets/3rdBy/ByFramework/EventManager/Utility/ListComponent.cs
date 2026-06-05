//=====================================================
// 文件名称: ListComponent.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: 可回收的 List 组件，用于 EventManager 内部减少临时分配。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Utility
{
    using System;
    using System.Collections.Generic;
    using Core;

    /// <summary>
    /// 可回收的 List 组件。
    /// </summary>
    /// <typeparam name="T">列表元素类型。</typeparam>
    public class ListComponent<T> : List<T>, IDisposable
    {
        /// <summary>
        /// 从对象池中创建或获取 ListComponent 实例。
        /// </summary>
        /// <returns>可使用的 ListComponent 实例。</returns>
        public static ListComponent<T> Create()
        {
            return EventTypePool.Instance.Fetch(typeof(ListComponent<T>)) as ListComponent<T>;
        }

        /// <summary>
        /// 清空当前列表并回收到对象池。
        /// </summary>
        public void Dispose()
        {
            Clear();
            EventTypePool.Instance.Recycle(this);
        }
    }
}
