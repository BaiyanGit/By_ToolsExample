namespace Demos.示例_多用工程车
{
    using UnityEngine;

    [RequireComponent(typeof(ConfigurableJoint))]
    public abstract class JointBase : MonoBehaviour
    {
        [Header("驱动速度"), Range(1, 60)] public float driveVelocity = 30;
        [Header("铰链驱动")] protected JointDrive jointDrive;
        [Header("可配置铰链关节")] protected ConfigurableJoint configurableJoint;

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Awake()
        {
            configurableJoint = GetComponent<ConfigurableJoint>();
            jointDrive.maximumForce = Mathf.Infinity; //最大作用力
        }

        /// <summary>
        /// 铰链驱动
        /// </summary>
        public abstract void JointDrive(float value);
    }
}