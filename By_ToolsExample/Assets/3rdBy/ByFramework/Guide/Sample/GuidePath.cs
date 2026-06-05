namespace _3rdBy.ByFramework.Guide.Sample
{
    using UnityEngine;

    /// <summary>
    /// 路径引导
    /// </summary>
    public class GuidePath : GuideBase
    {
        public GuideTrigger gTrigger;

        public GuideNavLine gPathLine;

        public override void Begin()
        {
            gTrigger.TriggerEnter = EndPathGuide; //使用触发检测终点
            if (GTargetObj != null)
            {
                if (!GTargetObj.activeInHierarchy)
                {
                    GTargetObj.gameObject.SetActive(true);
                }

                GTargetObj.GetComponent<MeshRenderer>().enabled = true;
                gPathLine.ShowLine(gTrigger.gameObject, GTargetObj);
            }
        }

        private void EndPathGuide(Collider other)
        {
            if (GTargetObj == null) return;
            string name = GTargetObj.name;
            if (other.name == name)
            {
                gPathLine.HideLine();
                gTrigger.TriggerEnter = null;
                GCallBack?.Invoke(true, "");
            }
        }

        public override void End()
        {

        }

        public override void OnUpdate()
        {

        }
    }
}