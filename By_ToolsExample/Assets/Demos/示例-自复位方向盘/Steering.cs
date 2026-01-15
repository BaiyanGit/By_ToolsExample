using System.Collections;
using UnityEngine;

namespace Demos.示例_自复位方向盘
{
    public class Steering : MonoBehaviour
    {
        public Transform carCurrentTarget;
        private float _lastValue;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(3);
            SetFree();
        }

        private void SetFree()
        {
            Shanghaiziyoudu.Instance.currentTarget = carCurrentTarget;
        }

        private void Update()
        {
            var steeringValue = FreePowerSteering();
            if (Mathf.Approximately(_lastValue, steeringValue)) return;
            _lastValue = steeringValue;
            Debug.Log("SteeringValue: " + steeringValue);
        }


        /// <summary>
        /// 归一化
        /// </summary>
        /// <param name="x"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        private float Normalize(float x, float minValue, float maxValue)
        {
            if (Mathf.Approximately(x, 0)) return 0f;
            if (Mathf.Approximately(x, maxValue)) return 1f;
            if (Mathf.Approximately(x, minValue)) return -1f;

            var rangeHalf  = (maxValue - minValue) / 2; // 求出范围的一半
            var mean       = (maxValue + minValue) / 2; // 求出平均值
            var offset     = x - mean;                  // 求出偏移量
            var normalized = offset / rangeHalf;        // 求出归一化值

            normalized = x > 0 ? Mathf.Clamp(normalized, 0, 1) : Mathf.Clamp(normalized, -1, 0);

            return normalized;
        }


        /// <summary>
        /// 获取自由度方向盘
        /// </summary>
        /// <param name="module"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private float FreePowerSteering()
        {
            var value = Shanghaiziyoudu.Instance.GetCoderPosition();
            return Normalize(value, Shanghaiziyoudu.Instance.minSteering,Shanghaiziyoudu.Instance.maxSteering);
        }
    }
}