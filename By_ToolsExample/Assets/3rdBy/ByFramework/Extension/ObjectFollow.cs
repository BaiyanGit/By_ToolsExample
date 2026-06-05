namespace _3rdBy.ByFramework.Extension
{
    using UnityEngine;

    //
    public class ObjectFollow : MonoBehaviour
    {
        public static void Register(GameObject obj, Camera camera, float planeZ = 0.9f)
        {
            var objectFollow = obj.GetComponent<ObjectFollow>();
            if (objectFollow == null)
            {
                objectFollow = obj.AddComponent<ObjectFollow>();
                objectFollow._camera = camera;
                objectFollow._planeZ = planeZ;
            }
        }

        public static void UnRegister(GameObject obj)
        {
            var objectFollow = obj.GetComponent<ObjectFollow>();
            if (objectFollow != null)
            {
                GameObject.Destroy(objectFollow);
            }
        }

        private Camera _camera;
        private float _planeZ;

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }

            transform.position = PositionConverter.ScreenPointToWorldPoint(_camera, Input.mousePosition, _planeZ);
        }
    }
}