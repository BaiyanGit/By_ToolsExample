namespace _3rdBy.MetaFramework.Guide
{
    using UnityEngine;

    /// <summary>
    /// UI事件渗透
    /// </summary>
    public class GuideUIPenetrate : MonoBehaviour, ICanvasRaycastFilter
    {
        private RectTransform _target;

        public void SetTargetImage(RectTransform target)
        {
            _target = target;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (_target == null)
                return true;

            return !RectTransformUtility.RectangleContainsScreenPoint(_target, sp, eventCamera);
        }
    }
}
