namespace Demos.示例_UI曲面叠加滚动.无限滚动
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 箭头控制器 - 管理左右箭头的显示和交互
    /// </summary>
    public class RollViewArrowController : MonoBehaviour
    {
        [Header("左箭头")] public Button leftArrow;           // 左箭头按钮
        [Header("右箭头")] public Button rightArrow;          // 右箭头按钮
        [Header("箭头隐藏透明度")] public float disableAlpha = 0.3f; // 箭头禁用时的透明度

        /// <summary>
        /// 更新箭头的显示状态
        /// </summary>
        public void UpdateArrows(int currentPage, int totalPages, bool infiniteLoop)
        {
            if (leftArrow)
            {
                bool canMovePrev = infiniteLoop || currentPage > 0;
                SetArrowActive(leftArrow, canMovePrev);
            }

            if (rightArrow)
            {
                bool canMoveNext = infiniteLoop || currentPage < totalPages - 1;
                SetArrowActive(rightArrow, canMoveNext);
            }
        }

        /// <summary>
        /// 设置箭头的活动状态
        /// </summary>
        private void SetArrowActive(Button arrowButton, bool active)
        {
            if (!arrowButton || !arrowButton.image)
                return;

            var color = arrowButton.image.color;
            color.a = active ? 1f : disableAlpha;
            arrowButton.image.color = color;

            arrowButton.interactable = active;
        }
    }
}