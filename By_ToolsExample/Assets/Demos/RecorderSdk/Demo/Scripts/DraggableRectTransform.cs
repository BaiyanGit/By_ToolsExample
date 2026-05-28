using UnityEngine;
using UnityEngine.EventSystems;

namespace Demos.RecorderSdk.Demo
{
    public sealed class DraggableRectTransform : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform target;
        public RectTransform boundary;
        public bool clampToParent = true;

        private RectTransform _self;
        private RectTransform _target;
        private RectTransform _boundary;
        private RectTransform _targetParent;
        private Canvas _rootCanvas;
        private Vector2 _pointerOffset;

        private void Awake()
        {
            ResolveReferences();
            ClampTarget();
        }

        private void OnEnable()
        {
            ResolveReferences();
            ClampTarget();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ResolveReferences();
            if (_target == null || _targetParent == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_targetParent, eventData.position, GetEventCamera(eventData), out Vector2 pointerLocal))
            {
                _pointerOffset = _target.anchoredPosition - pointerLocal;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_target == null || _targetParent == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_targetParent, eventData.position, GetEventCamera(eventData), out Vector2 pointerLocal))
            {
                return;
            }

            _target.anchoredPosition = pointerLocal + _pointerOffset;
            ClampTargetToBoundary();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ClampTarget();
        }

        private void ResolveReferences()
        {
            if (_self == null) _self = transform as RectTransform;
            _target = target != null ? target : _self;
            if (_target == null) return;

            _targetParent = _target.parent as RectTransform;
            _boundary = boundary != null ? boundary : (clampToParent ? _targetParent : null);
            _rootCanvas = _target.GetComponentInParent<Canvas>();
        }

        private void ClampTarget()
        {
            if (_target == null) return;
            ClampTargetToBoundary();
        }

        private void ClampTargetToBoundary()
        {
            if (_target == null || _targetParent == null || _boundary == null) return;

            Rect targetRect = GetRectInParentSpace(_target, _targetParent);
            Rect boundaryRect = GetRectInParentSpace(_boundary, _targetParent);
            if (boundaryRect.width <= 0f || boundaryRect.height <= 0f)
            {
                boundaryRect = GetScreenFallbackRectInTargetParent();
            }

            if (boundaryRect.width <= 0f || boundaryRect.height <= 0f) return;

            Vector2 correction = Vector2.zero;
            if (targetRect.width > boundaryRect.width)
            {
                correction.x = boundaryRect.center.x - targetRect.center.x;
            }
            else if (targetRect.xMin < boundaryRect.xMin)
            {
                correction.x = boundaryRect.xMin - targetRect.xMin;
            }
            else if (targetRect.xMax > boundaryRect.xMax)
            {
                correction.x = boundaryRect.xMax - targetRect.xMax;
            }

            if (targetRect.height > boundaryRect.height)
            {
                correction.y = boundaryRect.center.y - targetRect.center.y;
            }
            else if (targetRect.yMin < boundaryRect.yMin)
            {
                correction.y = boundaryRect.yMin - targetRect.yMin;
            }
            else if (targetRect.yMax > boundaryRect.yMax)
            {
                correction.y = boundaryRect.yMax - targetRect.yMax;
            }

            if (correction.sqrMagnitude > 0f)
            {
                _target.anchoredPosition += correction;
            }
        }

        private Rect GetRectInParentSpace(RectTransform rect, RectTransform parent)
        {
            if (rect == null || parent == null) return Rect.zero;

            Vector3[] worldCorners = new Vector3[4];
            rect.GetWorldCorners(worldCorners);

            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < worldCorners.Length; i++)
            {
                Vector2 localPoint = _targetParent.InverseTransformPoint(worldCorners[i]);
                min = Vector2.Min(min, localPoint);
                max = Vector2.Max(max, localPoint);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private Rect GetScreenFallbackRectInTargetParent()
        {
            RectTransform canvasRect = _rootCanvas != null ? _rootCanvas.transform as RectTransform : null;
            if (canvasRect == null || _targetParent != canvasRect) return Rect.zero;

            float scaleFactor = _rootCanvas.scaleFactor > 0f ? _rootCanvas.scaleFactor : 1f;
            Vector2 size = new Vector2(Screen.width / scaleFactor, Screen.height / scaleFactor);
            Vector2 pivot = canvasRect.pivot;
            return Rect.MinMaxRect(
                -pivot.x * size.x,
                -pivot.y * size.y,
                (1f - pivot.x) * size.x,
                (1f - pivot.y) * size.y);
        }

        private Camera GetEventCamera(PointerEventData eventData)
        {
            if (eventData != null && eventData.pressEventCamera != null) return eventData.pressEventCamera;
            if (_rootCanvas == null || _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
            return _rootCanvas.worldCamera;
        }
    }
}
