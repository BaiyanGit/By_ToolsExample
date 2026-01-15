namespace _3rdBy.MetaFramework.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using _3rdBy.MetaFramework.Singleton;
    using Cysharp.Threading.Tasks;
    using Interface;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// UI
    /// Prefab??     UIXXX
    /// Control??    UIXXX
    /// Model??      UIModelXXX
    /// View??       UIViewXXX
    /// </summary>
    public class UIManager : SingletonTemplate<UIManager>
    {
        public const string UI_RES_PATH = "Prefab/UI/{0}";

        /// <summary>
        /// UIRoot
        /// </summary>
        private UIRoot uiRootGo;

        /// <summary>
        /// UI缓存
        /// </summary>
        private Dictionary<string, IUIBase> uiCacheDic;

        /// <summary>
        /// UI栈
        /// </summary>
        private Stack<IUIBase> uiStack;

        /// <summary>
        /// UI列表
        /// </summary>
        private List<IUIBase> uiList;

        public UIManager()
        {
            uiCacheDic = new Dictionary<string, IUIBase>();
            uiStack = new Stack<IUIBase>();
            uiList = new List<IUIBase>();

            uiRootGo = UIRoot.Instance; //CreateUIRoot();
            uiRootGo.UpdateAction = Update;
        }

        private void Update()
        {
            foreach (var uiBase in uiStack.Where(uiBase => uiBase.IsShowing))
            {
                uiBase.OnUpdate();
            }

            foreach (var uiBase in uiList.Where(uiBase => uiBase.IsShowing))
            {
                uiBase.OnUpdate();
            }
        }

        public IUIBase Get(string uiName)
        {
            IUIBase ui = null;
            if (!uiCacheDic.TryGetValue(uiName, out ui))
            {
                //Debug.LogError("can not find the ui from cache:" + uiName);
                return null;
            }

            return ui;
        }

        /// <summary>
        /// 异步打开普通UI
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public async UniTask<IUIBase> OpenNormalAsync(string uiName, params object[] args)
        {
            if (!uiCacheDic.TryGetValue(uiName, out var ui))
            {
                ui = await LoadUIAsync(uiName);
            }

            if (!uiList.Contains(ui))
            {
                uiList.Add(ui);
            }

            ui.UIType = UIType.List;

            return ShowUI(ui, args);
        }

        /// <summary>
        /// 同步打开普通UI
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IUIBase OpenNormal(string uiName, params object[] args)
        {
            if (!uiCacheDic.TryGetValue(uiName, out var ui))
            {
                ui = LoadUI(uiName);
            }

            if (!uiList.Contains(ui))
            {
                uiList.Add(ui);
            }

            ui.UIType = UIType.List;

            return ShowUI(ui, args);
        }

        /// <summary>
        /// 关闭普通UI
        /// </summary>
        /// <param name="uiName"></param>
        public void CloseNormal(string uiName, bool isDestroy = false)
        {
            if (uiList.Count <= 0) return;

            IUIBase ui = uiList.Find(ui => ui.UIName.Equals(uiName));

            if (ui == null)
            {
                Debug.LogWarning("can not find the ui:" + uiName);
                return;
            }

            uiList.Remove(ui);

            HideUI(ui, isDestroy);
        }

        /// <summary>
        /// 异步打开栈UI
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="onComplete"></param>
        public async UniTask<IUIBase> OpenStackAsync(string uiName, params object[] args)
        {
            if (uiStack.Count > 0)
            {
                IUIBase topUI = uiStack.Peek();
                topUI.OnPause();
            }

            if (!uiCacheDic.TryGetValue(uiName, out var ui))
            {
                ui = await LoadUIAsync(uiName);
            }

            uiStack.Push(ui);

            ui.UIType = UIType.Stack;

            return ShowUI(ui, args);
        }

        /// <summary>
        /// 同步打开栈UI
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IUIBase OpenStack(string uiName, params object[] args)
        {
            if (uiStack.Count > 0)
            {
                IUIBase topUI = uiStack.Peek();
                topUI.OnPause();
            }

            if (!uiCacheDic.TryGetValue(uiName, out var ui))
            {
                ui = LoadUI(uiName);
            }

            uiStack.Push(ui);

            ui.UIType = UIType.Stack;

            return ShowUI(ui, args);
        }

        /// <summary>
        /// 关闭栈UI
        /// </summary>
        public void CloseStack(bool isDestroy = false)
        {
            if (uiStack.Count <= 0) return;

            IUIBase ui = uiStack.Pop();

            HideUI(ui, isDestroy);

            if (uiStack.Count > 0)
            {
                IUIBase topUI = uiStack.Peek();
                topUI.OnResume();
            }
        }

        private IUIBase ShowUI(IUIBase ui, params object[] args)
        {
            if (ui.IsShowing)
            {
                ui.UIHierarchy.PlayRefresh();
            }
            else
            {
                ui.IsShowing = true;
                ui.UIHierarchy.PlayIn();
            }
            //ui.uiGo.SetActive(true);

            ui.OnEnter(args);

            return ui;
        }

        private void HideUI(IUIBase ui, bool isDestroy = false)
        {
            ui.IsShowing = false;
            ui.OnExit();
            if (isDestroy)
            {
                uiCacheDic.Remove(ui.UIName);
                Object.Destroy(ui.UIGo);
            }
            else
            {
                ui.UIHierarchy.PlayOut();
                //ui.uiGo.SetActive(false);
            }
        }

        /// <summary>
        /// 关闭所有
        /// </summary>
        public void CloseAll(bool isDestroy = false)
        {
            while (uiStack.Count > 0)
            {
                CloseStack(isDestroy);
            }

            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                CloseNormal(uiList[i].UIName, isDestroy);
            }
        }

        /// <summary>
        /// 关闭所有排除部分，栈UI会全部关闭
        /// </summary>
        public void CloseAllWithout(bool isDestroy, params string[] names)
        {
            while (uiStack.Count > 0)
            {
                CloseStack(isDestroy);
            }

            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                if (names.Contains(uiList[i].UIName))
                {
                    continue;
                }

                CloseNormal(uiList[i].UIName, isDestroy);
            }
        }

        private async UniTask<IUIBase> LoadUIAsync(string uiName)
        {
            // create ui gameobject
            string uiPath = string.Format(UI_RES_PATH, uiName);

            var resLoader = Resources.LoadAsync<GameObject>(uiPath);
            await resLoader;

            var prefab = resLoader.asset as GameObject;
            if (!prefab)
            {
                throw new Exception("load ui prefab failed:" + uiName);
            }

            return CreateClass(uiName, prefab);
        }

        private IUIBase LoadUI(string uiName)
        {
            // create ui gameobject
            string uiPath = string.Format(UI_RES_PATH, uiName);

            var prefab = Resources.Load<GameObject>(uiPath);
            if (!prefab)
            {
                throw new Exception("load ui prefab failed:" + uiName);
            }

            return CreateClass(uiName, prefab);
        }

        private IUIBase CreateClass(string uiName, GameObject prefab)
        {
            // get ui class name
            string uiOriginName = uiName.Replace("UI", "");
            string uiModelName = "UIModel" + uiOriginName;
            string uiViewName = "UIView" + uiOriginName;

            // create ui class
            Type uiType = Type.GetType(uiName);
            Type uiModelType = Type.GetType(uiModelName);
            Type uiViewType = Type.GetType(uiViewName);

            IUIBase ui = Activator.CreateInstance(uiType) as IUIBase;
            IUIModel uiModel = Activator.CreateInstance(uiModelType) as IUIModel;
            IUIView uiView = Activator.CreateInstance(uiViewType) as IUIView;

            var parent = uiRootGo.GetLayerTransform(ui.GetLayer());
            if (parent == null) throw new Exception("can not find the ui layer:" + ui.GetLayer());

            GameObject uiGo = GameObject.Instantiate(prefab, parent);
            uiGo.name = uiName;
            uiGo.transform.localPosition = Vector3.zero;
            uiGo.transform.localScale = Vector3.one;

            // init class
            uiView.Init(uiGo);
            ui.UIName = uiName;
            ui.UIGo = uiGo;
            ui.uiModel = uiModel;
            ui.uiView = uiView;

            //添加UI效果
            ui.UIHierarchy = ui.UIGo.AddComponent<UIHierarchy>();

            // init ui 
            ui.OnInit();

            // add to cache
            uiCacheDic.Add(uiName, ui);

            // complete
            return ui;
        }
    }
}