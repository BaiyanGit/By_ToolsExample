namespace Network.Server
{
    using System;
    using UnityEngine;

    /// <summary>
    /// 消息体基类
    /// </summary>
    [Serializable]
    public class MsgBase
    {
        /// <summary>
        /// 消息头
        /// </summary>
        public int msgHeader;

        /// <summary>
        /// 消息长度
        /// </summary>
        public int msgLength;
    }

    /// <summary>
    /// 登录数据
    /// </summary>
    [Serializable]
    public class LoginData
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName;

        /// <summary>
        /// 用户密码
        /// </summary>
        public string password;

        /// <summary>
        /// 消息
        /// </summary>
        public string msg;
    }

    [Serializable]
    public class ObjectTestData
    {
        /// <summary>
        /// 物体名称
        /// </summary>
        public string objectName;

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// 旋转
        /// </summary>
        public Vector3 rotation;

        /// <summary>
        /// 消息
        /// </summary>
        public string msg;
    }
}