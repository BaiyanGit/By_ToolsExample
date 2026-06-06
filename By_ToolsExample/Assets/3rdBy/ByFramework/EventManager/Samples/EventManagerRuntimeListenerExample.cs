//=====================================================
// 文件名称: EventManagerRuntimeListenerExample.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: EventManager Runtime Listener 最小验证示例。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Samples
{
    using _3rdBy.ByFramework.EventManager.Core;
    using UnityEngine;

    /// <summary>
    /// EventManager Runtime Listener 最小验证示例。
    /// 用于在 Unity Console 中验证 Type/Enum 事件和 RemoveListener 行为。
    /// </summary>
    public class EventManagerRuntimeListenerExample : MonoBehaviour
    {
        [Header("是否在 Start 时自动运行验证")] private bool _runOnStart = true;

        [Header("Type 无参事件触发次数")] private int _typeNoArgCount;

        [Header("Type 单参数事件累计值")] private int _typeIntValue;

        [Header("Enum A 事件触发次数")] private int _enumACount;

        [Header("Enum B 事件触发次数")] private int _enumBCount;

        [Header("Enum 单参数事件累计值")] private int _enumIntValue;

        [Header("EventManager 移除监听验证事件触发次数")] private int _eventManagerRemoveCount;

        private sealed class TypeNoArgEvent
        {
        }

        private sealed class TypeIntEvent
        {
        }

        private sealed class EventManagerRemoveEvent
        {
        }

        private void Start()
        {
            if (_runOnStart)
            {
                RunValidation();
            }
        }

        /// <summary>
        /// 运行 EventManager Runtime Listener 最小验证。
        /// </summary>
        [ContextMenu("运行 EventManager Runtime Listener 验证")]
        public void RunValidation()
        {
            _typeNoArgCount         = 0;
            _typeIntValue           = 0;
            _enumACount             = 0;
            _enumBCount             = 0;
            _enumIntValue           = 0;
            _eventManagerRemoveCount = 0;

            EventManager.EnsureInstance();

            ValidateTypeNoArgEvent();
            ValidateTypeIntEvent();
            ValidateEnumNoArgEventIsolation();
            ValidateEnumIntEvent();
            ValidateEventManagerRemoveListener();

            Debug.Log("[ByFramework][EventManagerRuntimeListenerExample] 验证完成。");
        }

        private void ValidateTypeNoArgEvent()
        {
            var eventManager = EventManager.Instance;

            eventManager.AddListener(typeof(TypeNoArgEvent), OnTypeNoArgEvent);
            eventManager.Broadcast(typeof(TypeNoArgEvent));
            eventManager.RemoveListener(typeof(TypeNoArgEvent), OnTypeNoArgEvent);
            eventManager.Broadcast(typeof(TypeNoArgEvent));

            LogResult("Type 无参事件", _typeNoArgCount == 1, $"实际次数={_typeNoArgCount}, 预期次数=1");
        }

        private void ValidateTypeIntEvent()
        {
            var eventManager = EventManager.Instance;

            eventManager.AddListener<int>(typeof(TypeIntEvent), OnTypeIntEvent);
            eventManager.Broadcast(typeof(TypeIntEvent), 100);
            eventManager.RemoveListener<int>(typeof(TypeIntEvent), OnTypeIntEvent);
            eventManager.Broadcast(typeof(TypeIntEvent), 50);

            LogResult("Type 单参数事件", _typeIntValue == 100, $"实际值={_typeIntValue}, 预期值=100");
        }

        private void ValidateEnumNoArgEventIsolation()
        {
            var eventManager = EventManager.Instance;

            eventManager.AddListener(EventNotice.A, OnEnumAEvent);
            eventManager.AddListener(EventNotice.B, OnEnumBEvent);
            eventManager.Broadcast(EventNotice.A);
            eventManager.RemoveListener(EventNotice.A, OnEnumAEvent);
            eventManager.RemoveListener(EventNotice.B, OnEnumBEvent);

            LogResult(
                "Enum A/B 隔离",
                _enumACount == 1 && _enumBCount == 0,
                $"A 实际次数={_enumACount}, A 预期次数=1, B 实际次数={_enumBCount}, B 预期次数=0");
        }

        private void ValidateEnumIntEvent()
        {
            var eventManager = EventManager.Instance;

            eventManager.AddListener<int>(EventNotice.HpChange, OnEnumIntEvent);
            eventManager.Broadcast(EventNotice.HpChange, 100);
            eventManager.RemoveListener<int>(EventNotice.HpChange, OnEnumIntEvent);
            eventManager.Broadcast(EventNotice.HpChange, 50);

            LogResult("Enum 单参数事件", _enumIntValue == 100, $"实际值={_enumIntValue}, 预期值=100");
        }

        private void ValidateEventManagerRemoveListener()
        {
            var eventManager = EventManager.Instance;

            eventManager.AddListener(typeof(EventManagerRemoveEvent), OnEventManagerRemoveEvent);
            eventManager.Broadcast(typeof(EventManagerRemoveEvent));
            eventManager.RemoveListener(typeof(EventManagerRemoveEvent), OnEventManagerRemoveEvent);
            eventManager.Broadcast(typeof(EventManagerRemoveEvent));

            LogResult(
                "EventManager 移除监听",
                _eventManagerRemoveCount == 1,
                $"实际次数={_eventManagerRemoveCount}, 预期次数=1");
        }

        private void OnTypeNoArgEvent()
        {
            _typeNoArgCount++;
        }

        private void OnTypeIntEvent(int value)
        {
            _typeIntValue += value;
        }

        private void OnEnumAEvent()
        {
            _enumACount++;
        }

        private void OnEnumBEvent()
        {
            _enumBCount++;
        }

        private void OnEnumIntEvent(int value)
        {
            _enumIntValue += value;
        }

        private void OnEventManagerRemoveEvent()
        {
            _eventManagerRemoveCount++;
        }

        private static void LogResult(string title, bool success, string detail)
        {
            if (success)
            {
                Debug.Log($"[ByFramework][事件管理器运行时侦听器示例] 通过：{title}（{detail}）");
                return;
            }

            Debug.LogError($"[ByFramework][事件管理器运行时侦听器示例] 失败：{title}（{detail}）");
        }
    }
}
