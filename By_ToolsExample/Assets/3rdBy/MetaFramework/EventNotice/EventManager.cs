namespace _3rdBy.MetaFramework.EventNotice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            var container = new GameObject("[Event]");
            Instance = container.AddComponent<EventManager>();
            Instance.LoadAll();
            DontDestroyOnLoad(container);
        }

        private List<Type> _allEventTypes;
        private Dictionary<Type, List<object>> _allEvents = new();

        private List<Type> GetAllAttributeTypes()
        {
            //标签查找
            var assembly = Assembly.GetAssembly(typeof(EventAttribute));
            var types = assembly.GetExportedTypes();

            bool IsMyAttribute(IEnumerable<Attribute> o)
            {
                return o.OfType<EventAttribute>().Any();
            }

            var typeIes = types.Where(o => IsMyAttribute(Attribute.GetCustomAttributes(o, true)));
            //去除abstract父类
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