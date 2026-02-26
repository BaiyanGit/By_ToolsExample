namespace Demos.示例_UI曲面叠加滚动.无限滚动模块化.辅助脚本
{
    using UnityEngine;
    using UnityEngine.UI;

    public class RollViewInfoPanel : MonoBehaviour
    {
        [TextArea] private const string Desc = "<b><color=red><size=40> 不使用ScrollRect实现的UI叠加滚动 </size></color></b>\n\n" +
                                               "支持：\n" + "无限循环滚动、曲面滚动、曲面缩放、曲面颜色变化。\n\n" +
                                               "操作：\n" +
                                               "鼠标滚轮滚动、左右箭头键切换曲面、拖拽滚动、点击Item滚动居中。\n\n" +
                                               "曲面UI滚动：\n" +
                                               "在Image、RawImage、Text上挂载 <color=green>ByCurvedUI</color> 材质，并设置曲面参数\n" +
                                               "<color=red>注意：相机一定是Camera模式且不能是正交！！！(使用曲面后，鼠标点击Item触发会有聚焦错误问题，暂时无解)</color>";

        [Header("描述文本")] public Text descText;
        [Header("展开面板按钮")] public Button btnExpand;

        [Header("展开位置")] public Vector2 expandPos = new(350, 0);
        [Header("收缩位置")] public Vector2 shrinkPos = new(-350, 0);

        private RectTransform _rectTransform;
        private bool _isExpanded;

        private void OnValidate()
        {
            if (descText)
            {
                descText.text = Desc;
            }
        }

        private void Start()
        {
            _rectTransform = transform as RectTransform;
            btnExpand.onClick.AddListener(OnExpand);
        }

        private void OnExpand()
        {
            _isExpanded = !_isExpanded;
        }

        private void Update()
        {
            var pos = _isExpanded ? expandPos : shrinkPos;
            _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, pos, 0.1f);
        }
    }
}