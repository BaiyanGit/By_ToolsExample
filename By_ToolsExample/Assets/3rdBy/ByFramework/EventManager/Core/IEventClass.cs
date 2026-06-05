//=====================================================
// 文件名称: IEventClass.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: Class 类型声明式事件处理器接口。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Core
{
    /// <summary>
    /// Class 类型声明式事件处理器接口。
    /// </summary>
    public interface IEventClass : IEvent
    {
        /// <summary>
        /// 处理 Class 事件数据。
        /// </summary>
        /// <param name="a">事件数据对象。</param>
        void Handle(object a);
    }
}
