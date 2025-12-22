namespace Demos.示例_多用工程车
{
    using System;
    using UnityEngine;

    public class ConfigurableJointRotate : JointBase
    {
        private float _lastInputValue; //上一次输入的值
        private Vector3 V3Velocity => Vector3.one * driveVelocity; //速度

        public bool isRot;

        public Vector3 minAngleLimit = Vector3.zero;
        public Vector3 maxAngleLimit = Vector3.zero;

        protected override void Awake()
        {
            base.Awake();
            configurableJoint.xMotion = ConfigurableJointMotion.Locked;
            configurableJoint.yMotion = ConfigurableJointMotion.Locked;
            configurableJoint.zMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;
            configurableJoint.angularZLimit = new SoftJointLimit
            {
                limit = 60,
                bounciness = 0,
                contactDistance = 0
            };


            jointDrive.positionDamper = 0; //阻尼
            jointDrive.positionSpring = Mathf.Infinity; //弹簧
            configurableJoint.angularYZDrive = jointDrive;
        }

        private void FixedUpdate()
        {
            if (isRot)
                RotateInput();
            else
                PhixedUpdate();
        }

        private void PhixedUpdate()
        {
            if (Input.GetKey(KeyCode.A))
            {
                JointDrive(1);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                JointDrive(-1);
            }
            else
            {
                JointDrive(0);
            }
        }

        private void RotateInput()
        {
            if (Input.GetKey(KeyCode.A))
            {
                SetRotateAngle(1);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                SetRotateAngle(-1);
            }
            else
            {
                SetRotateAngle(0);
            }
        }

        float v = 0;

        private void SetRotateAngle(float value)
        {
            v += Time.fixedDeltaTime * driveVelocity * value;

            if (v > maxAngleLimit.z)
            {
                v = maxAngleLimit.z;
            }
            else if (v < minAngleLimit.z)
            {
                v = minAngleLimit.z;
            }

            Debug.Log("定位" + v);
            var z = new Vector3(0, 0, v);
            configurableJoint.targetRotation = Quaternion.Euler(z);

            jointDrive.positionSpring = 0; //弹簧
            jointDrive.positionDamper = Mathf.Infinity; //阻尼
            configurableJoint.targetAngularVelocity = Vector3.zero; //速度
        }

        public override void JointDrive(float value)
        {
            if (Math.Abs(_lastInputValue - value) == 0) return;
            _lastInputValue = value;

            if (value == 0)
            {
                Debug.Log("定位");
                configurableJoint.targetRotation = Quaternion.Euler(transform.localEulerAngles);
                jointDrive.positionSpring = 0; //弹簧
                jointDrive.positionDamper = Mathf.Infinity; //阻尼
                configurableJoint.targetAngularVelocity = Vector3.zero; //速度
                return;
            }

            jointDrive.positionSpring = 0; //弹簧
            jointDrive.positionDamper = Mathf.Infinity; //阻尼
            configurableJoint.targetAngularVelocity = V3Velocity * (value * Time.fixedDeltaTime); //速度
            configurableJoint.angularYZDrive = jointDrive; //驱动
        }
    }
}