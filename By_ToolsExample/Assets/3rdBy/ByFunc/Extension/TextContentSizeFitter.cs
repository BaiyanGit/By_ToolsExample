namespace _3rdBy.ByFunc.Extension {
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 根据文本内容决定背景图大小适配
    /// </summary>

    [ExecuteAlways]
    [AddComponentMenu ("Layout/TextContentSizeFitter")]
    public class TextContentSizeFitter : ContentSizeFitter {
        //TODO：offsetSize、widthLimit：属性不在属性面板显示，如需在属性面板修改，请在Inspector中选择Debug模式进行修改。
        [SerializeField] private Vector2 offsetSize = new Vector2 (100, 50); //背景图大小适配偏移大小
        [SerializeField] private float widthLimit = 500; //背景限制显示宽度
        private Text self_Text;
        private RectTransform parentRect;
        private float maxWidth;
        private bool start;

        protected override void OnEnable () {
            maxWidth = Screen.width - widthLimit;

            self_Text = this.GetComponent<Text> ();

            parentRect = this.transform.parent.GetComponent<RectTransform> ();
            
            start = true;
        }

        protected override void OnDisable () {
            start = false;
            self_Text.rectTransform.sizeDelta = Vector2.zero;
            this.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // Update is called once per frame
        private void Update () {
            if (start) {
                if (self_Text.preferredWidth > maxWidth) {
                    self_Text.alignment = TextAnchor.MiddleLeft;
                    self_Text.rectTransform.sizeDelta = new Vector2 (maxWidth, self_Text.rectTransform.sizeDelta.y);
                    this.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    this.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                } else {
                    self_Text.alignment = TextAnchor.MiddleCenter;
                    this.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    this.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                }

            }
            parentRect.sizeDelta = new Vector2 (self_Text.rectTransform.rect.width + offsetSize.x, self_Text.rectTransform.rect.height + offsetSize.y);
        }
    }
}