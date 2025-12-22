namespace Example.ColorPanel
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// 色板
    /// </summary>
    public class ColorSaturation : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [Header("Canvas")] public Canvas canvas;
        [Header("色环选择图标")] public RectTransform selectorWheel;

        private RawImage _colorSaturation; //调色板
        private RawImage _selectorWheelImage;
        public RectTransform RectTrans { get; private set; }
        public UnityEvent<Color> colorEvent;

        private void Awake()
        {
            RectTrans = transform as RectTransform;
            _colorSaturation = GetComponent<RawImage>();
            _selectorWheelImage = selectorWheel.GetComponent<RawImage>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        /// <summary>
        /// Screen Space - Overlay
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 selectorWheelPos;
            switch (canvas.renderMode)
            {
                case RenderMode.ScreenSpaceCamera:
                    // 获取鼠标在Canvas上的位置
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTrans, eventData.position,
                        eventData.pressEventCamera, out selectorWheelPos);
                    break;
                case RenderMode.ScreenSpaceOverlay:
                    selectorWheelPos = transform.InverseTransformPoint(eventData.position);
                    break;
                case RenderMode.WorldSpace:
                default:
                    return;
            }

            selectorWheelPos.x = Mathf.Clamp(selectorWheelPos.x, 0, _colorSaturation.rectTransform.rect.width - 1);
            selectorWheelPos.y = Mathf.Clamp(selectorWheelPos.y, 0, _colorSaturation.rectTransform.rect.height - 1);
            selectorWheel.localPosition = selectorWheelPos;

            //获取调色板图片
            var tex = (Texture2D)_colorSaturation.texture;
            var rect = _colorSaturation.rectTransform.rect;
            var pos = selectorWheel.localPosition;
            //获取鼠标（smallIcon）在调色板上的x，y轴上的比例
            var swatchesX = pos.x / rect.width;
            var swatchesY = pos.y / rect.height;

            //设置圆圈颜色分类
            if (swatchesY <= 0.5f || swatchesX >= 0.5f) _selectorWheelImage.color = Color.white;
            else _selectorWheelImage.color = Color.black;

            //通过鼠标在调色板的位置比例，获取鼠标在调色板上的具体(X,Y)坐标
            var x = (int)(swatchesX * tex.width);
            var y = (int)(swatchesY * tex.height);
            SetFinalColor(tex.GetPixel(x, y)); //获取该位置下的像素的颜色值
        }

        /// <summary>
        /// 更新显示板的颜色
        /// </summary>
        /// <param name="eventData"></param>
        public void UpdateColor(PointerEventData eventData = null)
        {
            var tex = (Texture2D)_colorSaturation.texture; //获取调色板图片
            var rect = _colorSaturation.rectTransform.rect;
            var pos = selectorWheel.localPosition;
            //获取鼠标（smallIcon）在调色板上的x，y轴上的比例
            var swatchesX = pos.x / rect.width;
            var swatchesY = pos.y / rect.height;

            //通过鼠标在调色板的位置比例，获取鼠标在调色板上的具体(X,Y)坐标
            var x = (int)(swatchesX * tex.width);
            var y = (int)(swatchesY * tex.height);
            SetFinalColor(tex.GetPixel(x, y)); //获取该位置下的像素的颜色值,并设置该最终颜色
        }

        /// <summary>
        /// 设置最终的颜色
        /// </summary>
        /// <param name="color"></param>
        public void SetFinalColor(Color color)
        {
            colorEvent?.Invoke(color);
        }
    }
}