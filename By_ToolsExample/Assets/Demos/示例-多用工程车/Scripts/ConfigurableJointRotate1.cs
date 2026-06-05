namespace Demos.示例_多用工程车.Scripts
{
    using System;
    using UnityEngine;

    public class ConfigurableJointRotate1 : MonoBehaviour
    {
        private ConfigurableJoint _joint;
        [Header("铰链驱动")] private JointDrive _jointDrive;
        [Header("旋转速度(弧度制)")] public Vector3 targetAngularVelocity = Vector3.forward * 30;


        [Header("弹簧"), SerializeField] private float positionSpring = Mathf.Infinity;
        [Header("阻尼"), SerializeField] private float positionDamper = Mathf.Infinity;
        [Header("最大作用力"), SerializeField] private float maximumForce = Mathf.Infinity;

        private void Awake()
        {
            _joint = GetComponent<ConfigurableJoint>();

            // _jointDrive.positionDamper = positionDamper; //阻尼
            // _jointDrive.positionSpring = positionSpring; //弹簧
            // _jointDrive.maximumForce = maximumForce; //最大作用力
            // _joint.angularYZDrive = _jointDrive;
        }

        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.A))
                SetJointDrive(1);

            else if (Input.GetKey(KeyCode.D))
                SetJointDrive(-1);
            else

                SetJointDrive(0);
        }


        private float _lastValue;

        private void SetJointDrive(float value)
        {
            if (Math.Abs(_lastValue - value) == 0) return;
            _lastValue = value;

            if (value == 0)
            {
                Debug.Log("停止_joint运动");

                _jointDrive.positionSpring   = positionSpring;
                _jointDrive.positionDamper   = 0;
                _joint.targetAngularVelocity = Vector3.zero;
                return;
            }

            var v3 = targetAngularVelocity * value * Time.fixedDeltaTime;
            _joint.targetAngularVelocity = v3;
            _jointDrive.positionSpring   = 0;              //弹簧
            _jointDrive.positionDamper   = positionDamper; //阻尼
            _joint.angularYZDrive        = _jointDrive;
        }
    }
}