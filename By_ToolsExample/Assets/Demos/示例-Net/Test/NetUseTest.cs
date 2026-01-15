using UnityEngine;

namespace Net.Test
{
    using Msg;
    using Proto;
    using Scripts;
    using UnityEngine.UI;

    public class NetUseTest : MonoBehaviour
    {
        public Button btnSendLogin;
        public Button btnConnected;

        private void Awake()
        {
            InitEvent();
        }

        private void InitEvent()
        {
            btnSendLogin.onClick.AddListener(SuSendMsg);
            btnConnected.onClick.AddListener(() =>
            {
                ClientManager.Instance.SendConnect();
            });
            NetEventHandler.Instance.AddListenHandler(typeof(LoginResponse), ReceiveLogin);
        }

        private void Start()
        {
            ClientManager.Instance.SendConnect();
        }

        /// <summary>
        /// 测试接收服务发送的登录消息
        /// </summary>
        /// <param name="data"></param>
        private static void ReceiveLogin(object data)
        {
            if (data is LoginResponse loginData)
                Debug.Log($"msg：{loginData.Message} Result:{loginData.Result}");//
        }

        /// <summary>
        /// 测试向服务发送的登录消息
        /// </summary>
        private static void SuSendMsg()
        {
            var loginRequest = new LoginRequest
            {
                Username = "张三",
                Password = "123456"
            };
            ClientManager.Instance.SendMessage(loginRequest);
        }
    }
}