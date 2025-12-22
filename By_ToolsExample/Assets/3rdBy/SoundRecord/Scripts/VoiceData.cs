namespace Program.HttpData
{
    using System;
    using System.Collections.Generic;
    using MetaFramework.Helper;
    using ZCustom;

    /// <summary>
    /// 语音 请求数据（加入房间、获取聊天消息）
    /// </summary>
    [Serializable]
    public class VoiceRequestData : RequestDataStruct
    {
        public int userid;
        public string username;
    }

    /// <summary>
    /// 语音 请求返回（加入房间、获取聊天消息）
    /// </summary>
    [Serializable]
    public class VoiceResponseData : ResponseDataStruct
    {
    }

    /// <summary>
    /// 语音 上传的聊天消息
    /// </summary>
    [Serializable]
    public class VoiceUploadChatRequestData : RequestDataStruct
    {
        public int type; //0:文本 1:语音
        public string username; //用户名
        public int userid; //用户Id
        public string data; //内容
    }

    /// <summary>
    /// 语音 返回聊天消息
    /// </summary>
    [Serializable]
    public class VoiceUploadChatResponseData : ResponseDataStruct
    {
        public VoiceChatData infos;
    }

    /// <summary>
    /// 返回的聊天内容
    /// </summary>
    [Serializable]
    public class VoiceChatData
    {
        public int type; //0:文本 1:语音
        public int userid; //用户Id
        public string username; //用户名
        public string data; //内容
        public string date; //时间
    }

}