using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 稳定的物理驱动卷帘（基于 ArticulationBody）
///
/// 目的
/// - 自动化生成门片（不需要手动布置）
/// - 每片使用 ArticulationBody + Revolute joint，并使用 ArticulationDrive 来控制角度，保证比 HingeJoint 更稳定
/// - 绕指定圆柱半径卷起（每片在卷起时沿圆柱堆叠）
/// - 在代码里把重要的物理/求解器参数都设置好，以尽量避免抖动、穿模和“崩飞”
///
/// 使用
/// - 把脚本挂到一个空 GameObject（该对象位置为圆柱轴心）
/// - 可选设置 segmentPrefab（若为空会自动创建 Cube）
/// - 运行时调用 Open() / Close() / Toggle() 或者按 O / C / T 键测试
///
/// 注意与调优
/// - Articulation 的某些 API 随 Unity 版本有细微差异（anchor/parentAnchor / anchorRotation 名称等）。
///   如果你的 Unity 报错，请参照 Unity 官方 ArticulationBody 文档把对应属性名替换为你版本的命名（注释处标注了替换点）。
/// - 如果仍出现穿模，请适当：
///     - 增加 solverIterations / solverVelocityIterations
///     - 增加 drive.stiffness 和 damping
///     - 将邻接片设置 IgnoreCollision（代码里默认忽略非相邻片以减少卡住）
/// - 如果你希望门片可被外力强行推动（例如被玩家撞开），可以增大 forceLimit；想完全锁死则把 forceLimit 设较大并调高 stiffness。
/// </summary>
[DisallowMultipleComponent]
public class RollingShutterArticulation : MonoBehaviour
{
    [Header("片段 / 生成（无需手动创建）")]
    public GameObject segmentPrefab;       // 可选：若为空自动创建 Cube（带 BoxCollider）
    public int segmentCount = 18;
    public float segmentWidth = 0.12f;     // 沿弧长方向的尺寸（每片弧长）
    public float segmentHeight = 1.8f;     // 沿轴向高度
    public float segmentDepth = 0.06f;     // 厚度（径向）
    public float gapBetween = 0.0f;        // 展开时片间间隙

    [Header("卷轴 / 方向")]
    public float cylinderRadius = 0.22f;   // 卷轴半径（内径）
    public Vector3 cylinderAxis = Vector3.forward; // 圆柱轴（本物体局部方向）
    public bool wrapClockwise = true;      // 卷动方向

    [Header("Articulation 驱动 / 稳定性调参")]
    [Tooltip("驱动刚度（越大越硬）")]
    public float driveStiffness = 20000f;
    [Tooltip("驱动阻尼（越大越快衰减震荡）")]
    public float driveDamping = 2000f;
    [Tooltip("驱动最大力矩/力（越大越不容易被外力打断）")]
    public float driveForceLimit = 10000f;
    [Tooltip("期望角速度（度/秒），驱动通过 target + targetVelocity 控制")]
    public float driveSpeedDegPerSec = 120f;

    [Header("Solver 设置（全局）")]
    public int physicsSolverIterations = 12;       // Physics.defaultSolverIterations
    public int physicsSolverVelocityIterations = 8;

    [Header("碰撞设置")]
    public bool enableCollisionsBetweenNeighbors = false; // 邻接片是否发生碰撞（通常设 false）
    public bool addColliders = true;
    public bool collidersAreTriggers = false;

    [Header("动画")]
    public float openAnglePerSegmentDeg = -10f; // 每片卷一圈的增量角，脚本会自动按 segmentWidth/cylinderRadius 计算并可作为参考
    public float openCloseTransitionTime = 1.0f; // 目标角度插值时间（平滑设置驱动 target）

    // 运行时数据
    private List<Transform> segments = new List<Transform>();
    private List<ArticulationBody> bodies = new List<ArticulationBody>();
    private List<float> closedLocalAngleDeg = new List<float>(); // 每片闭合时的局部角（一般 0）
    private List<float> openTargetAngleDeg = new List<float>();  // 每片打开（卷起）时的 drive.target 值（度）
    private Coroutine currentRoutine;

    void Start()
    {
        Build();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) Open();
        if (Input.GetKeyDown(KeyCode.C)) Close();
        if (Input.GetKeyDown(KeyCode.T)) Toggle();
    }

    [ContextMenu("Build")]
    public void Build()
    {
        ClearExisting();

        if (segmentCount <= 0) return;

        // 设置全局物理迭代数（可能影响所有刚体）
        Physics.defaultSolverIterations = Mathf.Max(1, physicsSolverIterations);
        Physics.defaultSolverVelocityIterations = Mathf.Max(1, physicsSolverVelocityIterations);

        // 单片绕圆周角（rad & deg）由弧长 ≈ segmentWidth, r = cylinderRadius
        float alphaRad = Mathf.Max(1e-6f, segmentWidth / cylinderRadius);
        float alphaDeg = alphaRad * Mathf.Rad2Deg;

        // 如果用户没有设置 openAnglePerSegmentDeg, 用计算的 alphaDeg（带方向）
        float segAngleDeg = (openAnglePerSegmentDeg == 0f) ? (wrapClockwise ? -alphaDeg : alphaDeg) : openAnglePerSegmentDeg * (wrapClockwise ? -1f : 1f);

        // 生成 top anchor — 我们把第一个 segment 的 parent 设为一个静态的 ArticulationBody Root（便于关节链稳定）
        GameObject rootAnchor = new GameObject("Shutter_RootAnchor");
        rootAnchor.transform.SetParent(transform, false);
        rootAnchor.transform.localPosition = Vector3.zero;
        rootAnchor.transform.localRotation = Quaternion.identity;
        ArticulationBody rootAb = rootAnchor.AddComponent<ArticulationBody>();
        rootAb.immovable = true; // 作为固定根

        ArticulationBody prevAb = rootAb;
        Transform prevTransform = rootAnchor.transform;

        // 保证 cylinderAxis 为单位向量（本地）
        Vector3 localAxis = cylinderAxis.normalized;

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject seg;
            if (segmentPrefab != null)
            {
                seg = Instantiate(segmentPrefab, transform);
            }
            else
            {
                seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.transform.SetParent(transform, false);
            }

            seg.name = $"Shutter_Segment_{i}";

            // 统一 local scale 约定：
            // local X = 宽度（弧长方向）
            // local Y = 高度（轴向）
            // local Z = 厚度（径向）
            seg.transform.localScale = new Vector3(segmentWidth, segmentHeight, segmentDepth);

            // 设置闭合（放下）时的位置：从 top 向下排
            float yOffset = - (segmentHeight + gapBetween) * i - segmentHeight * 0.5f;
            seg.transform.localPosition = new Vector3(0f, yOffset, 0f);
            seg.transform.localRotation = Quaternion.identity;

            // 添加/配置 Collider
            if (addColliders)
            {
                Collider c = seg.GetComponent<Collider>();
                if (c == null) c = seg.AddComponent<BoxCollider>();
                c.isTrigger = collidersAreTriggers;
            }
            else
            {
                // 移除任何 collider（防止碰撞）
                Collider c = seg.GetComponent<Collider>();
                if (c != null) DestroyImmediate(c);
            }

            // 添加 ArticulationBody
            ArticulationBody ab = seg.GetComponent<ArticulationBody>();
            if (ab == null) ab = seg.AddComponent<ArticulationBody>();

            // 重要：把每个 segment 的 transform 作为 articulation 子体（父亲为 prevAb.transform）
            // 为了 Articulation 正确工作，需要在层级上把 GameObject 作为父子关系：
            seg.transform.SetParent(prevTransform, true);

            // 配置 joint 类型为 Revolute
            ab.jointType = ArticulationJointType.RevoluteJoint;

            // anchor / parentAnchor 配置：
            // 我们希望铰链处在片的“顶边”上（local Y positive half）
            Vector3 localAnchor = new Vector3(0f, segmentHeight * 0.5f, 0f);
            // parentAnchor 在 prev 的局部坐标（要把 world 锚点转换为 parent 的局部）
            Vector3 worldAnchor = seg.transform.TransformPoint(localAnchor);
            Vector3 parentLocalAnchor = prevTransform.InverseTransformPoint(worldAnchor);

            // 注意：不同 Unity 版本对属性命名差异，下面分别尝试设置常见的属性名（如果你的版本不含某些成员，注释/替换即可）
            try
            {
                // 常见 API（Unity 2020+）
                ab.anchorPosition = localAnchor; // anchor 相对于本体
                ab.parentAnchorPosition = parentLocalAnchor; // 对父物体的锚点
            }
            catch
            {
                // 若你的 Unity 版本没有 anchorPosition / parentAnchorPosition，则尝试使用:
                // ab.anchorPosition = localAnchor; (or) ab.anchor = localAnchor;
                // ab.parentAnchorPosition = parentLocalAnchor; (or) ab.connectedAnchor = parentLocalAnchor;
                // 如果仍然报错，请根据你的版本查找对应属性并替换，上面的注释提示可能的替代名。
            }

            // 设置 joint 轴：我们希望旋转轴与本 object 的某个本地方向一致
            // 通过旋转铰链参考系使 xDrive/yDrive/zDrive 中某一轴作为转动轴
            // 这里我们把轴放到本地 X 轴（常见选择），如果需要其他轴，请修改 anchorRotation
            // 若想绕 transform.right 旋转，可把 anchorRotation 设为 identity and use xDrive.
            Quaternion anchorRot = Quaternion.identity;
            try
            {
                // 常见 API
                ab.anchorRotation = anchorRot;
                ab.parentAnchorRotation = Quaternion.identity;
            }
            catch
            {
                // 如果没有这些属性可跳过（可能你的版本使用不同的方式），但务必确保你使用的驱动轴与物体朝向一致。
            }

            // 重要：把关节的旋转轴映射到 xDrive / yDrive / zDrive
            // 这里我们把 Revolute 轴映射到 xDrive（即围绕本局部 X 轴转动）
            // 如果你需要用其他轴，请把相应的 drive 赋值并调整 anchorRotation。
            ArticulationDrive drive = new ArticulationDrive();
            drive.stiffness = driveStiffness;
            drive.damping = driveDamping;
            drive.forceLimit = driveForceLimit;
            drive.target = 0f;
            drive.targetVelocity = 0f;

            // 由于关节的自由度在 x/y/z 中，我们把旋转自由度设置在 xDrive：
            ab.xDrive = drive;

            // 其余自由度锁住（将它们的 limits/lock 标记为 locked）
            ab.yDrive = new ArticulationDrive() { stiffness = 0f, damping = 0f, forceLimit = 0f, target = 0f };
            ab.zDrive = new ArticulationDrive() { stiffness = 0f, damping = 0f, forceLimit = 0f, target = 0f };

            // 物理属性
            ab.mass = Mathf.Max(0.1f, segmentHeight * segmentWidth * segmentDepth * 100f);
            ab.useGravity = true;
            ab.collisionDetectionMode = CollisionDetectionMode.Continuous; // 若你的 Unity 版本 支持 ArticulationBody.collisionDetectionMode
            ab.linearDamping = 0.05f;
            ab.angularDamping = 0.2f;

            // 保存
            segments.Add(seg.transform);
            bodies.Add(ab);

            // 记录闭合角（默认 0）与 open 目标角（累积）
            closedLocalAngleDeg.Add(0f);
            float cumulativeDeg = segAngleDeg * i; // 每片相对 root 的累积角
            openTargetAngleDeg.Add(cumulativeDeg);

            // 下一环的父对象 / Articulation 节点应当是本 segment（以构成链）
            prevAb = ab;
            prevTransform = seg.transform;
        }

        // 碰撞过滤：如果不想邻接片碰撞则忽略邻接碰撞（防止卡住）
        if (!enableCollisionsBetweenNeighbors && addColliders)
        {
            // 仅忽略直接相邻的 collider（相邻片依赖关节约束）
            for (int i = 0; i < segments.Count; i++)
            {
                Collider a = segments[i].GetComponent<Collider>();
                if (a == null) continue;
                if (i - 1 >= 0)
                {
                    Collider b = segments[i - 1].GetComponent<Collider>();
                    if (b != null) Physics.IgnoreCollision(a, b, true);
                }
            }
        }

        // 初始为闭合状态（给所有关节 target = 0）
        ApplyDriveTargetsImmediate(0f);
    }

    /// <summary>
    /// 清理之前生成的东西
    /// </summary>
    [ContextMenu("ClearExisting")]
    public void ClearExisting()
    {
        if (currentRoutine != null) { StopCoroutine(currentRoutine); currentRoutine = null; }

        // 删除所有 children（包含 rootAnchor 与 segments）
        var children = new List<Transform>();
        foreach (Transform t in transform) children.Add(t);
        foreach (var c in children) DestroyImmediate(c.gameObject);

        segments.Clear();
        bodies.Clear();
        closedLocalAngleDeg.Clear();
        openTargetAngleDeg.Clear();
    }

    /// <summary>
    /// 逐步把每个关节的 drive.target 设置为相应角度（0 到 open）
    /// targetRatio in [0,1]
    /// </summary>
    private void ApplyDriveTargetsImmediate(float targetRatio)
    {
        targetRatio = Mathf.Clamp01(targetRatio);

        for (int i = 0; i < bodies.Count; i++)
        {
            ArticulationBody ab = bodies[i];
            float finalDeg = Mathf.Lerp(closedLocalAngleDeg[i], openTargetAngleDeg[i], targetRatio);
            // xDrive 中的 target 表示角度（度）， targetVelocity 表示速度（度/秒）
            ArticulationDrive drive = ab.xDrive;
            drive.target = finalDeg;
            drive.targetVelocity = 0f;
            ab.xDrive = drive;
        }
    }

    /// <summary>
    /// 平滑设置 drive.target（带速度），避免瞬时冲击
    /// </summary>
    private IEnumerator SmoothSetTargets(float targetRatio)
    {
        float startRatio = 0f;
        // 通过读取当前第一个 body 的 drive.target 反推当前 ratio（简单估算）
        if (bodies.Count > 0)
        {
            float curTarget = bodies[0].xDrive.target;
            float a = closedLocalAngleDeg[0];
            float b = openTargetAngleDeg[0];
            float denom = (Mathf.Abs(b - a) > 1e-4f) ? (b - a) : 1f;
            startRatio = Mathf.Clamp01((curTarget - a) / denom);
        }

        float elapsed = 0f;
        float dur = Mathf.Max(0.001f, openCloseTransitionTime);

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float u = elapsed / dur;
            float ratio = Mathf.SmoothStep(startRatio, targetRatio, u);

            // 计算每个关节的目标角与目标角速度
            for (int i = 0; i < bodies.Count; i++)
            {
                ArticulationBody ab = bodies[i];
                float desiredDeg = Mathf.Lerp(closedLocalAngleDeg[i], openTargetAngleDeg[i], ratio);
                // 平稳速度：估计剩余角度 / 时间
                float remainingDeg = (Mathf.Lerp(closedLocalAngleDeg[i], openTargetAngleDeg[i], targetRatio) - desiredDeg);
                float vel = remainingDeg / Mathf.Max(0.0001f, dur - elapsed); // deg/sec estimate

                ArticulationDrive drive = ab.xDrive;
                drive.target = desiredDeg;
                drive.targetVelocity = Mathf.Clamp(vel, -Mathf.Abs(driveSpeedDegPerSec), Mathf.Abs(driveSpeedDegPerSec));
                // ensure stiffness/damping/forceLimit remain set
                drive.stiffness = driveStiffness;
                drive.damping = driveDamping;
                drive.forceLimit = driveForceLimit;
                ab.xDrive = drive;
            }

            yield return null;
        }

        // 最终强制设为目标
        ApplyDriveTargetsImmediate(targetRatio);
        currentRoutine = null;
    }

    public void Open()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(SmoothSetTargets(1f));
    }

    public void Close()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(SmoothSetTargets(0f));
    }

    public void Toggle()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        // 通过第一个 body 的 xDrive.target 判断当前状态
        float cur = (bodies.Count > 0) ? bodies[0].xDrive.target : 0f;
        float mid = (openTargetAngleDeg[0] + closedLocalAngleDeg[0]) * 0.5f;
        float targetRatio = (Mathf.Abs(Mathf.DeltaAngle(cur, closedLocalAngleDeg[0])) < Mathf.Abs(Mathf.DeltaAngle(cur, openTargetAngleDeg[0]))) ? 1f : 0f;

        currentRoutine = StartCoroutine(SmoothSetTargets(targetRatio));
    }

    // 调试辅助：在 Inspector 中可直接让脚本重新计算 open 目标（当半径或片宽改变）
    [ContextMenu("RecomputeOpenTargets")]
    public void RecomputeOpenTargets()
    {
        // recompute based on segmentWidth/cylinderRadius if needed
        if (segments.Count == 0) return;
        float alphaRad = Mathf.Max(1e-6f, segmentWidth / cylinderRadius);
        float alphaDeg = alphaRad * Mathf.Rad2Deg;
        float segAngleDeg = (openAnglePerSegmentDeg == 0f) ? (wrapClockwise ? -alphaDeg : alphaDeg) : openAnglePerSegmentDeg * (wrapClockwise ? -1f : 1f);
        openTargetAngleDeg.Clear();
        for (int i = 0; i < segments.Count; i++) openTargetAngleDeg.Add(segAngleDeg * i);
    }

    // 可被外部调用的强制抛出/收回（瞬时）
    public void ApplyInstantOpen()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        ApplyDriveTargetsImmediate(1f);
    }
    public void ApplyInstantClose()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        ApplyDriveTargetsImmediate(0f);
    }

    // 编辑器可视化（辅助调试）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cylinderRadius);
        Gizmos.color = Color.yellow;
        Vector3 axisWorld = transform.TransformDirection(cylinderAxis.normalized);
        Gizmos.DrawLine(transform.position, transform.position + axisWorld * 0.5f);
    }
}