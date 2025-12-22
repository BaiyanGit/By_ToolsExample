using System;
using UnityEngine;
using UnityEngine.Events;

namespace ZCustom
{
    public class GuideDestroy : MonoBehaviour
    {
        public UnityAction Destroy;
        
        private void OnDestroy()
        {
            Destroy?.Invoke();
        }
    }
}