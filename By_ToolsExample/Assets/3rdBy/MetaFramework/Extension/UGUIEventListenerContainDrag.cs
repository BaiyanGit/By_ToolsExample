// ----------------------------------------------------------------------------------------------------
// Copyright © Guo jin ming. All rights reserved.
// Homepage: https://kylin.app/
// E-Mail: kevin@kylin.app
// ----------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZCustom
{
    /// <summary>
    ///  UGUI 事件 监听 并且包含拖拽
    /// </summary>
    public class UGUIEventListenerContainDrag : MonoBehaviour,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public delegate void VoidDelegate(GameObject go);

        public delegate void VoidDelegateDrag(GameObject go, PointerEventData eventData);

        public string mAudioType;

        public VoidDelegate onClick;
        public VoidDelegate onDown;
        public VoidDelegate onEnter;
        public VoidDelegate onExit;
        public VoidDelegate onUp;
        public VoidDelegateDrag onDragStart;
        public VoidDelegateDrag onDrag;
        public VoidDelegateDrag onDragEnd;
        public VoidDelegate onLongPress;

        private bool _isUp = true; // 与长按配合使用
        private float _time = 0;

        #region ... Unity Api

        private void Update()
        {
            //长按
            if (!_isUp && onLongPress != null && Time.realtimeSinceStartup - _time > 1)
            {
                onLongPress(gameObject);
                _isUp = true;
            }
        }

        #endregion

        #region ... IPointerHandler

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null)
            {
                if (!string.IsNullOrEmpty(mAudioType) && GetComponent<Button>() != null &&
                    GetComponent<Button>().interactable)
                {
                    // TODO : 播放声音 mAudioType 
                }

                if (GetComponent<Button>() == null || GetComponent<Button>().interactable)
                    onClick(gameObject);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onDown?.Invoke(gameObject);
            if (_isUp) _time = Time.realtimeSinceStartup;
            _isUp = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onEnter?.Invoke(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onExit?.Invoke(gameObject);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onUp?.Invoke(gameObject);
            _isUp = true;
            _time = 0;
        }

        #endregion

        #region ... IDragHandler

        public void OnBeginDrag(PointerEventData eventData)
        {
            onDragStart?.Invoke(gameObject, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(gameObject, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onDragEnd?.Invoke(gameObject, eventData);
        }

        #endregion

        public static UGUIEventListenerContainDrag Get(GameObject go, string clickAudio = "BtnClick")
        {
            var listener = go.GetComponent<UGUIEventListenerContainDrag>();
            if (listener == null) listener = go.AddComponent<UGUIEventListenerContainDrag>();
            listener.mAudioType = clickAudio;
            return listener;
        }
    }
}