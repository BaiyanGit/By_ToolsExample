namespace _3rdBy.ByFramework.UI
{
    using System;
    using System.Collections.Generic;
    using Singleton;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public enum UILayer
    {
        Low,
        Center,
        Top,
        Guide,
    }

    public enum UIType
    {
        List,
        Stack
    }

    public class UIRoot : MonoSingletonTemplate<UIRoot>
    {
        private readonly Dictionary<UILayer, RectTransform> _rootDic = new();

        public Camera uiCamera;
        public Canvas canvas;

        public Action UpdateAction { get; set; }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            // CreateUIRoot();
            FindUILayer();
        }

        private void Update()
        {
            UpdateAction?.Invoke();
        }

        private void FindUILayer()
        {
            foreach (UILayer type in Enum.GetValues(typeof(UILayer)))
            {
                var t = transform.Find($"Canvas/{type.ToString()}_Layer");

                _rootDic.Add(type, t.GetComponent<RectTransform>());
            }
        }

        public RectTransform GetLayerTransform(UILayer uiLayer)
        {
            _rootDic.TryGetValue(uiLayer, out var tempTrans);
            return tempTrans;
        }

        private GameObject CreateUIRoot()
        {
            GameObject go = new GameObject("UIRoot");
            DontDestroyOnLoad(go);

            go.transform.localPosition = Vector3.zero;
            go.transform.localScale    = Vector3.one;
            go.layer                   = LayerMask.NameToLayer("UI");

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2;

            var canvasScaler = go.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.Shrink;

            go.AddComponent<GraphicRaycaster>();

            //create ui node
            foreach (UILayer type in Enum.GetValues(typeof(UILayer)))
            {
                var node = new GameObject(type.ToString());
                node.transform.parent        = go.transform;
                node.transform.localPosition = Vector3.zero;
                node.transform.localScale    = Vector3.one;
                node.layer                   = go.layer;

                //����RectTransform ʹ�ڵ�������Ļ
                var rect = node.AddComponent<RectTransform>();
                rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
                rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
            }

            //add event system
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.transform.parent        = go.transform;
            eventSystem.transform.localPosition = Vector3.zero;
            eventSystem.transform.localScale    = Vector3.one;

            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            eventSystem.AddComponent<BaseInput>();

            return go;
        }
    }
}
