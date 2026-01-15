namespace Demos.示例_触发器.Scripts
{
    using UnityEngine;

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