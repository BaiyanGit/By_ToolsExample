namespace _3rdBy.ByFramework.Guide.Sample
{
    using UnityEngine;

    /// <summary>
    /// 计时引导
    /// </summary>
    public class GuideTimer : GuideBase
    {
        /// <summary>
        /// 时长
        /// </summary>
        public float gTime;

        public GuideTrigger gTrigger;

        private float _tempTimer;

        public override void Begin()
        {
            if (GTargetObj != null)
            {
                gTrigger.TriggerStay = JudgeTimer;
            }
        }

        private void JudgeTimer(Collider other, float time, int frame)
        {
            if (GTargetObj == null) return;
            string name = GTargetObj.name;
            if (other.name != name) return;
            if (time >= gTime)
            {
                GCallBack?.Invoke(true, "");
            }
        }

        private void JudgeTimer()
        {
            if (GTargetObj != null) return;
            _tempTimer += Time.deltaTime;
            if (_tempTimer >= gTime)
            {
                GCallBack?.Invoke(true, "");
            }
        }

        public override void End()
        {
            _tempTimer = 0;
            gTrigger.TriggerStay = null;
        }

        public override void OnUpdate()
        {
            JudgeTimer();
        }
    }
}