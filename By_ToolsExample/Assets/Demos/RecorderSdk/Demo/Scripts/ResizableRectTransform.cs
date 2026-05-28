using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Demos.RecorderSdk.Demo
{
    /// <summary>
    /// Resizes a UI panel from a drag handle and optionally scales its content as a single window.
    /// Intended for the stream demo panel: the outer panel changes size, while its inner controls scale together.
    /// </summary>
    public sealed class ResizableRectTransform : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Resize Target")]
        public RectTransform target;
        public RectTransform boundary;
        public Vector2 minSize = new Vector2(420f, 280f);
        public Vector2 maxSize = Vector2.zero;
        public bool clampToBoundary = true;

        [Header("Content Scaling")]
        [Tooltip("When enabled, children inside the target panel are wrapped under a content root and scaled with the window size.")]
        public bool scaleContent = true;

        [Tooltip("Optional content root. If empty, one is created at runtime and all target children except this resize handle are moved into it.")]
        public RectTransform contentRoot;

        [Tooltip("Use one scale value for both axes. This keeps text/buttons from being stretched.")]
        public bool uniformContentScale = true;

        [Tooltip("Children that should not be moved into the content root. The resize handle is always excluded automatically.")]
        public List<RectTransform> excludeFromContentScale = new List<RectTransform>();

        private RectTransform _self;
        private RectTransform _target;
        private RectTransform _boundary;
        private RectTransform _targetParent;
        private Canvas _rootCanvas;
        private Vector2 _startPointerLocal;
        private Vector2 _startSize;
        private Vector2 _baseTargetSize;
        private bool _contentPrepared;

        private void Awake()
        {
            ResolveReferences();
            PrepareContentRootIfNeeded();
            ClampSizeToBoundary();
            ApplyContentScale();
        }

        private void OnEnable()
        {
            ResolveReferences();
            PrepareContentRootIfNeeded();
            ClampSizeToBoundary();
            ApplyContentScale();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled) return;
            ResolveReferences();
            ApplyContentScale();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ResolveReferences();
            PrepareContentRootIfNeeded();
            if (_target == null || _targetParent == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_targetParent, eventData.position, GetEventCamera(eventData), out Vector2 pointerLocal))
            {
                _startPointerLocal = pointerLocal;
                _startSize = _target.sizeDelta;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_target == null || _targetParent == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_targetParent, eventData.position, GetEventCamera(eventData), out Vector2 pointerLocal))
            {
                return;
            }

            Vector2 delta = pointerLocal - _startPointerLocal;
            Vector2 desiredSize = _startSize + new Vector2(delta.x, -delta.y);
            _target.sizeDelta = ClampSize(desiredSize);

            if (clampToBoundary)
            {
                ClampSizeToBoundary();
            }

            ApplyContentScale();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ClampSizeToBoundary();
            ApplyContentScale();
        }

        private void ResolveReferences()
        {
            if (_self == null) _self = transform as RectTransform;
            _target = target != null ? target : transform.parent as RectTransform;
            if (_target == null) return;

            _targetParent = _target.parent as RectTransform;
            _boundary = boundary != null ? boundary : _targetParent;
            _rootCanvas = _target.GetComponentInParent<Canvas>();

            if (_baseTargetSize == Vector2.zero && _target.sizeDelta.x > 0f && _target.sizeDelta.y > 0f)
            {
                _baseTargetSize = _target.sizeDelta;
            }
        }

        private void PrepareContentRootIfNeeded()
        {
            if (!scaleContent || _target == null || _contentPrepared) return;

            if (_baseTargetSize == Vector2.zero)
            {
                _baseTargetSize = new Vector2(
                    Mathf.Max(minSize.x, _target.rect.width > 0f ? _target.rect.width : _target.sizeDelta.x),
                    Mathf.Max(minSize.y, _target.rect.height > 0f ? _target.rect.height : _target.sizeDelta.y));
            }

            if (contentRoot == null)
            {
                Transform existing = _target.Find("ResizableContentRoot");
                if (existing != null)
                {
                    contentRoot = existing as RectTransform;
                }
            }

            if (contentRoot == null)
            {
                GameObject rootObject = new GameObject("ResizableContentRoot", typeof(RectTransform));
                contentRoot = rootObject.GetComponent<RectTransform>();
                contentRoot.SetParent(_target, false);
                contentRoot.SetAsFirstSibling();
                ConfigureContentRootRect(contentRoot);

                List<Transform> childrenToMove = new List<Transform>();
                for (int i = 0; i < _target.childCount; i++)
                {
                    Transform child = _target.GetChild(i);
                    if (child == contentRoot || child == transform || child == _self) continue;
                    if (IsExcludedFromContentScale(child as RectTransform)) continue;
                    childrenToMove.Add(child);
                }

                for (int i = 0; i < childrenToMove.Count; i++)
                {
                    childrenToMove[i].SetParent(contentRoot, false);
                }
            }
            else
            {
                ConfigureContentRootRect(contentRoot);
            }

            if (_self != null)
            {
                _self.SetAsLastSibling();
            }

            _contentPrepared = true;
        }

        private void ConfigureContentRootRect(RectTransform root)
        {
            if (root == null) return;
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = _baseTargetSize;
            root.localRotation = Quaternion.identity;
            if (root.localScale == Vector3.zero) root.localScale = Vector3.one;
        }

        private bool IsExcludedFromContentScale(RectTransform rect)
        {
            if (rect == null) return false;
            if (rect == _self) return true;
            for (int i = 0; i < excludeFromContentScale.Count; i++)
            {
                if (excludeFromContentScale[i] == rect) return true;
            }
            return false;
        }

        private Vector2 ClampSize(Vector2 desiredSize)
        {
            desiredSize.x = Mathf.Max(minSize.x, desiredSize.x);
            desiredSize.y = Mathf.Max(minSize.y, desiredSize.y);

            if (maxSize.x > 0f) desiredSize.x = Mathf.Min(maxSize.x, desiredSize.x);
            if (maxSize.y > 0f) desiredSize.y = Mathf.Min(maxSize.y, desiredSize.y);

            return desiredSize;
        }

        private void ClampSizeToBoundary()
        {
            if (_target == null || _targetParent == null || _boundary == null || !clampToBoundary) return;

            _target.sizeDelta = ClampSize(_target.sizeDelta);

            Rect targetRect = GetRectInParentSpace(_target, _targetParent);
            Rect boundaryRect = GetRectInParentSpace(_boundary, _targetParent);
            if (boundaryRect.width <= 0f || boundaryRect.height <= 0f)
            {
                boundaryRect = GetScreenFallbackRectInTargetParent();
            }

            if (boundaryRect.width <= 0f || boundaryRect.height <= 0f) return;

            Vector2 size = _target.sizeDelta;
            if (targetRect.xMax > boundaryRect.xMax)
            {
                size.x -= targetRect.xMax - boundaryRect.xMax;
            }

            if (targetRect.yMin < boundaryRect.yMin)
            {
                size.y -= boundaryRect.yMin - targetRect.yMin;
            }

            if (maxSize.x <= 0f) size.x = Mathf.Min(size.x, boundaryRect.width);
            if (maxSize.y <= 0f) size.y = Mathf.Min(size.y, boundaryRect.height);

            _target.sizeDelta = ClampSize(size);
        }

        private void ApplyContentScale()
        {
            if (!scaleContent || contentRoot == null || _target == null) return;
            if (_baseTargetSize.x <= 0f || _baseTargetSize.y <= 0f) return;

            float scaleX = Mathf.Max(0.01f, _target.sizeDelta.x / _baseTargetSize.x);
            float scaleY = Mathf.Max(0.01f, _target.sizeDelta.y / _baseTargetSize.y);

            if (uniformContentScale)
            {
                float scale = Mathf.Min(scaleX, scaleY);
                contentRoot.localScale = new Vector3(scale, scale, 1f);
            }
            else
            {
                contentRoot.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            contentRoot.anchoredPosition = Vector2.zero;
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
                Vector2 localPoint = parent.InverseTransformPoint(worldCorners[i]);
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
