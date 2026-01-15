namespace Network.Server
{
    using UnityEngine;

    public class HeadDefine
    {
        public const int Login = 0x10001; //登录
        public const int ObjectInfo = 0x10002; //物体信息
    }

    public class StartServer : MonoBehaviour
    {
        public string msg = "Hello World!";
        public new GameObject gameObject;
        private ObjectTestData _objectTestData = new();

        private void Start()
        {
            SocketServer.Instance.StartServer();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                // 构造登录数据
                var loginData = new LoginData
                {
                    userName = "admin" + Count,
                    password = "123456",
                    msg = msg,
                };
                SendMsgToClient(loginData);
            }

            if (Input.GetKey(KeyCode.S))
            {
                SendMsgToClientObject();
            }
        }

        private void SendMsgToClientObject()
        {
            // _objectTestData.msgHeader = HeadDefine.ObjectInfo;
            // _objectTestData.objectName = gameObject.name;
            // _objectTestData.position = gameObject.transform.position;
            // _objectTestData.rotation = gameObject.transform.rotation.eulerAngles;
            //
            // var bodyBytes = ClientSession.Serializer(_objectTestData);
            // foreach (var clientSession in ClientSessionManager.ClientSessions.Values)
            // {
            //     clientSession.SendMsgToClient(bodyBytes);
            // }
        }

        private const int Count = 0;

        private void SendMsgToClient(LoginData loginData)
        {
            // 序列化登录数据
            var bodyBytes = ClientSession.Serializer(loginData);

            var msgBase = new MsgBase
            {
                msgHeader = HeadDefine.Login,
                msgLength = bodyBytes.Length + 8
            };

            var headBytes = ClientSession.Serializer(msgBase);
            
            // 合并消息头和消息体
            bodyBytes = ClientSession.CombineBytes(headBytes, bodyBytes);


            Debug.Log($"Send Msg To Client：{Count}");

            foreach (var clientSession in ClientSessionManager.ClientSessions.Values)
            {
                clientSession.SendMsgToClient(bodyBytes);
            }
        }

        // 监听程序退出
        private void OnApplicationQuit()
        {
            ClientSessionManager.ClientSessions.RemoveAllClientSession();
        }
    }
}