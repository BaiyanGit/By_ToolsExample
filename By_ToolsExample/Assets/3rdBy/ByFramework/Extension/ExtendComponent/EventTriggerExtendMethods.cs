namespace _3rdBy.ByFramework.Extension.ExtendComponent
{
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    /// <summary>
    /// EventTrigger扩展方法
    /// </summary>
    public static class EventTriggerExtendMethods
    {
        /// <summary>
        /// EventTrigger中注册事件
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="eventType"></param>
        /// <param name="callback"></param>
        public static void RegisterEvent(this EventTrigger trigger, EventTriggerType eventType,
            UnityAction<BaseEventData> callback)
        {
            // 查找是否已经存在要注册的事件，如果这个事件不存在，就创建新的实例
            var entry = trigger.triggers.FirstOrDefault(existingEntry => existingEntry.eventID == eventType) ??
                        new EventTrigger.Entry { eventID = eventType };

            // 添加触发回调并注册事件
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
        }

        /// <summary>
        /// 在本对象上添加EventTrigger组件并注册
        /// </summary>
        /// <param name="self"></param>
        /// <param name="eventType"></param>
        /// <param name="callback"></param>
        public static void RegisterEvent(this GameObject self, EventTriggerType eventType,
            UnityAction<BaseEventData> callback)
        {
            var eventTrigger = self.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = self.AddComponent<EventTrigger>();
            }

            eventTrigger.RegisterEvent(eventType, callback);
        }
    }
}