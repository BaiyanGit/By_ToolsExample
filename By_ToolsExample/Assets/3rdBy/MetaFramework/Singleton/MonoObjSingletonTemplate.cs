namespace _3rdBy.MetaFramework.Singleton
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

                _instance = new GameObject().AddComponent<T>();
                _instance.name = $"[Single_{typeof(T).Name}]";
                DontDestroyOnLoad(_instance);
                return _instance;
            }
        }

        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }
}