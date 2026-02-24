namespace Demos.示例_UI曲面叠加滚动.无限滚动模块化
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// 输入处理器 - 处理拖拽、键盘和鼠标输入
    /// </summary>
    public class RollViewInputHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("上一个")] public Button prevButton;
        [Header("下一个")] public Button nextButton;
        [Header("拖拽力度")] public float dragSensitivity = 0.001f; // 拖拽时滚动的敏感度
        [Header("滚轮速度")] public float wheelSpeed = 0.8f;        // 鼠标滚轮的滚动速度

        public event Action<float> OnDragAction;
        public event Action        OnBeginDragAction;
        public event Action        OnEndDragAction;
        public event Action        OnMoveNextAction;
        public event Action        OnMovePrevAction;

        public bool isDragging { get; private set; }


        private void Start()
        {
            // 添加 null 检查，避免未绑定按钮时报错
            if (prevButton != null)
                prevButton.onClick.AddListener(() => { OnMovePrevAction?.Invoke(); });

            if (nextButton != null)
                nextButton.onClick.AddListener(() => { OnMoveNextAction?.Invoke(); });
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            OnBeginDragAction?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            float delta = -eventData.delta.x * dragSensitivity;
            OnDragAction?.Invoke(delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            OnEndDragAction?.Invoke();
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            // 鼠标滚轮
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(wheel) > 0.01f)
            {
                OnDragAction?.Invoke(-wheel * wheelSpeed);
            }

            // 键盘
            if (Input.GetKeyDown(KeyCode.RightArrow)) OnMoveNextAction?.Invoke();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) OnMovePrevAction?.Invoke();
        }
#endif
    }
}