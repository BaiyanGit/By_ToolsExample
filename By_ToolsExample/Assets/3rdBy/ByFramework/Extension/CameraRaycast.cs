namespace _3rdBy.ByFramework.Extension
{
    using ExtendComponent;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    /// <summary>
    /// 触发器无法使用
    /// </summary>
    public class CameraRaycast : MonoBehaviour
    {
        public static CameraRaycast Register(Camera camera, float distance = 100, int layerMask = 0)
        {
            var cameraRaycast = camera.GetOrAddComponent<CameraRaycast>();
            cameraRaycast._camera = camera;
            cameraRaycast._distance = distance;
            cameraRaycast._layerMask = layerMask;
            return cameraRaycast;
        }

        public static void UnRegister(Camera camera)
        {
            var cameraRaycast = camera.GetComponent<CameraRaycast>();
            if (cameraRaycast != null)
            {
                GameObject.Destroy(cameraRaycast);
            }
        }

        public UnityAction<RaycastHit> OnClick { get; set; }
        public UnityAction<RaycastHit> OnHit { get; set; }

        private float _distance;
        private int _layerMask;
        private Camera _camera;

        private Ray _ray;
        private RaycastHit _hit;

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }
            
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            _ray = _camera.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawLine(_ray.origin, _ray.origin + _ray.direction * _distance);
            if (Physics.Raycast(_ray, out _hit, _distance, 1 << _layerMask))
            {
                OnHit?.Invoke(_hit);
                if (Input.GetMouseButtonDown(0))
                {
                    OnClick?.Invoke(_hit);
                }
            }
        }

        private void OnDestroy()
        {
            OnClick = null;
            OnHit = null;
        }
    }
}