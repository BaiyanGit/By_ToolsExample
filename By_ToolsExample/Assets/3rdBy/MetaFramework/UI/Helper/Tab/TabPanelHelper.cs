namespace _3rdBy.MetaFramework.UI.Helper.Tab
{
    using UnityEngine;

    [RequireComponent(typeof(CanvasGroup))]
    public class TabPanelHelper : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void OnShow()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            _canvasGroup.transform.SetAsLastSibling();
        }

        public virtual void Refresh()
        {
            
        }

        public virtual void OnHide()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _canvasGroup.transform.SetAsFirstSibling();
        }
    }
}