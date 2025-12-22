using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ZCustom
{
    /// <summary>
    /// UI引导
    /// </summary>
    public class GuideUI : GuideBase
    {
        /// <summary>
        /// 镂空形状
        /// </summary>
        public UIGuideBase gShape;

        /// <summary>
        /// 箭头
        /// </summary>
        public RectTransform gArrow;

        /// <summary>
        /// 画布
        /// </summary>
        public Canvas gCanvas;

        private Button _curGuideBtn;

        public override void Begin()
        {
            StartCoroutine(WaitLoad());
        }

        private IEnumerator WaitLoad()
        {
            //yield return new WaitUntil(() => (_curGuideObj = GameObject.Find(gPath)) != null);
            yield return null;
            RectTransform target = GTargetObj.GetComponent<RectTransform>();
            if (gShape != null)
            {
                SetGuideMask(target);
            }

            if (gArrow != null)
            {
                SetGuideArrow(target);
            }

            _curGuideBtn = GTargetObj.GetComponent<Button>();
            if (_curGuideBtn)
            {
                _curGuideBtn.onClick.AddListener(EndUI);
            }
            else
            {
                GTargetObj.gameObject.AddComponent<GuideEvents>().SingleClick += EndUI;
            }
        }

        private void SetGuideMask(RectTransform target)
        {
            gShape.SetTarget(target, gCanvas);
            gShape.gameObject.SetActive(true);
        }

        private void SetGuideArrow(RectTransform target)
        {
            gArrow.gameObject.SetActive(true);
            float posX = target.position.x;
            float posY = target.position.y;
            Vector3 offset = Vector3.zero;
            if (posX >= Screen.width / 2)
            {
                if (posY >= Screen.height / 2)
                    offset = new Vector3(-gArrow.rect.width / 2, -gArrow.rect.height / 2, 0);
                else
                    offset = new Vector3(-gArrow.rect.width / 2, gArrow.rect.height / 2, 0);
            }
            else
            {
                if (posY >= 540)
                    offset = new Vector3(gArrow.rect.width / 2, -gArrow.rect.height / 2, 0);
                else
                    offset = new Vector3(gArrow.rect.width / 2, gArrow.rect.height / 2, 0);
            }

            gArrow.transform.position = target.position + offset;
            Vector3 direction = target.position - gArrow.transform.position;
            gArrow.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction.normalized);
        }

        private void EndUI()
        {
            GCallBack?.Invoke(true, "");
        }

        public override void End()
        {
            if (gShape != null) gShape.gameObject.SetActive(false); //关闭形状遮罩
            if (gArrow != null) gArrow.gameObject.SetActive(false); //关闭箭头
            if (_curGuideBtn) _curGuideBtn.onClick.RemoveAllListeners();
            else Destroy(GTargetObj.GetComponent<GuideEvents>());
            _curGuideBtn = null;
        }

        public override void OnUpdate()
        {
        }
    }
}