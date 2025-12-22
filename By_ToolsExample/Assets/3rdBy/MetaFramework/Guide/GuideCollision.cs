using System;
using UnityEngine;
using UnityEngine.Events;

namespace ZCustom
{
    public class GuideCollision : MonoBehaviour
    {
        public UnityAction<Collision> CollisionEnter { get; set; }
        public UnityAction<Collision, float> CollisionStay { get; set; }
        public UnityAction<Collision> CollisionExit { get; set; }

        private float _tempTime;

        private void OnCollisionEnter(Collision other)
        {
            CollisionEnter?.Invoke(other);
        }

        private void OnCollisionStay(Collision other)
        {
            _tempTime += Time.deltaTime;
            CollisionStay?.Invoke(other, _tempTime);
        }

        private void OnCollisionExit(Collision other)
        {
            _tempTime = 0;
            CollisionExit?.Invoke(other);
        }
    }
}