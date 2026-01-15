namespace _3rdBy.MetaFramework.UI.FoldPanel
{
    using DG.Tweening;
    using UnityEngine;

    public class PanelItem : MonoBehaviour
    {
        public void Open()
        {
            gameObject.SetActive(true);
            transform.DOScaleY(1, 0.1f);
        }

        public void Close()
        {
            transform.DOScaleY(0, 0.1f).OnComplete(() => { gameObject.SetActive(false); });
        }
    }
}