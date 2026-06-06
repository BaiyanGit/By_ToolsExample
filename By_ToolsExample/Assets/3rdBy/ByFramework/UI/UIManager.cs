namespace _3rdBy.ByFramework.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Interface;
    using Singleton;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// UI 管理器
    /// MVC模式的UI类命名规则
    /// Prefab  => UIxx
    /// Control => UIxx
    /// Model   => UIModelXXX
    /// View    => UIModelXXX
    /// </summary>
    public class UIManager : SingletonTemplate<UIManager>
    {
        private const string UI_RES_PATH = "Prefab/UI/{0}";

        /// <summary>
        /// UIRoot
        /// </summary>
        private readonly UIRoot _uiRootGo;

        /// <summary>
        /// UI缓存
        /// </summary>
        private readonly Dictionary<string, IUIBase> _uiCacheDic;

        /// <summary>
        /// UI栈
        /// </summary>
        private readonly Stack<IUIBase> _uiStack;

        /// <summary>
        /// UI列表
        /// </summary>
        private readonly List<IUIBase> _uiList;

        public UIManager()
        {
            _uiCacheDic = new Dictionary<string, IUIBase>();
            _uiStack    = new Stack<IUIBase>();
            _uiList     = new List<IUIBase>();

            _uiRootGo              = UIRoot.Instance; //CreateUIRoot();
            _uiRootGo.UpdateAction = Update;
        }

        private void Update()
        {
            foreach (var uiBase in _uiStack.Where(uiBase => uiBase.IsShowing))
            {
                uiBase.OnUpdate();
            }

            foreach (var uiBase in _uiList.Where(uiBase => uiBase.IsShowing))
            {
                uiBase.OnUpdate();
            }
        }

        public IUIBase Get(string uiName)
        {
            //Debug.LogError("[UIManager] 未在缓存中找到 UI：" + uiName);
            return _uiCacheDic.GetValueOrDefault(uiName);
        }

        /// <summary>
        /// 异步打开普通UI
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public async UniTask<IUIBase> OpenNormalAsync(string uiName, params object[] args)
        {
            if (!_uiCacheDic.TryGetValue(uiName, out var ui))
            {
                ui = await LoadUIAsync(uiName);
            }

            if (!_uiList.Contains(ui))
            {
                _uiList.Add(ui);
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
            if (!_uiCacheDic.TryGetValue(uiName, out var ui))
            {
                ui = LoadUI(uiName);
            }

            if (!_uiList.Contains(ui))
            {
                _uiList.Add(ui);
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
            if (_uiList.Count <= 0) return;

            var ui = _uiList.Find(ui => ui.UIName.Equals(uiName));

            if (ui == null)
            {
                Debug.LogWarning("[UIManager] 未找到 UI：" + uiName);
                return;
            }

            _uiList.Remove(ui);

            HideUI(ui, isDestroy);
        }

        /// <summary>
        /// 异步打开栈UI
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="onComplete"></param>
        public async UniTask<IUIBase> OpenStackAsync(string uiName, params object[] args)
        {
            if (_uiStack.Count > 0)
            {
                var topUI = _uiStack.Peek();
                topUI.OnPause();
            }

            if (!_uiCacheDic.TryGetValue(uiName, out var ui))
            {
                ui = await LoadUIAsync(uiName);
            }

            _uiStack.Push(ui);

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
            if (_uiStack.Count > 0)
            {
                var topUI = _uiStack.Peek();
                topUI.OnPause();
            }

            if (!_uiCacheDic.TryGetValue(uiName, out var ui))
            {
                ui = LoadUI(uiName);
            }

            _uiStack.Push(ui);

            ui.UIType = UIType.Stack;

            return ShowUI(ui, args);
        }

        /// <summary>
        /// 关闭栈UI
        /// </summary>
        public void CloseStack(bool isDestroy = false)
        {
            if (_uiStack.Count <= 0) return;

            var ui = _uiStack.Pop();

            HideUI(ui, isDestroy);

            if (_uiStack.Count > 0)
            {
                var topUI = _uiStack.Peek();
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
                _uiCacheDic.Remove(ui.UIName);
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
            while (_uiStack.Count > 0)
            {
                CloseStack(isDestroy);
            }

            for (int i = _uiList.Count - 1; i >= 0; i--)
            {
                CloseNormal(_uiList[i].UIName, isDestroy);
            }
        }

        /// <summary>
        /// 关闭所有排除部分，栈UI会全部关闭
        /// </summary>
        public void CloseAllWithout(bool isDestroy, params string[] names)
        {
            while (_uiStack.Count > 0)
            {
                CloseStack(isDestroy);
            }

            for (int i = _uiList.Count - 1; i >= 0; i--)
            {
                if (names.Contains(_uiList[i].UIName))
                {
                    continue;
                }

                CloseNormal(_uiList[i].UIName, isDestroy);
            }
        }

        private async UniTask<IUIBase> LoadUIAsync(string uiName)
        {
            // 创建ui游戏对象
            string uiPath = string.Format(UI_RES_PATH, uiName);

            var resLoader = Resources.LoadAsync<GameObject>(uiPath);
            await resLoader;

            var prefab = resLoader.asset as GameObject;
            if (!prefab)
            {
                throw new Exception("加载ui预制件失败:" + uiName);
            }

            return CreateClass(uiName, prefab);
        }

        private IUIBase LoadUI(string uiName)
        {
            // 创建ui游戏对象
            string uiPath = string.Format(UI_RES_PATH, uiName);

            var prefab = Resources.Load<GameObject>(uiPath);
            if (!prefab)
            {
                throw new Exception("加载ui预制件失败:" + uiName);
            }

            return CreateClass(uiName, prefab);
        }

        private IUIBase CreateClass(string uiName, GameObject prefab)
        {
            // 获取ui类名称
            string uiOriginName = uiName.Replace("UI", "");
            string uiModelName  = "UIModel" + uiOriginName;
            string uiViewName   = "UIView" + uiOriginName;

            // 创建ui类
            var uiType      = Type.GetType(uiName);
            var uiModelType = Type.GetType(uiModelName);
            var uiViewType  = Type.GetType(uiViewName);

            if (uiType == null || uiModelType == null || uiViewType == null)
                throw new Exception($"UI类可能包含了不必要的命名空间:{uiName}");

            var ui      = Activator.CreateInstance(uiType) as IUIBase;
            var uiModel = Activator.CreateInstance(uiModelType) as IUIModel;
            var uiView  = Activator.CreateInstance(uiViewType) as IUIView;

            if (ui == null) throw new Exception($"创建UI类失败:{uiName}");
            var parent = _uiRootGo.GetLayerTransform(ui.GetLayer());
            if (parent == null) throw new Exception($"找不到UI层:{ui.GetLayer()}");

            var uiGo = Object.Instantiate(prefab, parent);
            uiGo.name                    = uiName;
            uiGo.transform.localPosition = Vector3.zero;
            uiGo.transform.localScale    = Vector3.one;

            // 初始化UI类
            uiView?.Init(uiGo);
            ui.UIName  = uiName;
            ui.UIGo    = uiGo;
            ui.UIModel = uiModel;
            ui.UIView  = uiView;

            // 添加UI效果
            ui.UIHierarchy = ui.UIGo.AddComponent<UIHierarchy>();

            // 初始化UI
            ui.OnInit();

            // 添加到缓存
            _uiCacheDic.Add(uiName, ui);

            // 完成
            return ui;
        }
    }
}
