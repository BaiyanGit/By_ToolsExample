using UnityEngine;

public class PhysicsRoadRoller : MonoBehaviour
{
    [Header("卷轴驱动")] public float motorSpeed = 120f;
    public float motorForce = 500f;

    [Header("板子物理")] [Tooltip("板子之间的物理间距（米）")]
    public float plateSpacing = 0.02f;

    [Header("引用")] public HingeJoint hinge;

    void FixedUpdate()
    {
        if (hinge == null) return;

        JointMotor motor = hinge.motor;
        motor.targetVelocity = motorSpeed;
        motor.force          = motorForce;
        hinge.motor          = motor;
        hinge.useMotor       = true;
    }

#if UNITY_EDITOR
    // Editor 调参时立刻刷新
    void OnValidate()
    {
        ApplySpacing();
    }
#endif

    /// <summary>
    /// 根据 plateSpacing 重新排列板子
    /// </summary>
    public void ApplySpacing()
    {
        // 假设板子是子物体，沿本地 Y 排列
        float offset = 0f;

        foreach (Transform plate in transform)
        {
            plate.localPosition = new Vector3(
                plate.localPosition.x,
                offset,
                plate.localPosition.z
            );

            // 下一块板子的位置 = 当前厚度 + 间距
            var   col       = plate.GetComponent<Collider>();
            float thickness = col ? col.bounds.size.y : 0.05f;

            offset += thickness + plateSpacing;
        }
    }
}