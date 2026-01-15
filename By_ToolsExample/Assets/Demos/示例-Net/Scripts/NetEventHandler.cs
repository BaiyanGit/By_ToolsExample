namespace Net.Scripts
{
    using System;
    using System.Collections.Generic;
    using _3rdBy.MetaFramework.Singleton;
    using UnityEngine;

    /// <summary>
    /// 用于处理网络消息
    /// </summary>
    public delegate void MsgHandler(object data);

    /// <summary>
    /// 网络监听事件
    /// </summary>
    public class NetEventHandler : SingletonTemplate<NetEventHandler>
    {
        private Dictionary<Type, MsgHandler> _netEventPool = new();

        /// <summary>
        /// 添加网络消息监听处理事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msgHandler"></param>
        public void AddListenHandler(Type type, MsgHandler msgHandler)
        {
            if (_netEventPool.ContainsKey(type))
            {
                Debug.Log("Event : " + type);
                _netEventPool[type] += msgHandler;
            }
            else
                _netEventPool.Add(type, msgHandler);
        }

        /// <summary>
        /// 移出监听处理事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msgHandler"></param>
        public void RemoveListenHandler(Type type, MsgHandler msgHandler)
        {
            if (!_netEventPool.ContainsKey(type))
            {
                Debug.LogError($"当前没有监听事件{type}");
                return;
            }

            _netEventPool[type] -= msgHandler;
            _netEventPool.Remove(type);
        }

        /// <summary>
        /// 广播事件
        /// </summary>
        public void BroadcastHandler(Type type, object msgHandler)
        {
            if (!_netEventPool.ContainsKey(type))
            {
                Debug.LogError($"未监听此事件{type}");
                return;
            }

            _netEventPool[type]?.Invoke(msgHandler);
        }
    }
}