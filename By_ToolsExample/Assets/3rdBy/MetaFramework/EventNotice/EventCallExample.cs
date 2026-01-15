namespace _3rdBy.MetaFramework.EventNotice
{
    using UnityEngine;

    //使用示例：在启动程序时，注入AEvent<结构体>，在使用时调用Publish进行通知。
    public struct StructExample
    {
        public float testValue;
    }

//挂载到物体上即可
    public class EventCallExample : MonoBehaviour
    {
        private StructExample _structExample;
        private void Start()
        {
            _structExample.testValue = 100;

            //调用通知
            EventManager.Instance.Publish(_structExample);
        }
    }

    public class EventUseExample1 : AEvent<StructExample>
    {
        protected override void Run(StructExample a)
        {
            Debug.Log(a.testValue);
        }
    }

    public class EventUseExample2 : AEvent<StructExample>
    {
        protected override void Run(StructExample a)
        {
            Debug.Log(a.testValue);
        }
    }
}