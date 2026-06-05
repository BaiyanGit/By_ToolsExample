//=====================================================
// 文件名称: MonoObjSingletonTemplate.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-05
// 描    述: 
//=====================================================
namespace _3rdBy.ByFramework.Singleton
{
    public abstract class SingletonTemplate<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object syslock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syslock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}