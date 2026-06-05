using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _3rdBy.ByFramework.UI.UIExtend.ExcelItem
{
    using ByTools.JsonConvertTool.ExcelDataTool.ExcelData;

    /// <summary>
    /// Excel数据列表项实例化，用于显示Excel数据列表
    /// </summary>
    public class ExcelUIView : MonoBehaviour
    {
        [Header("实例容器"), SerializeField] private Transform content;
        [Header("实例对象"), SerializeField] private ExcelItemBase _item;

        public List<ExcelItemBase> excelItemsList { get; } = new();

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataList">数据</param>
        /// <param name="onClickAction">回调</param>
        /// <param name="cont">实例化对象的服父物体</param>
        /// <param name="isJoin">是否使用已经生成的</param>
        public void InitContent<T>(List<T> dataList, UnityAction<T> onClickAction = null) where T : ExcelObject
        {
            _item.gameObject.SetActive(false);

            if (dataList == null || dataList.Count == 0)
            {
                Debug.LogError("实例化的数据列表为空");
                return;
            }

            for (var i = 0; i < dataList.Count; i++)
            {
                ExcelItemBase item;
                if (i < excelItemsList.Count)
                {
                    item = excelItemsList[i];
                }
                else
                {
                    item = Instantiate(_item, content);
                    excelItemsList.Add(item);
                }

                item.name = $"ExcelItem_{i}";
                item.gameObject.SetActive(true);

                item.InitData(dataList[i], onClickAction);
            }

            _ = RefreshLayout();
        }

        public void ClearContent()
        {
            foreach (var item in excelItemsList)
            {
                item.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 刷新布局，包含ContentSizeFitter
        /// </summary>
        private async UniTaskVoid RefreshLayout()
        {
            var sizeFitter = content.GetComponent<ContentSizeFitter>();
            if (sizeFitter)
            {
                sizeFitter.enabled = false;
                await UniTask.Delay(1);
                sizeFitter.enabled = true;
            }

            await UniTask.Delay(1);
            LayoutRebuilder.MarkLayoutForRebuild(content as RectTransform);
        }
    }
}