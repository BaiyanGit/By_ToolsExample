namespace _3rdBy.ByFunc.Extension
{
    using UnityEngine;

    public class CameraController : MonoBehaviour
    {
        public Transform target;
        public float distanceMin = 2.0f;
        public float distanceMax = 15.0f;
        public float sensitivity = 10.0f;
        public float zoomSpeed = 1.0f;
        public float minY = -20.0f;
        public float maxY = 80.0f;
        public bool needDamping = false;
        public float damping = 3f;
        public float rotDamping = 10f;

        private Vector3 offset;
        private float currentDistance = 10.0f;
        private float mouseX = 0.0f;
        private float mouseY = 0.0f;

        void OnEnable()
        {
            offset = transform.position - target.position;
            currentDistance = offset.magnitude;
            mouseX = transform.eulerAngles.y;
            mouseY = transform.eulerAngles.x;
        }

        void LateUpdate()
        {
            // 滚轮调节相机远近
            currentDistance -= Input.GetAxis("Mouse ScrollWheel") * sensitivity * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, distanceMin, distanceMax);

            // 右键移动相机视角
            if (Input.GetMouseButton(1))
            {
                mouseX += Input.GetAxis("Mouse X") * sensitivity;
                mouseY -= Input.GetAxis("Mouse Y") * sensitivity;
                mouseY = Mathf.Clamp(mouseY, minY, maxY);
            }

            var rotation = Quaternion.Euler(mouseY, mouseX, 0);
            var position = rotation * new Vector3(0, 0, -currentDistance) + target.position;

            if (needDamping)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * damping);
                transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * rotDamping);
            }
            else
            {
                transform.rotation = rotation;
                transform.position = position;
            }
        }
    }
}