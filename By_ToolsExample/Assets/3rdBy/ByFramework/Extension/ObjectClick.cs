namespace _3rdBy.ByFramework.Extension
{
    using ExtendComponent;
    using UnityEngine;
    using UnityEngine.Events;

    public class ObjectClick : MonoBehaviour
    {
        public static void Register(GameObject obj, UnityAction<GameObject> onClick)
        {
            var objectClick = obj.GetOrAddComponent<ObjectClick>();
            objectClick.OnClick = onClick;
        }

        public static void UnRegister(GameObject obj, UnityAction<GameObject> onClick)
        {
            var objectClick = obj.GetComponent<ObjectClick>();
            if (objectClick != null)
            {
                GameObject.Destroy(objectClick);
            }
        }

        public UnityAction<GameObject> OnClick { get; private set; }

        private void Awake()
        {
            if (GetComponent<Collider>() == null)
            {
                Debug.LogError($"{name}没有任何碰撞体，{nameof(ObjectClick)}添加失败！");
                GameObject.Destroy(gameObject);
            }
        }

        private void OnMouseDown()
        {
            OnClick?.Invoke(gameObject);
        }

        private void OnDestroy()
        {
            OnClick = null;
        }
    }
}