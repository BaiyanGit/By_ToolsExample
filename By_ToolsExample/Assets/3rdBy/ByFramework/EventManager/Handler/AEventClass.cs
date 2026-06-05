//=====================================================
// 文件名称: AEventClass.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: Class 类型声明式事件处理器基类。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Handler
{
    using System;
    using Core;
    using UnityEngine;

    /// <summary>
    /// Class 类型声明式事件处理器基类。
    /// </summary>
    /// <typeparam name="A">事件数据类型。</typeparam>
    [Event]
    public abstract class AEventClass<A> : IEventClass where A : class
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
        /// <param name="a">事件数据对象。</param>
        protected abstract void Run(object a);

        /// <summary>
        /// 处理 Class 事件数据。
        /// </summary>
        /// <param name="a">事件数据对象。</param>
        public void Handle(object a)
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
