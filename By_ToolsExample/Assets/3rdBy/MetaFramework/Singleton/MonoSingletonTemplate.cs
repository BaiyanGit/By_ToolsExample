namespace _3rdBy.MetaFramework.Singleton
{
    using UnityEngine;

    public abstract class MonoSingletonTemplate<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }


        protected virtual void Awake()
        {
            Instance = this as T;
            DontDestroyOnLoad(this);
        }
        
        protected virtual void Start()
        {
            
        }

        protected virtual void OnDestroy()
        {
            Instance = null;
        }
    }
}