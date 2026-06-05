namespace _3rdBy.ByFramework.UI.Helper.Tab
{
    using System.Collections.Generic;
    using UnityEngine;

    public class TabListHelper : MonoBehaviour
    {
        [SerializeField] private List<TabItemHelper> tabItems;

        public TabItemHelper ActiveTabItem
        {
            get
            {
                return tabItems.Find(item => item.Toggle.isOn);
            }
        }

        public void ForceSetTab(int index)
        {
            if (index < 0 || index >= tabItems.Count)
            {
                return;
            }

            for (var i = 0; i < tabItems.Count; i++)
            {
                var isOn = i == index;
                tabItems[i].Toggle.SetIsOnWithoutNotify(isOn);
                tabItems[i].Toggle.onValueChanged.Invoke(isOn);
            }
        }
    }
}