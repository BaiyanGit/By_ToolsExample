//=====================================================
// 文件名称: EventAttribute.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: 标记 EventManager 声明式事件处理器类型。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Core
{
    using System;

    /// <summary>
    /// EventManager 声明式事件处理器标记。
    /// 被标记的非抽象事件处理器会在 EventManager 初始化时被扫描并注册。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventAttribute : Attribute
    {
    }
}
