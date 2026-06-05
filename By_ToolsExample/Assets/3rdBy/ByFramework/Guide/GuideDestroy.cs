namespace _3rdBy.ByFramework.Guide
{
    using UnityEngine;
    using UnityEngine.Events;

    public class GuideDestroy : MonoBehaviour
    {
        public UnityAction Destroy;
        
        private void OnDestroy()
        {
            Destroy?.Invoke();
        }
    }
}