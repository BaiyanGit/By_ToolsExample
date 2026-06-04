/*
 * FileName:    EventCenter
 * Description: 事件中心类，添加、删除、分发事件
 */

namespace _3rdBy.MetaFramework.EventCenter
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    #region 无参数委托

    public delegate void CallBack();

    #endregion

    #region 带参数委托

    public delegate void CallBack<in T>(T arg);

    public delegate void CallBack<in T, in TX>(T arg1, TX arg2);

    public delegate void CallBack<in T, in TX, in TY>(T arg1, TX arg2, TY arg3);

    public delegate void CallBack<in T, in TX, in TY, in TZ>(T arg1, TX arg2, TY arg3, TZ arg4);

    public delegate void CallBack<in T, in TX, in TY, in TZ, in TW>(T arg1, TX arg2, TY arg3, TZ arg4, TW arg5);

    #endregion


    public class EventCenter : MonoBehaviour
    {
        /// <summary>
        /// 字典，用于存放事件码和委托对应
        /// </summary>
        private static readonly Dictionary<Type, Delegate> eventTable = new();

        #region 添加和移除事件前的判断

        /// <summary>
        /// 添加事件监听前的判断
        /// </summary>
        /// <param name="eventEnum"></param>
        /// <param name="callBack"></param>
        /// <exception cref="Exception"></exception>
        private static void OnAddListenerJudge(Type eventEnum, Delegate callBack)
        {
            // 先判断事件码是否存在于字典中，不存在则先添加
            // 先给字典添加事件码,委托设置为空
            eventTable.TryAdd(eventEnum, null);

            // 判断当前事件码的委托类型和要添加的委托类型是否一致，不一致不能添加，抛出异常
            var d = eventTable[eventEnum];
            if (d != null && d.GetType() != callBack.GetType())
            {
                throw new Exception($"尝试为事件码{eventEnum}添加不同事件的委托,当前事件所对应的委托是{d.GetType()},要添加的委托类型{callBack.GetType()}");
            }
        }

        /// <summary>
        /// 移除事件码前的判断
        /// </summary>
        /// <param name="eventEnum"></param>
        /// <exception cref="Exception"></exception>
        private static void OnRemoveListenerBeforeJudge(Type eventEnum)
        {
            // 判断是否包含指定事件码
            if (!eventTable.ContainsKey(eventEnum))
            {
                throw new Exception(string.Format("移除监听错误;没有事件码", eventEnum));
            }
        }

        /// <summary>
        /// 移除事件码后的判断,用于移除字典中空的事件码
        /// </summary>
        /// <param name="eventEnum"></param>
        private static void OnRemoveListenerLaterJudge(Type eventEnum)
        {
            if (eventTable[eventEnum] == null)
            {
                eventTable.Remove(eventEnum);
            }
        }

        #endregion

        #region 添加监听

        // 无参
        public static void AddListener(Type eventEnum, CallBack callBack)
        {
            OnAddListenerJudge(eventEnum, callBack);
            eventTable[eventEnum] = (CallBack)eventTable[eventEnum] + callBack;
        }

        // 带参数
        public static void AddListener<T>(Type eventEnum, CallBack<T> callBack)
        {
            OnAddListenerJudge(eventEnum, callBack);
            eventTable[eventEnum] = (CallBack<T>)eventTable[eventEnum] + callBack;
        }

        public static void AddListener<T, TX>(Type eventEnum, CallBack<T, TX> callBack)
        {
            OnAddListenerJudge(eventEnum, callBack);
            eventTable[eventEnum] = (CallBack<T, TX>)eventTable[eventEnum] + callBack;
        }

        public static void AddListener<T, TX, TY>(Type eventEnum, CallBack<T, TX, TY> callBack)
        {
            OnAddListenerJudge(eventEnum, callBack);
            eventTable[eventEnum] = (CallBack<T, TX, TY>)eventTable[eventEnum] + callBack;
        }

        public static void AddListener<T, TX, TY, TZ>(Type eventEnum, CallBack<T, TX, TY, TZ> callBack)
        {
            OnAddListenerJudge(eventEnum, callBack);
            eventTable[eventEnum] = (CallBack<T, TX, TY, TZ>)eventTable[eventEnum] + callBack;
        }

        public static void AddListener<T, TX, TY, TZ, TW>(Type eventEnum, CallBack<T, TX, TY, TZ, TW> callBack)
        {
            OnAddListenerJudge(eventEnum, callBack);
            eventTable[eventEnum] = (CallBack<T, TX, TY, TZ, TW>)eventTable[eventEnum] + callBack;
        }

        #endregion

        #region 移除监听

        public static void RemoveListener(Type eventEnum, CallBack callBack)
        {
            OnRemoveListenerBeforeJudge(eventEnum);
            eventTable[eventEnum] = (CallBack)eventTable[eventEnum] - callBack;
            OnRemoveListenerLaterJudge(eventEnum);
        }

        public static void RemoveListener<T>(Type eventEnum, CallBack<T> callBack)
        {
            OnRemoveListenerBeforeJudge(eventEnum);
            eventTable[eventEnum] = (CallBack<T>)eventTable[eventEnum] - callBack;
            OnRemoveListenerLaterJudge(eventEnum);
        }

        public static void RemoveListener<T, TX>(Type eventEnum, CallBack<T, TX> callBack)
        {
            OnRemoveListenerBeforeJudge(eventEnum);
            eventTable[eventEnum] = (CallBack<T, TX>)eventTable[eventEnum] - callBack;
            OnRemoveListenerLaterJudge(eventEnum);
        }

        public static void RemoveListener<T, TX, TY>(Type eventEnum, CallBack<T, TX, TY> callBack)
        {
            OnRemoveListenerBeforeJudge(eventEnum);
            eventTable[eventEnum] = (CallBack<T, TX, TY>)eventTable[eventEnum] - callBack;
            OnRemoveListenerLaterJudge(eventEnum);
        }

        public static void RemoveListener<T, TX, TY, TZ>(Type eventEnum, CallBack<T, TX, TY, TZ> callBack)
        {
            OnRemoveListenerBeforeJudge(eventEnum);
            eventTable[eventEnum] = (CallBack<T, TX, TY, TZ>)eventTable[eventEnum] - callBack;
            OnRemoveListenerLaterJudge(eventEnum);
        }

        public static void RemoveListener<T, TX, TY, TZ, TW>(Type eventEnum, CallBack<T, TX, TY, TZ, TW> callBack)
        {
            OnRemoveListenerBeforeJudge(eventEnum);
            eventTable[eventEnum] = (CallBack<T, TX, TY, TZ, TW>)eventTable[eventEnum] - callBack;
            OnRemoveListenerLaterJudge(eventEnum);
        }

        #endregion

        #region 广播事件

        public static void Broadcast(Type eventEnum)
        {
            if (!eventTable.TryGetValue(eventEnum, out var d)) return;
            if (d is CallBack callBack)
            {
                callBack();
            }
        }

        public static void Broadcast<T>(Type eventEnum, T arg1)
        {
            if (!eventTable.TryGetValue(eventEnum, out var d)) return;
            if (d is CallBack<T> callBack)
            {
                callBack(arg1);
            }
        }

        public static void Broadcast<T, TX>(Type eventEnum, T arg1, TX arg2)
        {
            if (!eventTable.TryGetValue(eventEnum, out var d)) return;
            if (d is CallBack<T, TX> callBack)
            {
                callBack(arg1, arg2);
            }
        }

        public static void Broadcast<T, TX, TY>(Type eventEnum, T arg1, TX arg2, TY arg3)
        {
            if (!eventTable.TryGetValue(eventEnum, out var d)) return;
            if (d is CallBack<T, TX, TY> callBack)
            {
                callBack(arg1, arg2, arg3);
            }
        }

        public static void Broadcast<T, TX, TY, TZ>(Type eventEnum, T arg1, TX arg2, TY arg3, TZ arg4)
        {
            if (!eventTable.TryGetValue(eventEnum, out var d)) return;
            if (d is CallBack<T, TX, TY, TZ> callBack)
            {
                callBack(arg1, arg2, arg3, arg4);
            }
        }

        public static void Broadcast<T, TX, TY, TZ, TW>(Type eventEnum, T arg1, TX arg2, TY arg3, TZ arg4, TW arg5)
        {
            if (!eventTable.TryGetValue(eventEnum, out var d)) return;
            var callBack = d as CallBack<T, TX, TY, TZ, TW>;
            callBack?.Invoke(arg1, arg2, arg3, arg4, arg5);
        }

        #endregion
    }
}