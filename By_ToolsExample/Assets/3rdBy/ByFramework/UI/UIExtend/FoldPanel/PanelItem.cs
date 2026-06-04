namespace _3rdBy.MetaFramework.UI.FoldPanel
{
    using DG.Tweening;
    using UnityEngine;

    public class PanelItem : MonoBehaviour
    {
        [Header("折叠动画时间")] [SerializeField] private float foldTime = 0.1f;

        public void Open()
        {
            gameObject.SetActive(true);
            transform.DOScaleY(1, foldTime);
        }

        public void Close()
        {
            transform.DOScaleY(0, foldTime).OnComplete(() => { gameObject.SetActive(false); });
        }
    }
}