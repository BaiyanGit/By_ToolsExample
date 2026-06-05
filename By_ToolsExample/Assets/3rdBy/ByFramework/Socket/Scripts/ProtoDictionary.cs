namespace _3rdBy.ByFramework.Socket.Scripts
{
    using System;
    using System.Collections.Generic;
    using Google.Protobuf;
    using Msg;
    using Proto;

    /// <summary>
    /// 消息协议定义
    /// </summary>
    public static class ProtoDictionary
    {
        #region 协议定义

        private static readonly List<int> protoIds = new(); // 协议号
        private static readonly List<Type> protoType = new(); // 协议类型
        private static readonly Dictionary<RuntimeTypeHandle, MessageParser> protoParsers = new(); // 解析Proto文件

        static ProtoDictionary()
        {
            protoIds.Add(0x0001);
            protoType.Add(typeof(LoginRequest));
            protoParsers.Add(typeof(LoginRequest).TypeHandle, LoginRequest.Parser);

            protoIds.Add(0x0002);
            protoType.Add(typeof(LoginResponse));
            protoParsers.Add(typeof(LoginResponse).TypeHandle, LoginResponse.Parser);

            protoIds.Add(0x0003); //心跳
            protoType.Add(typeof(HeartBeat));
            protoParsers.Add(typeof(HeartBeat).TypeHandle, HeartBeat.Parser);
        }

        #endregion

        #region 功能函数

        /// <summary>
        /// 协议类型列表中是否包含协议的类型
        /// </summary>
        /// <param name="type">协议类型</param>
        /// <returns>true:包含 false:不包含</returns>
        public static bool IsContainProtoType(Type type)
        {
            return protoType.Contains(type);
        }

        /// <summary>
        /// 协议号列表中是否包含协议号
        /// </summary>
        /// <param name="protoId">协议号</param>
        /// <returns>true:包含 false:不包含</returns>
        public static bool IsContainProtoId(int protoId)
        {
            return protoIds.Contains(protoId);
        }

        /// <summary>
        /// 根据协议号查找对应的协议类型
        /// </summary>
        /// <param name="protoId">协议号</param>
        /// <returns>返回消息体类型</returns>
        public static Type GetProtoTypeByProtoId(int protoId)
        {
            var index = protoIds.IndexOf(protoId);
            return protoType[index];
        }

        /// <summary>
        /// 根据协议类型查找对应的ProtoId
        /// </summary>
        /// <param name="type">协议类型</param>
        /// <returns>返回消息号</returns>
        public static int GetProtoIdByProtoType(Type type)
        {
            var index = protoType.IndexOf(type);
            return protoIds[index];
        }

        /// <summary>
        /// 根据句柄类型找到对应解析Proto消息的类型
        /// </summary>
        /// <param name="typeHandle"></param>
        /// <returns></returns>
        public static MessageParser GetMessageParser(RuntimeTypeHandle typeHandle)
        {
            protoParsers.TryGetValue(typeHandle, out var messageParser);
            return messageParser;
        }

        #endregion
    }
}