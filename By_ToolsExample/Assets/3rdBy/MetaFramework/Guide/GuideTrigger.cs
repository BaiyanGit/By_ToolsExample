using UnityEngine;
using UnityEngine.Events;

namespace ZCustom
{
    /// <summary>
    /// 引导触发检测
    /// </summary>
    public class GuideTrigger : MonoBehaviour
    {
        public UnityAction<Collider> TriggerEnter { get; set; }
        public UnityAction<Collider, float, int> TriggerStay { get; set; }
        public UnityAction<Collider> TriggerExit { get; set; }

        private float _tempTime;
        private int _frame;

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TriggerStay?.Invoke(other, _tempTime, _frame);
            _tempTime += Time.deltaTime;
            _frame++;
        }

        private void OnTriggerExit(Collider other)
        {
            _tempTime = 0;
            _frame = 0;
            TriggerExit?.Invoke(other);
        }
    }
}