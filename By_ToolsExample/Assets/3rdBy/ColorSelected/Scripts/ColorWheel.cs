namespace Example.ColorPanel
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    /// <summary>
    /// 颜色选择环
    /// </summary>
    public class ColorWheel : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [Header("Canvas")] public Canvas canvas;
        [Header("色环选择图标")] public RectTransform selectorWheel;
        [Header("色板操作")] public ColorSaturation subColorSelector;

        private Image _colorWheel; //圆环色图
        private RawImage _colorSaturation; //色板色图
        private Vector2 _newV2 = Vector2.zero;
        private Texture2D _saturationTexture2D; //调色板图片
        private const float PixelWidth = 256.0f; //色板像素的宽
        private const float PixelHeight = 256.0f; //色板像素的高
        private Vector4 _currentColorHSV = new(0, 1, 1, 1); //当前HSV
        private Transform _thisParent; //父级
        private Texture2D _swatches; //色板图片
        private Vector2 _centerPos; //选择盘的中心位置
        private float _selectPos; //取色环在选择圆盘的中心位置
        private RectTransform _rectTrans;

        private void Awake()
        {
            var thiTransform = transform;
            _thisParent = thiTransform.parent;
            _rectTrans = thiTransform as RectTransform;

            _colorWheel = GetComponent<Image>();
            _colorSaturation = subColorSelector.GetComponent<RawImage>();
            _swatches = _colorWheel.sprite.texture;
            _saturationTexture2D = new Texture2D((int)PixelWidth, (int)PixelHeight);
            _colorSaturation.texture = _saturationTexture2D;
        }

        private void Start()
        {
            GetSelectColorWheelPos();
            StartPosColor();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            CircleMove(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            CircleMove(eventData);
        }

        /// <summary>
        /// 圆圈移动
        /// </summary>
        /// <param name="eventData"></param>
        private void CircleMove(PointerEventData eventData)
        {
            Vector2 mousePos;

            switch (canvas.renderMode)
            {
                case RenderMode.ScreenSpaceCamera:
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTrans, eventData.position,
                        eventData.pressEventCamera, out mousePos);
                    break;
                case RenderMode.ScreenSpaceOverlay:
                    mousePos = eventData.position;
                    break;
                case RenderMode.WorldSpace:
                default:
                    return;
            }

            //圆心指向鼠标位置的向量 = 鼠标位置 - 圆形位置
            mousePos = _thisParent.InverseTransformPoint(mousePos) -
                       _thisParent.InverseTransformPoint(transform.position + (Vector3)_centerPos);
            var rightPos = _thisParent.InverseTransformVector(_colorWheel.rectTransform.right);
            SetColor(mousePos, rightPos, eventData);
        }

        /// <summary>
        /// 获取取色环的位置
        /// </summary>
        private void GetSelectColorWheelPos()
        {
            var rect = _colorWheel.rectTransform.rect;
            _centerPos = new Vector2(rect.width / 2, rect.height / 2); //计算颜色选择盘的中心位置
            _selectPos = rect.width / 2 - selectorWheel.rect.width / 2; //计算颜色选择器与颜色选择盘中心的距离
        }

        /// <summary>
        /// 根据颜色改变选择设置颜色
        /// </summary>
        /// <param name="color"></param>
        public void SetSelectorWheel(Color color)
        {
            Color.RGBToHSV(color, out var radian, out var saturation, out var brightness); //计算传入颜色的HSV
            UpdateSelectorWheel(color, radian, saturation, brightness);
        }

        /// <summary>
        /// 根据图片颜色设置颜色
        /// </summary>
        /// <param name="img"></param>
        public void SetSelectorWheel(Graphic img)
        {
            Color.RGBToHSV(img.color, out var radian, out var saturation, out var brightness); //计算传入颜色的HSV
            UpdateSelectorWheel(img.color, radian, saturation, brightness);
        }

        /// <summary>
        /// 更新颜色和选择圆圈的位置
        /// </summary>
        /// <param name="color"></param>
        /// <param name="radian"></param>
        /// <param name="saturation"></param>
        /// <param name="brightness"></param>
        private void UpdateSelectorWheel(Color color, float radian, float saturation, float brightness)
        {
            //Color.RGBToHSV(img.color, out var radian, out var saturation, out var brightness); //计算传入颜色的HSV
            RGBToHSV(Color.HSVToRGB(radian, 1, 1)); //获取内部色盘的最亮色

            _newV2.x = _centerPos.x + _selectPos * Mathf.Cos(radian * 360 * Mathf.Deg2Rad);
            _newV2.y = _centerPos.y + _selectPos * Mathf.Sin(radian * 360 * Mathf.Deg2Rad);
            selectorWheel.localPosition = _newV2; //计算色环小圈的位置

            UpdateSaturation(_currentColorHSV); //更新内部色盘图片

            var rect1 = subColorSelector.RectTrans.rect;
            subColorSelector.selectorWheel.transform.localPosition =
                new Vector2(saturation * rect1.width, brightness * rect1.height); //计算内部色盘小圈位置
            subColorSelector.SetFinalColor(color);
        }

        /// <summary>
        /// 开始位置颜色
        /// </summary>
        private void StartPosColor()
        {
            //圆心指向鼠标位置的向量 = 鼠标位置 - 圆形位置 
            var mousePos = _thisParent.InverseTransformPoint(selectorWheel.position) -
                           _thisParent.InverseTransformPoint(transform.position + (Vector3)_centerPos);
            var rightPos = _thisParent.InverseTransformVector(_colorWheel.rectTransform.right);

            SetColor(mousePos, rightPos);
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="rightPos"></param>
        /// <param name="eventData"></param>
        private void SetColor(Vector2 mousePos, Vector2 rightPos, PointerEventData eventData = null)
        {
            var angle = VectorAngle(mousePos, rightPos);
            _newV2.x = _centerPos.x + _selectPos * Mathf.Cos(angle * Mathf.Deg2Rad);
            _newV2.y = _centerPos.y + _selectPos * Mathf.Sin(angle * Mathf.Deg2Rad);
            selectorWheel.localPosition = _newV2;

            //1.计算选色环在Image上的位置占比 (0~1)
            //2.通过位置占比,映射获取到要获取的贴图像素位置
            var rect = _colorWheel.rectTransform.rect;
            var pos = selectorWheel.localPosition;
            var x = (int)(pos.x / rect.width * _swatches.width);
            var y = (int)(pos.y / rect.height * _swatches.height);

            var color = _swatches.GetPixel(x, y); //获取该位置下的像素的颜色值
            RGBToHSV(color);
            UpdateSaturation(_currentColorHSV); //更新色板颜色
            if (eventData == null) return; //如果是初始化的话，就不用设置了。因为颜色已经设定过了
            subColorSelector.UpdateColor(eventData); //更新显示板的颜色
        }

        /// <summary>
        /// 更新调色板
        /// </summary>
        /// <param name="hsv"></param>
        private void UpdateSaturation(Vector4 hsv)
        {
            for (var y = 0; y < PixelHeight; y++)
            {
                for (var x = 0; x < PixelWidth; x++)
                {
                    var pixColor = GetSaturation(hsv, x / PixelWidth, y / PixelHeight);
                    _saturationTexture2D.SetPixel(x, y, pixColor);
                }
            }

            _saturationTexture2D.Apply();
            _colorSaturation.texture = _saturationTexture2D;
        }

        /// <summary>
        /// 根据设置分辨率转化HSV
        /// </summary>
        /// <param name="hsv"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static Color GetSaturation(Vector4 hsv, float x, float y)
        {
            var saturationHSV = hsv;
            saturationHSV.y = x;
            saturationHSV.z = y;
            saturationHSV.w = 1;
            return HSVToRGB(saturationHSV);
        }

        /// <summary>
        /// 根据 HSV 转化 RGB
        /// </summary>
        /// <param name="hsv"></param>
        /// <returns></returns>
        private static Color HSVToRGB(Vector4 hsv)
        {
            var color = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
            color.a = hsv.w;
            return color;
        }

        /// <summary>
        /// 根据 RBG 转化 HSV
        /// </summary>
        /// <param name="color"></param>
        private void RGBToHSV(Color color)
        {
            Color.RGBToHSV(color, out _currentColorHSV.x, out _currentColorHSV.y, out _currentColorHSV.z);
            // 更新调色板的位置
        }

        /// <summary>
        /// 计算两个向量之间的角度
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private static float VectorAngle(Vector2 from, Vector2 to)
        {
            var cross = Vector3.Cross(from, to);
            var angle = Vector2.Angle(from, to);
            return cross.z > 0 ? -angle : angle;
        }
    }
}