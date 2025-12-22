using UnityEngine;

namespace Network.Client
{
    public class StartClient : MonoBehaviour
    {
        private void Start()
        {
            ClientManager.Instance.ContentToServer();
        }
    }
}