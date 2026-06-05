namespace _3rdBy.ByFramework.UI.UIExtend.FoldPanel
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class DataItem : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private Toggle _toggle;

        private ItemData _itemData;
        private UnityAction<string, bool> _onClick;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool arg0)
        {
            _onClick?.Invoke(_itemData.optionName, arg0);
        }

        public void SetInfo(ItemData data, ToggleGroup toggleGroup, UnityAction<string, bool> unityAction)
        {
            _itemData = data;
            _onClick = unityAction;

            _text.text = data.optionName;
            _toggle.group = toggleGroup;
        }
    }
}