namespace _3rdBy.ByFramework.UI.Helper.Tab
{
    using UnityEngine;
    using UnityEngine.UI;

    public class TabItemHelper : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private TabPanelHelper tabPanel;

        public Toggle Toggle => toggle;

        public TabPanelHelper TabPanel => tabPanel;

        private void Awake()
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool arg0)
        {
            if (arg0)
            {
                tabPanel.OnShow();
            }
            else
            {
                tabPanel.OnHide();
            }
        }
    }
}