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
        [Header("启用拖拽")] public bool enableDragInput = true;
        [Header("实时拖拽")] public bool realtimeDragInput = true;
        [Header("启用键盘")] public bool enableKeyboardInput = true;
        [Header("启用鼠标")] public bool enableMouseInput = true;

        [Header("上一个(按钮)")] public Button prevButton;
        [Header("下一个(按钮)")] public Button nextButton;
        [Header("拖拽时滚动的敏感度")] public float dragSensitivity = 0.001f;


        public event Action<float, bool> OnDragAction;
        public event Action              OnBeginDragAction;
        public event Action              OnEndDragAction;
        public event Action              OnMoveNextAction;
        public event Action              OnMovePrevAction;

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
            if (!enableDragInput) return;
            isDragging = true;
            OnBeginDragAction?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            float delta = -eventData.delta.x * dragSensitivity;
            OnDragAction?.Invoke(delta, realtimeDragInput);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!enableDragInput) return;
            isDragging = false;
            OnEndDragAction?.Invoke();
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            // 鼠标滚轮
            if (!enableMouseInput) return;
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(wheel) > 0.01f)
            {
                if (wheel > 0)
                    OnMovePrevAction?.Invoke();
                else
                    OnMoveNextAction?.Invoke();
            }

            // 键盘
            if (!enableKeyboardInput) return;
            if (Input.GetKeyDown(KeyCode.RightArrow)) OnMoveNextAction?.Invoke();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) OnMovePrevAction?.Invoke();
        }
#endif
    }
}