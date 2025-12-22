using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MetaFramework.Helper
{
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