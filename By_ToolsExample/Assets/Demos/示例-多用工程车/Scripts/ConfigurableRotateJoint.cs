namespace Demos.示例_多用工程车.Scripts
{
    using UnityEngine;

    public enum Axis
    {
        axisX,
        axisY,
        axisZ
    }

    public enum AxisLocal
    {
        axisY,
        axisZ
    }

    /// <summary>
    /// 可配置旋转铰链
    /// </summary>
    [RequireComponent(typeof(ConfigurableJoint))]
    public class ConfigurableRotateJoint : MonoBehaviour
    {
        [Header("可配置铰链关节")] private ConfigurableJoint _joint;
        [Header("弹簧")] public float positionSpring = 1e+29f;
        [Header("阻尼")] public float positionDamper = 1e+29f;
        [Header("最大作用力")] public float maximumForce = 1e+29f;
        [Header("旋转速度(弧度制)")] public Vector3 targetAngularVelocity;
        [Header("铰链驱动")] private JointDrive _jointDrive;
        [Header("需要旋转的轴对应铰链设置的可以活动的部分")] public Axis axis;
        [Header("旋转产生坐标差的是哪个轴(禁止使用x轴)")] public AxisLocal axisLocal;
        [Header("需要取反")] public bool needNegate;
        [Header("初始值")] private Vector3 _initialRot;

        private void Awake()
        {
            _joint = transform.GetComponent<ConfigurableJoint>();

            _joint.targetRotation = Quaternion.Euler(Vector3.zero);

            _initialRot = transform.localEulerAngles;

            _jointDrive.positionSpring = positionSpring;

            _jointDrive.positionDamper = 0;

            _jointDrive.maximumForce = maximumForce;

            switch (axis)
            {
                case Axis.axisX:
                {
                    _joint.angularXDrive = _jointDrive;
                }
                    break;
                case Axis.axisY:
                {
                    _joint.angularYZDrive = _jointDrive;
                }
                    break;
                case Axis.axisZ:
                {
                    _joint.angularYZDrive = _jointDrive;
                }
                    break;
            }

            DriveMechanism(0);
        }

        private float _lastValue = -1;

        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.A))
                DriveMechanism(1);

            if (Input.GetKey(KeyCode.D))
                DriveMechanism(-1);
            else
            {
                DriveMechanism(0);
            }
        }

        public void DriveMechanism(float value)
        {
            if (_lastValue != value)
            {
                Debug.Log("copy value");
                _lastValue = value;
            }
            else
            {
                Debug.Log("return");
                return;
            }

            if (value == 0)
            {
                Debug.Log(0);
                float valueRot = 0;

                switch (axisLocal)
                {
                    case AxisLocal.axisY:
                    {
                        valueRot = _initialRot.y - transform.localEulerAngles.y;

                        if (valueRot < 0)
                        {
                            valueRot += 360;
                        }
                    }
                        break;
                    case AxisLocal.axisZ:
                    {
                        valueRot = _initialRot.z - transform.localEulerAngles.z;

                        if (valueRot < 0)
                        {
                            valueRot += 360;
                        }
                    }
                        break;
                }

                if (needNegate)
                {
                    valueRot = -valueRot;
                }

                switch (axis)
                {
                    case Axis.axisX:
                    {
                        _joint.targetRotation = Quaternion.Euler(new Vector3(valueRot, 0, 0));
                    }
                        break;
                    case Axis.axisY:
                    {
                        _joint.targetRotation = Quaternion.Euler(new Vector3(0, valueRot, 0));
                    }
                        break;
                    case Axis.axisZ:
                    {
                        _joint.targetRotation = Quaternion.Euler(new Vector3(0, 0, valueRot));
                    }
                        break;
                }

                _jointDrive.positionSpring = positionSpring;

                _jointDrive.positionDamper = 0;

                _joint.targetAngularVelocity = Vector3.zero;
            }
            else
            {
                _joint.targetAngularVelocity = targetAngularVelocity * value * Time.fixedDeltaTime;

                _jointDrive.positionSpring = 0;

                _jointDrive.positionDamper = positionDamper;
            }

            switch (axis)
            {
                case Axis.axisX:
                {
                    _joint.angularXDrive = _jointDrive;
                }
                    break;
                case Axis.axisY:
                {
                    _joint.angularYZDrive = _jointDrive;
                }
                    break;
                case Axis.axisZ:
                {
                    _joint.angularYZDrive = _jointDrive;
                }
                    break;
            }
        }
    }
}