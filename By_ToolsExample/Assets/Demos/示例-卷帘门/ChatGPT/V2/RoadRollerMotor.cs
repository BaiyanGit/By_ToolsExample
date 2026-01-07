using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class RoadRollerMotor : MonoBehaviour
{
    [Header("目标转速 (度/秒)")] public float targetSpeed = 60f;

    [Header("最大扭矩")] public float motorForce = 90000f;

    [Header("是否启用")] public bool enableMotor = true;

    private HingeJoint _hinge;

    private void Awake()
    {
        _hinge = GetComponent<HingeJoint>();
        _hinge.axis = Vector3.up;
    }

    private void FixedUpdate()
    {
        if (_hinge == null) return;

        _hinge.useMotor = enableMotor;

        if (!enableMotor) return;

        JointMotor motor = _hinge.motor;
        motor.targetVelocity = targetSpeed;
        motor.force          = motorForce;
        _hinge.motor         = motor;
    }
}