using System;

namespace ZCustom
{
    [Serializable]
    public class RequestDataStruct
    {
    }

    [Serializable]
    public class ResponseDataStruct
    {
        public int code;
        public string msg;
    }


    // [Serializable]
    // public class ResponseDataStruct
    // {
    //     public bool success;
    //     public int code;
    //     public string message;
    // }
}