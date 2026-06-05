//=====================================================
// 文件名称: AEvent.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: Struct 类型声明式事件处理器基类。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Handler
{
    using System;
    using Core;
    using UnityEngine;

    /// <summary>
    /// Struct 类型声明式事件处理器基类。
    /// </summary>
    /// <typeparam name="A">事件数据类型。</typeparam>
    [Event]
    public abstract class AEvent<A> : IEvent where A : struct
    {
        /// <summary>
        /// 获取当前处理器对应的事件数据类型。
        /// </summary>
        /// <returns>事件数据类型。</returns>
        public Type GetEventType()
        {
            return typeof(A);
        }

        /// <summary>
        /// 执行具体事件处理逻辑。
        /// </summary>
        /// <param name="a">事件数据。</param>
        protected abstract void Run(A a);

        /// <summary>
        /// 处理 Struct 事件数据。
        /// </summary>
        /// <param name="a">事件数据。</param>
        public void Handle(A a)
        {
            try
            {
                Run(a);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
