// using Unity.VisualScripting;

namespace _3rdBy.MetaFramework.UI
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// 此脚本用于实现UI视觉效果
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Animator))]
    public class UIHierarchy : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private Animator _animator;
        private CancellationTokenSource _playOutCts;

        //是否播放动画，默认为true
        public bool UseAnimation { get; set; } = true;

        private void Awake()
        {
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            _animator = gameObject.GetComponent<Animator>();
        }

        public void PlayOut()
        {
            if (UseAnimation)
            {
                _playOutCts = new CancellationTokenSource();
                UniTask.Void(async () =>
                {
                    _canvasGroup.blocksRaycasts = false;
                    _animator.Play("UI_Panel_Out");
                    await UniTask.Yield();
                    var duration = _animator.GetCurrentAnimatorStateInfo(0).length;
                    await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _playOutCts.Token);
                    transform.SetAsFirstSibling();
                    gameObject.SetActive(false);
                    _playOutCts = null;
                });
            }
            else
            {
                _canvasGroup.blocksRaycasts = false;
                transform.SetAsFirstSibling();
                gameObject.SetActive(false);
            }
        }

        public void PlayIn()
        {
            if (UseAnimation)
            {
                UniTask.Void(async () =>
                {
                    if (_playOutCts != null)
                    {
                        _playOutCts.Cancel();
                        _animator.Play("None");
                        await UniTask.Yield();
                    }

                    gameObject.SetActive(true);
                    _canvasGroup.blocksRaycasts = true;
                    _animator.Play("UI_Panel_In");
                    transform.SetAsLastSibling();
                });
            }
            else
            {
                gameObject.SetActive(true);
                _canvasGroup.blocksRaycasts = true;
                transform.SetAsLastSibling();
            }
        }

        public void PlayRefresh()
        {
            if (UseAnimation)
            {
                UniTask.Void(async () =>
                {
                    _animator.Play("None");
                    await UniTask.Yield();
                    _animator.Play("UI_Panel_Refresh");
                });
            }
        }

        public void SetInteractable(bool value)
        {
            _canvasGroup.interactable = value;
        }
    }
}