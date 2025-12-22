using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ZCustom
{
    /// <summary>
    /// 鼠标按键引导
    /// </summary>
    public class GuideMDown : GuideBase
    {
        /// <summary>
        /// 鼠标按键
        /// </summary>
        public int gKey;

        /// <summary>
        /// 长按时长
        /// </summary>
        public float gTime;

        private float _tempTimer = 0; //临时计时数据
        private RaycastHit _hit; //
        private Ray _ray; //

        public override void Begin()
        {

        }

        public override void End()
        {
            _tempTimer = 0;
        }

        public override void OnUpdate()
        {
            if (GTargetObj != null)
            {
                if (Input.GetMouseButton(gKey))
                {
                    _ray = GuideManager.Instance.controlCam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(_ray, out _hit))
                    {
                        if (_hit.transform.name == GTargetObj.name)
                        {
                            Debug.Log(_tempTimer);
                            _tempTimer += Time.deltaTime;
                            if (_tempTimer >= gTime)
                            {
                                GCallBack?.Invoke(true, "");
                            }
                        }
                    }
                }
            }
            else
            {
                if (Input.GetMouseButton(gKey))
                {
                    _tempTimer += Time.deltaTime;
                    if (_tempTimer > gTime)
                    {
                        GCallBack?.Invoke(true, "");
                    }
                }
            }
        }
    }
}
