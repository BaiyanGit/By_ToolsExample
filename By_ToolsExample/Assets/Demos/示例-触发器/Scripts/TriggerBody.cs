using UnityEngine;

namespace 触发器
{
    public class TriggerBody : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("OnTriggerEnter：" + other.name);
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log("OnTriggerExit：" + other.name);
        }
    }
}