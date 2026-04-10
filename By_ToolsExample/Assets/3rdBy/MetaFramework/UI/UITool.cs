namespace _3rdBy.MetaFramework.UI
{
    using Cysharp.Threading.Tasks; 
    using UnityEngine;

    /// <summary>
    /// UI帮助类
    /// </summary>
    public static class UITool
    {
        public static T Get<T>() where T : class, IUIBase
        {
            string uiName = typeof(T).Name;

            var ui = UIManager.Instance.Get(uiName);

            return ui as T;
        }

        public static async UniTask<T> OpenStackAsync<T>(params object[] args) where T : class, IUIBase
        {
            string uiName = typeof(T).Name;

            var ui = await UIManager.Instance.OpenStackAsync(uiName, args);

            return ui as T;
        }

        public static T OpenStack<T>(params object[] args) where T : class, IUIBase
        {
            string uiName = typeof(T).Name;

            var ui = UIManager.Instance.OpenStack(uiName, args);

            return ui as T;
        }

        public static void CloseStack(bool isDestroy = false)
        {
            UIManager.Instance.CloseStack(isDestroy);
        }

        public static async UniTask<T> OpenAsync<T>(params object[] args) where T : class, IUIBase
        {
            string uiName = typeof(T).Name;

            var ui = await UIManager.Instance.OpenNormalAsync(uiName, args);

            return ui as T;
        }

        public static T Open<T>(params object[] args) where T : class, IUIBase
        {
            string uiName = typeof(T).Name;

            var ui = UIManager.Instance.OpenNormal(uiName, args);

            return ui as T;
        }

        public static void Close<T>(bool isDestroy = false) where T : IUIBase
        {
            string uiName = typeof(T).Name;

            UIManager.Instance.CloseNormal(uiName, isDestroy);
        }

        public static void CloseAll(bool isDestroy = false)
        {
            UIManager.Instance.CloseAll(isDestroy);
        }

        /// <summary>
        /// 关闭所有排除部分，栈UI会全部关闭
        /// </summary>
        /// <param name="names">排除</param>
        public static void CloseAllWithout(bool isDestroy, params string[] names)
        {
            UIManager.Instance.CloseAllWithout(isDestroy, names);
        }

        /// <summary>
        /// UI坐标系中，世界坐标转UI坐标
        /// </summary>
        /// <param name="worldPos">世界坐标，transform.position</param>
        /// <param name="uiLayer">临时捕获的rectTransform载体，默认即可</param>
        /// <returns></returns>
        public static Vector2 World2UIPosition(Vector3 worldPos, UILayer uiLayer = UILayer.Center)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(UIRoot.Instance.uiCamera, worldPos);
            return Screen2UIPosition(screenPos, uiLayer);
        }

        /// <summary>
        /// UI坐标系中，屏幕坐标转UI坐标
        /// </summary>
        /// <param name="screenPos">屏幕坐标，例如1920-1080...</param>
        /// <param name="uiLayer">临时捕获的rectTransform载体，默认即可</param>
        /// <returns></returns>
        public static Vector2 Screen2UIPosition(Vector2 screenPos, UILayer uiLayer = UILayer.Center)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UIRoot.Instance.GetLayerTransform(uiLayer),
                screenPos, UIRoot.Instance.uiCamera, out Vector2 localPos);
            return localPos;
        }
    }
}