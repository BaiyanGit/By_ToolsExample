namespace _3rdBy.MetaFramework.UI.FoldPanel
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class TitleItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Text _title;
        //[SerializeField]
        //private Transform arrow;

        private FoldData _foldData;
        private PanelItem _panelItem;
        private bool _isFold = true; // 是否是折叠状态
        private UnityAction<string, bool> _onClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isFold)
            {
                _isFold = false;
                //arrow.DORotate(Vector3.zero, 0.1f);
                _panelItem.Open();
            }
            else
            {
                _isFold = true;
                //arrow.DORotate(new Vector3(0, 0, 90), 0.1f);
                _panelItem.Close();
            }

            _onClick?.Invoke(_foldData.titleName, _isFold);
        }

        public void SetTitle(FoldData foldData, UnityAction<string, bool> unityAction)
        {
            _foldData = foldData;
            _onClick = unityAction;

            _title.text = foldData.titleName;
        }

        public void SetFoldPanel(PanelItem panelItem)
        {
            _panelItem = panelItem;
        }
    }
}