namespace _3rdBy.ByFramework.Singleton
{
    using UnityEngine;

    public abstract class MonoObjSingletonTemplate<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<T>();
                if (_instance != null)
                {
                    DontDestroyOnLoad(_instance.gameObject);
                    return _instance;
                }

                var container = new GameObject($"[Single_{typeof(T).Name}]");
                _instance = container.AddComponent<T>();
                DontDestroyOnLoad(container);
                return _instance;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
