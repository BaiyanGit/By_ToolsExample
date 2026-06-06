//=====================================================
// 文件名称: EventManager.RuntimeListener.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: EventManager 的运行时监听事件扩展，支持 Type 与 Enum 事件。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// EventManager 运行时监听事件扩展。
    /// Type 事件使用 Type 本身作为 Key，Enum 事件使用枚举实例本身作为 Key。
    /// </summary>
    public partial class EventManager
    {
        [Header("运行时监听事件表，Type 事件使用 Type，Enum 事件使用枚举实例")]
        private readonly Dictionary<object, Delegate> _runtimeEventTable = new();

        /// <summary>
        /// 确保事件管理器实例存在。
        /// </summary>
        /// <returns>事件管理器实例。</returns>
        public static void EnsureInstance()
        {
            if (Instance != null)
            {
                return;
            }

            var parentGo = GameObject.Find("[ByFramework]");
            RegisterForFrameworkEntry();
            InitializeForFrameworkEntry(parentGo != null ? parentGo.transform : null);
            StartForFrameworkEntry();
        }

        /// <summary>
        /// 添加 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        public void AddListener(Type eventType, CallBack callBack)
        {
            AddListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 添加 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        public void AddListener<T>(Type eventType, CallBack<T> callBack)
        {
            AddListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 添加 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        public void AddListener<T, TX>(Type eventType, CallBack<T, TX> callBack)
        {
            AddListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 添加 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        public void AddListener<T, TX, TY>(Type eventType, CallBack<T, TX, TY> callBack)
        {
            AddListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 添加 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        public void AddListener<T, TX, TY, TZ>(Type eventType, CallBack<T, TX, TY, TZ> callBack)
        {
            AddListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 添加 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        /// <typeparam name="TW">第五个参数类型。</typeparam>
        public void AddListener<T, TX, TY, TZ, TW>(Type eventType, CallBack<T, TX, TY, TZ, TW> callBack)
        {
            AddListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 添加 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        public void AddListener(Enum eventId, CallBack callBack)
        {
            AddListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 添加 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        public void AddListener<T>(Enum eventId, CallBack<T> callBack)
        {
            AddListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 添加 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        public void AddListener<T, TX>(Enum eventId, CallBack<T, TX> callBack)
        {
            AddListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 添加 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        public void AddListener<T, TX, TY>(Enum eventId, CallBack<T, TX, TY> callBack)
        {
            AddListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 添加 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        public void AddListener<T, TX, TY, TZ>(Enum eventId, CallBack<T, TX, TY, TZ> callBack)
        {
            AddListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 添加 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        /// <typeparam name="TW">第五个参数类型。</typeparam>
        public void AddListener<T, TX, TY, TZ, TW>(Enum eventId, CallBack<T, TX, TY, TZ, TW> callBack)
        {
            AddListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 移除 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        public void RemoveListener(Type eventType, CallBack callBack)
        {
            RemoveListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 移除 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        public void RemoveListener<T>(Type eventType, CallBack<T> callBack)
        {
            RemoveListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 移除 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        public void RemoveListener<T, TX>(Type eventType, CallBack<T, TX> callBack)
        {
            RemoveListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 移除 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        public void RemoveListener<T, TX, TY>(Type eventType, CallBack<T, TX, TY> callBack)
        {
            RemoveListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 移除 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        public void RemoveListener<T, TX, TY, TZ>(Type eventType, CallBack<T, TX, TY, TZ> callBack)
        {
            RemoveListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 移除 Type 事件监听。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        /// <typeparam name="TW">第五个参数类型。</typeparam>
        public void RemoveListener<T, TX, TY, TZ, TW>(Type eventType, CallBack<T, TX, TY, TZ, TW> callBack)
        {
            RemoveListenerInternal(eventType, callBack);
        }

        /// <summary>
        /// 移除 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        public void RemoveListener(Enum eventId, CallBack callBack)
        {
            RemoveListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 移除 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        public void RemoveListener<T>(Enum eventId, CallBack<T> callBack)
        {
            RemoveListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 移除 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        public void RemoveListener<T, TX>(Enum eventId, CallBack<T, TX> callBack)
        {
            RemoveListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 移除 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        public void RemoveListener<T, TX, TY>(Enum eventId, CallBack<T, TX, TY> callBack)
        {
            RemoveListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 移除 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        public void RemoveListener<T, TX, TY, TZ>(Enum eventId, CallBack<T, TX, TY, TZ> callBack)
        {
            RemoveListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 移除 Enum 事件监听。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="callBack">事件回调。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        /// <typeparam name="TW">第五个参数类型。</typeparam>
        public void RemoveListener<T, TX, TY, TZ, TW>(Enum eventId, CallBack<T, TX, TY, TZ, TW> callBack)
        {
            RemoveListenerInternal(eventId, callBack);
        }

        /// <summary>
        /// 广播 Type 事件。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        public void Broadcast(Type eventType)
        {
            BroadcastInternal<CallBack>(eventType, callback => callback());
        }

        /// <summary>
        /// 广播 Type 事件。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        public void Broadcast<T>(Type eventType, T arg1)
        {
            BroadcastInternal<CallBack<T>>(eventType, callback => callback(arg1));
        }

        /// <summary>
        /// 广播 Type 事件。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <param name="arg2">第二个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        public void Broadcast<T, TX>(Type eventType, T arg1, TX arg2)
        {
            BroadcastInternal<CallBack<T, TX>>(eventType, callback => callback(arg1, arg2));
        }

        /// <summary>
        /// 广播 Type 事件。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <param name="arg2">第二个事件参数。</param>
        /// <param name="arg3">第三个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        public void Broadcast<T, TX, TY>(Type eventType, T arg1, TX arg2, TY arg3)
        {
            BroadcastInternal<CallBack<T, TX, TY>>(eventType, callback => callback(arg1, arg2, arg3));
        }

        /// <summary>
        /// 广播 Type 事件。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <param name="arg2">第二个事件参数。</param>
        /// <param name="arg3">第三个事件参数。</param>
        /// <param name="arg4">第四个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        public void Broadcast<T, TX, TY, TZ>(Type eventType, T arg1, TX arg2, TY arg3, TZ arg4)
        {
            BroadcastInternal<CallBack<T, TX, TY, TZ>>(eventType, callback => callback(arg1, arg2, arg3, arg4));
        }

        /// <summary>
        /// 广播 Type 事件。
        /// </summary>
        /// <param name="eventType">事件类型 Key。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <param name="arg2">第二个事件参数。</param>
        /// <param name="arg3">第三个事件参数。</param>
        /// <param name="arg4">第四个事件参数。</param>
        /// <param name="arg5">第五个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        /// <typeparam name="TW">第五个参数类型。</typeparam>
        public void Broadcast<T, TX, TY, TZ, TW>(Type eventType, T arg1, TX arg2, TY arg3, TZ arg4, TW arg5)
        {
            BroadcastInternal<CallBack<T, TX, TY, TZ, TW>>(eventType, callback => callback(arg1, arg2, arg3, arg4, arg5));
        }

        /// <summary>
        /// 广播 Enum 事件。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        public void Broadcast(Enum eventId)
        {
            BroadcastInternal<CallBack>(eventId, callback => callback());
        }

        /// <summary>
        /// 广播 Enum 事件。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        public void Broadcast<T>(Enum eventId, T arg1)
        {
            BroadcastInternal<CallBack<T>>(eventId, callback => callback(arg1));
        }

        /// <summary>
        /// 广播 Enum 事件。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <param name="arg2">第二个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        public void Broadcast<T, TX>(Enum eventId, T arg1, TX arg2)
        {
            BroadcastInternal<CallBack<T, TX>>(eventId, callback => callback(arg1, arg2));
        }

        /// <summary>
        /// 广播 Enum 事件。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <param name="arg2">第二个事件参数。</param>
        /// <param name="arg3">第三个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        public void Broadcast<T, TX, TY>(Enum eventId, T arg1, TX arg2, TY arg3)
        {
            BroadcastInternal<CallBack<T, TX, TY>>(eventId, callback => callback(arg1, arg2, arg3));
        }

        /// <summary>
        /// 广播 Enum 事件。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <param name="arg2">第二个事件参数。</param>
        /// <param name="arg3">第三个事件参数。</param>
        /// <param name="arg4">第四个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        public void Broadcast<T, TX, TY, TZ>(Enum eventId, T arg1, TX arg2, TY arg3, TZ arg4)
        {
            BroadcastInternal<CallBack<T, TX, TY, TZ>>(eventId, callback => callback(arg1, arg2, arg3, arg4));
        }

        /// <summary>
        /// 广播 Enum 事件。
        /// </summary>
        /// <param name="eventId">枚举事件 Key，使用枚举实例本身区分不同枚举值。</param>
        /// <param name="arg1">第一个事件参数。</param>
        /// <param name="arg2">第二个事件参数。</param>
        /// <param name="arg3">第三个事件参数。</param>
        /// <param name="arg4">第四个事件参数。</param>
        /// <param name="arg5">第五个事件参数。</param>
        /// <typeparam name="T">第一个参数类型。</typeparam>
        /// <typeparam name="TX">第二个参数类型。</typeparam>
        /// <typeparam name="TY">第三个参数类型。</typeparam>
        /// <typeparam name="TZ">第四个参数类型。</typeparam>
        /// <typeparam name="TW">第五个参数类型。</typeparam>
        public void Broadcast<T, TX, TY, TZ, TW>(Enum eventId, T arg1, TX arg2, TY arg3, TZ arg4, TW arg5)
        {
            BroadcastInternal<CallBack<T, TX, TY, TZ, TW>>(eventId, callback => callback(arg1, arg2, arg3, arg4, arg5));
        }

        private void AddListenerInternal(object eventKey, Delegate callBack)
        {
            if (eventKey == null) throw new ArgumentNullException(nameof(eventKey));
            if (callBack == null) throw new ArgumentNullException(nameof(callBack));

            // 运行时监听表统一使用 object Key：Type 事件直接使用 Type，Enum 事件直接使用枚举值。
            // 这样可以避免同一枚举类型下的不同枚举值共用同一个事件通道。
            _runtimeEventTable.TryAdd(eventKey, null);

            var currentDelegate = _runtimeEventTable[eventKey];
            if (currentDelegate != null && currentDelegate.GetType() != callBack.GetType())
            {
                throw new Exception($"尝试为事件{eventKey}添加不同类型的委托，当前委托类型是{currentDelegate.GetType()}，要添加的委托类型是{callBack.GetType()}");
            }

            _runtimeEventTable[eventKey] = Delegate.Combine(currentDelegate, callBack);
        }

        private void RemoveListenerInternal(object eventKey, Delegate callBack)
        {
            if (eventKey == null) throw new ArgumentNullException(nameof(eventKey));
            if (callBack == null) throw new ArgumentNullException(nameof(callBack));

            if (!_runtimeEventTable.ContainsKey(eventKey))
            {
                throw new Exception($"移除监听错误，没有事件：{eventKey}");
            }

            _runtimeEventTable[eventKey] = Delegate.Remove(_runtimeEventTable[eventKey], callBack);

            // 委托移除为空时清理 Key，避免后续广播保留空事件槽。
            if (_runtimeEventTable[eventKey] == null)
            {
                _runtimeEventTable.Remove(eventKey);
            }
        }

        private void BroadcastInternal<TDelegate>(object eventKey, Action<TDelegate> invokeAction)
            where TDelegate : class
        {
            if (eventKey == null) throw new ArgumentNullException(nameof(eventKey));

            if (!_runtimeEventTable.TryGetValue(eventKey, out var currentDelegate))
            {
                return;
            }

            if (currentDelegate is TDelegate callBack)
            {
                invokeAction(callBack);
            }
        }

        private void ClearRuntimeEventTable()
        {
            _runtimeEventTable.Clear();
        }
    }
    
}
