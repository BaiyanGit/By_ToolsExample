namespace Demos.示例_UI曲面叠加滚动.Scripts
{
    using UnityEngine;

    /// <summary>
    /// 描述
    /// </summary>
    [ExecuteInEditMode]
    public class OnGuiDesc : MonoBehaviour
    {
        [SerializeField] private Color _fontColor = new(0.8f, 0, 1, 1);
        [SerializeField] [Range(14, 40)] private int _fontSize = 32;
        [SerializeField] [Range(30, 100)] private float _yOffset = 50;
        [SerializeField] [Range(30, 100)] private float _heightBase = 50;

        private GUIStyle _style;

        private const string Desc = "<b><color=red><size=40> 不使用ScrollRect实现的UI叠加滚动 </size></color></b>\n\n" +
                                    "支持：\n" + "无限循环滚动、曲面滚动、曲面缩放、曲面颜色变化。\n\n" +
                                    "操作：\n" +
                                    "鼠标滚轮滚动、左右箭头键切换曲面、拖拽滚动、点击Item滚动居中。\n\n" +
                                    "曲面UI滚动：\n" +
                                    "在Image、RawImage、Text上挂载 <color=green>ByCurvedUI</color> 材质，并设置曲面参数\n" +
                                    "<color=red>注意：</color>相机一定是Camera模式且不能是正交！！！\n" +
                                    "(使用曲面后，鼠标点击Item触发会有聚焦错误问题，暂时无解)";

        private void OnGUI()
        {
            if (Application.isPlaying) return;
            _style ??= new GUIStyle
            {
                fontSize = 25,
                normal =
                {
                    textColor  = _fontColor,
                    background = Texture2D.grayTexture,
                },
            };

            // 使用 GUILayout 来适配内容大小
            GUILayout.BeginArea(new Rect(10, 10, Screen.width, Screen.height));
            _style.fontSize         = _fontSize;
            _style.normal.textColor = _fontColor;

            // 使用 GUILayout.Box 来适配内容大小，并设置灰色背景
            GUILayout.Box("", _style, GUILayout.Width(Screen.width - 30), GUILayout.Height(_heightBase));

            // 使用 GUILayout.Label 来适配内容大小
            GUILayout.Label(Desc, _style, GUILayout.Width(Screen.width - 30));

            GUILayout.EndArea();
        }
    }
}