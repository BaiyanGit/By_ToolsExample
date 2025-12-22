using UnityEngine;

namespace 齿轮测试
{
    using System;

    /// <summary>
    /// 齿轮转动
    /// </summary>
    public class GearRotation : MonoBehaviour
    {
        [Header("齿轮")] public new Rigidbody rigidbody;
        [Header("方向")] public bool isAnticlockwise;
        [Header("速度"), Range(0, 100)] public int speed = 2;

        private int _direction;

        private void Awake()
        {
        }

        private void Update()
        {
            _direction = isAnticlockwise ? -1 : 1;

            rigidbody.angularVelocity = Vector3.right * (_direction * (speed * 100 * Time.deltaTime));
        }
    }
}