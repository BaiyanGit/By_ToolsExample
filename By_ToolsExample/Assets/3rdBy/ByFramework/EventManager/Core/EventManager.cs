//=====================================================
// 文件名称: EventManager.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: 统一事件管理入口，负责声明式事件发布。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using Handler;
    using UnityEngine;
    using Utility;

    /// <summary>
    /// ByFramework 统一事件管理器。
    /// 支持声明式 Struct/Class/Async 事件，并通过 partial 扩展运行时监听事件。
    /// </summary>
    public partial class EventManager : MonoBehaviour
    {
        /// <summary>
        /// 获取事件管理器实例。
        /// </summary>
        public static EventManager Instance { get; private set; }

        [Header("所有声明式事件处理器类型")]
        private List<Type> _allEventTypes;

        [Header("声明式事件处理器表，Key 为事件数据类型")]
        private Dictionary<Type, List<object>> _allEvents = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            EnsureInstance();
        }

        private List<Type> GetAllAttributeTypes()
        {
            var assembly = Assembly.GetAssembly(typeof(EventAttribute));
            var types = assembly.GetExportedTypes();

            bool IsMyAttribute(IEnumerable<Attribute> o)
            {
                return o.OfType<EventAttribute>().Any();
            }

            var typeIes = types.Where(o => IsMyAttribute(Attribute.GetCustomAttributes(o, true)));
            return typeIes.Where(o => o.IsAbstract == false).ToList();
        }

        private void LoadAll()
        {
            _allEventTypes = GetAllAttributeTypes();
            foreach (Type type in _allEventTypes)
            {
                if (Activator.CreateInstance(type) is IEvent iEvent)
                {
                    Type eventType = iEvent.GetEventType();
                    if (!_allEvents.ContainsKey(eventType))
                    {
                        _allEvents.Add(eventType, new List<object>());
                    }

                    _allEvents[eventType].Add(iEvent);
                }
            }
        }

        /// <summary>
        /// 异步发布 Struct 事件。
        /// </summary>
        /// <param name="a">事件数据。</param>
        /// <typeparam name="T">事件数据类型。</typeparam>
        /// <returns>异步事件执行任务。</returns>
        public async UniTask PublishAsync<T>(T a) where T : struct
        {
            if (!_allEvents.TryGetValue(typeof(T), out var iEvents))
            {
                return;
            }

            using var list = ListComponent<UniTask>.Create();
            for (int i = 0; i < iEvents.Count; ++i)
            {
                object obj = iEvents[i];
                if (obj is not AEventAsync<T> aEvent)
                {
                    Debug.LogError($"event error: {obj.GetType().Name}");
                    continue;
                }

                list.Add(aEvent.Handle(a));
            }

            try
            {
                await UniTask.WhenAll(list);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 发布 Struct 事件。
        /// </summary>
        /// <param name="a">事件数据。</param>
        /// <typeparam name="T">事件数据类型。</typeparam>
        public void Publish<T>(T a) where T : struct
        {
            if (!_allEvents.TryGetValue(a.GetType(), out var iEvents))
            {
                return;
            }

            for (int i = 0; i < iEvents.Count; ++i)
            {
                object obj = iEvents[i];
                if (obj is not AEvent<T> aEvent)
                {
                    Debug.LogError($"event error: {obj.GetType().Name}");
                    continue;
                }

                aEvent.Handle(a);
            }
        }

        /// <summary>
        /// 发布 Class 事件，并在发布后释放事件数据。
        /// </summary>
        /// <param name="a">实现 IDisposable 的事件数据。</param>
        /// <typeparam name="T">事件数据类型。</typeparam>
        public void PublishClass<T>(T a) where T : IDisposable
        {
            if (!_allEvents.TryGetValue(a.GetType(), out var iEvents))
            {
                return;
            }

            for (int i = 0; i < iEvents.Count; ++i)
            {
                object obj = iEvents[i];
                IEventClass aEvent = (IEventClass)obj;
                aEvent.Handle(a);
            }

            a.Dispose();
        }
    }
}
