//=====================================================
// 文件名称: EventCallExample.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: EventManager Struct 声明式事件调用示例。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Samples
{
    using Core;
    using Handler;
    using UnityEngine;

    /// <summary>
    /// Struct 声明式事件示例数据。
    /// </summary>
    public struct StructExample
    {
        [Header("示例数值")]
        public float testValue;
    }

    /// <summary>
    /// EventManager Struct 声明式事件调用示例。
    /// </summary>
    public class EventCallExample : MonoBehaviour
    {
        [Header("待发布的示例事件数据")]
        private StructExample _structExample;

        private void Start()
        {
            _structExample.testValue = 100;
            EventManager.Instance.Publish(_structExample);
        }
    }

    /// <summary>
    /// StructExample 事件处理器示例一。
    /// </summary>
    public class EventUseExample1 : AEvent<StructExample>
    {
        /// <summary>
        /// 输出 StructExample 示例数值。
        /// </summary>
        /// <param name="a">StructExample 事件数据。</param>
        protected override void Run(StructExample a)
        {
            Debug.Log(a.testValue);
        }
    }

    /// <summary>
    /// StructExample 事件处理器示例二。
    /// </summary>
    public class EventUseExample2 : AEvent<StructExample>
    {
        /// <summary>
        /// 输出 StructExample 示例数值。
        /// </summary>
        /// <param name="a">StructExample 事件数据。</param>
        protected override void Run(StructExample a)
        {
            Debug.Log(a.testValue);
        }
    }
}
