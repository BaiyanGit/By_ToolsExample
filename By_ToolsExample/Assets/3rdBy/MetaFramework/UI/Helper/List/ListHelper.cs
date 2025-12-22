using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MetaFramework.Helper
{
    //[RequireComponent(typeof(GridLayoutGroup))]
    public class ListHelper : MonoBehaviour
    {
        [SerializeField] private ListItemHelper listItem;
        [SerializeField] private RectTransform container;
        [SerializeField] private ScrollRect scrollRect;

        public List<ListItemHelper> listItemHelpers = new List<ListItemHelper>();
        public List<ListItemHelper> activeListItemHelpers = new List<ListItemHelper>();

        /// <summary>
        /// 滚动到屏幕外的回调
        /// </summary>
        public UnityAction<bool> OnScrollOverScreenHandler { get; set; }

        private float _tempTime = 0f;
        private const float LoopInterval = 0.1f;
        private CancellationTokenSource _cts;

        protected virtual void Awake()
        {
            if (listItem == null)
            {
                //如果没有指定listItem，就抛出异常，显示当前list的名字
                throw new Exception($"没有指定listItem，当前list名字为{gameObject.name}");
            }

            listItem.gameObject.SetActive(false);
            if (container == null)
            {
                //如果没有指定container，就把自己当作container
                container = transform.GetComponent<RectTransform>();
            }

            if (scrollRect == null)
            {
                //如果没有指定scrollRect，就找父级的scrollRect
                scrollRect = GetComponentInParent<ScrollRect>();
            }
        }

        private void Update()
        {
            //如果有scrollRect，就判断是否滚动到屏幕外
            if (scrollRect != null)
            {
                if (OnScrollOverScreenHandler != null)
                {
                    _tempTime += Time.deltaTime;
                    if (_tempTime > LoopInterval)
                    {
                        _tempTime = 0f;
                        OnScrollOverScreenHandler.Invoke(container.anchoredPosition.y > scrollRect.viewport.rect.height);
                    }
                }
            }
        }

        public virtual void Refresh<T>(List<T> dataList, params object[] param) where T : ListItemData
        {
            activeListItemHelpers.Clear();
            if (dataList.Count > listItemHelpers.Count)
            {
                var count = dataList.Count - listItemHelpers.Count;
                //创建差值个数的item
                for (var i = 0; i < count; i++)
                {
                    var newItem = Instantiate(listItem, container);
                    var itemScript = newItem.GetComponent<ListItemHelper>();
                    listItemHelpers.Add(itemScript);
                }
            }

            for (var i = 0; i < listItemHelpers.Count; i++)
            {
                var itemScript = listItemHelpers[i];
                if (i < dataList.Count)
                {
                    itemScript.SetData(dataList[i], param);
                    itemScript.gameObject.SetActive(true);
                    activeListItemHelpers.Add(itemScript);
                }
                else
                {
                    itemScript.gameObject.SetActive(false);
                }
            }
        }

        public async UniTask NavigateTo(int index, float time = 0.5f)
        {
            if (index < 0 || index >= activeListItemHelpers.Count)
            {
                Debug.LogWarning($"index:{index}超出范围");
                return;
            }

            await NavigateTo(activeListItemHelpers[index], time);
        }

        public async UniTask NavigateTo(ListItemHelper itemHelper, float time = 0.5f)
        {
            if (scrollRect == null)
            {
                Debug.LogWarning("没有设置scrollRect");
                return;
            }

            _cts?.Cancel();

            var itemRt = itemHelper.GetComponent<RectTransform>();
            var itemCurrentLocalPos = scrollRect.GetComponent<RectTransform>()
                .InverseTransformVector(ConvertLocalPosToWorldPos(itemRt));
            var itemTargetLocalPos = scrollRect.GetComponent<RectTransform>()
                .InverseTransformVector(ConvertLocalPosToWorldPos(scrollRect.viewport));
            var diff = itemTargetLocalPos - itemCurrentLocalPos;
            diff.z = 0.0f;
            var newNormalizedPosition = new Vector2(
                diff.x / (scrollRect.content.rect.width - scrollRect.viewport.rect.width),
                diff.y / (scrollRect.content.rect.height - scrollRect.viewport.rect.height)
            );

            newNormalizedPosition = scrollRect.normalizedPosition - newNormalizedPosition;
            newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
            newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);

            //TODO:滚动到对应的位置
            _cts = new CancellationTokenSource();
            
            //TODO:需要在Unity宏定义里面定义一个：UNITASK_DOTWEEN_SUPPORT
            await DOTween.To(() => scrollRect.normalizedPosition,
                    x => scrollRect.normalizedPosition = x, newNormalizedPosition, time)
                .AwaitForComplete(cancellationToken: _cts.Token);
        }

        private Vector3 ConvertLocalPosToWorldPos(RectTransform target)
        {
            var pivotOffset = new Vector3(
                (0.5f - target.pivot.x) * target.rect.size.x,
                (0.5f - target.pivot.y) * target.rect.size.y,
                0f);

            var localPosition = target.localPosition + pivotOffset;

            return target.parent.TransformPoint(localPosition);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}