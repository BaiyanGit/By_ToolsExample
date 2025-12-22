using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ZCustom
{
    /// <summary>
    /// UGUI 折叠框
    /// </summary>
    public class FoldPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelItem; // 折叠页
        [SerializeField] private TitleItem _titleItem;
        [SerializeField] private DataItem _dataItem;
        [SerializeField] private Transform _content;

        private ToggleGroup _toggleGroup;

        public UnityEvent<string, bool> onClick;

        public List<FoldData> dataList = new List<FoldData>();

        private void Awake()
        {
            _toggleGroup = _content.GetComponent<ToggleGroup>();
        }

        private void Start()
        {
            foreach (var foldData in dataList)
            {
                // 创建标题
                var titleObj = Instantiate(_titleItem, _content);
                var title = titleObj.GetComponent<TitleItem>();
                title.SetTitle(foldData, (str, bo) => { onClick?.Invoke(str, bo); });
                title.gameObject.SetActive(true);

                // 创建子折叠面板
                var panelObj = Instantiate(_panelItem, _content);
                var panel = panelObj.GetComponent<PanelItem>();
                // 260是折叠页的宽度，30DataItem的高度
                //panel.GetComponent<RectTransform>().sizeDelta = new Vector3(260, 30 * dataList[i].data.Count);
                title.SetFoldPanel(panel);

                // 创建折叠页数据
                foreach (var itemData in foldData.data)
                {
                    var itemObj = Instantiate(_dataItem, panel.transform);
                    var item = itemObj.GetComponent<DataItem>();
                    item.gameObject.SetActive(true);
                    item.SetInfo(itemData, _toggleGroup, (str, bo) => { onClick?.Invoke(str, bo); });
                }
            }
        }
    }


    [System.Serializable]
    public class FoldData
    {
        public string titleName;
        public List<ItemData> data;
    }

    [System.Serializable]
    public class ItemData
    {
        public string optionName;
        //public string imageName;
        //public Sprite imageName;
    }
}