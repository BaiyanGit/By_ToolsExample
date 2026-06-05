//=====================================================
// 文件名称: IEvent.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: 声明式事件处理器基础接口。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Core
{
    using System;

    /// <summary>
    /// 声明式事件处理器基础接口。
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// 获取当前处理器对应的事件数据类型。
        /// </summary>
        /// <returns>事件数据类型。</returns>
        Type GetEventType();
    }
}
