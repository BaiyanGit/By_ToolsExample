using UnityEngine;

public class RollerShutterController : MonoBehaviour
{
    [Header("Slat Settings")] public int slatCount = 30;
    public Vector3 slatSize = new Vector3(1f, 0.05f, 0.3f);
    public float slatMass = 1f;

    [Header("Roll Settings")] public float rollRadius = 0.3f;
    public float rollSpeed = 200f;
    public float damping = 100f;
    public float stiffness = 10000f;

    [Header("Friction")] public PhysicsMaterial groundFrictionMat;

    private ArticulationBody axis;
    private ArticulationBody[] slats;

    void Start()
    {
        BuildRoller();
    }

    #region Build

    void BuildRoller()
    {
        // 卷帘轴
        GameObject axisGO = new GameObject("RollerAxis");
        axisGO.transform.parent        = transform;
        axisGO.transform.localPosition = Vector3.zero;

        axis           = axisGO.AddComponent<ArticulationBody>();
        axis.immovable = true;
        axis.jointType = ArticulationJointType.FixedJoint;

        slats = new ArticulationBody[slatCount];
        ArticulationBody parent = axis;

        for (int i = 0; i < slatCount; i++)
        {
            GameObject slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slat.name                 = $"Slat_{i}";
            slat.transform.parent     = transform;
            slat.transform.localScale = slatSize;

            // 初始位置（卷在轴上）
            float angle = i * (slatSize.z / rollRadius);
            Vector3 localPos = new Vector3(
                0,
                Mathf.Sin(angle) * rollRadius,
                Mathf.Cos(angle) * rollRadius
            );
            slat.transform.localPosition = localPos;

            // 碰撞器 & 摩擦
            var col = slat.GetComponent<BoxCollider>();
            col.material = groundFrictionMat;

            // Articulation
            var body = slat.AddComponent<ArticulationBody>();
            body.mass                 = slatMass;
            body.jointType            = ArticulationJointType.RevoluteJoint;
            body.anchorPosition       = Vector3.back * slatSize.z * 0.5f;
            body.parentAnchorPosition = Vector3.forward * slatSize.z * 0.5f;

            body.twistLock  = ArticulationDofLock.LockedMotion;
            body.swingYLock = ArticulationDofLock.LockedMotion;
            body.swingZLock = ArticulationDofLock.LockedMotion;

            var drive = body.xDrive;
            drive.forceLimit = 1000f;
            drive.stiffness  = stiffness;
            drive.damping    = damping;
            drive.target     = 0;
            body.xDrive      = drive;

            body.transform.SetParent(parent.transform, true);
            slats[i] = body;
            parent   = body;
        }
    }

    #endregion

    #region Control

    public void RollUp()
    {
        // 电机驱动卷起
        for (int i = 0; i < slats.Length; i++)
        {
            var drive = slats[i].xDrive;
            drive.target    = 90f;
            drive.stiffness = stiffness;
            drive.damping   = damping;
            slats[i].xDrive = drive;
        }
    }

    public void Unroll()
    {
        // 解除驱动，靠拉 & 重力展开
        for (int i = 0; i < slats.Length; i++)
        {
            var drive = slats[i].xDrive;
            drive.stiffness = 0;
            drive.damping   = damping * 0.5f;
            drive.target    = 0;
            slats[i].xDrive = drive;
        }
    }

    public void PullAxis(Vector3 force)
    {
        // 外力拉卷帘轴 → 展开更快
        axis.AddForce(force, ForceMode.Force);
    }

    #endregion
}