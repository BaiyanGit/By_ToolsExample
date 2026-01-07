namespace Demos.示例_卷帘门.ChatGPT.Scripts
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public enum RollDirection
    {
        [InspectorName("Forward")] Forward = 0,
        [InspectorName("Backward")] Backward = 1,
        [InspectorName("Right")] Right = 2,
        [InspectorName("Left")] Left = 3,
        [InspectorName("Up")] Up = 4,
        [InspectorName("Down")] Down = 5
    }

    [ExecuteAlways]
    public class RoadRollerController : MonoBehaviour
    {
        [Header("卷轴")] public ArticulationBody drum;
        [Header("卷轴速度（用于驱动）")] public float rollVelocity = 25f;
        [Header("卷轴方向")] public RollDirection rollDirection = RollDirection.Forward;

        [Header("第一个板子 (作为缠绕起点参考)")] public Transform firstPlate;
        [Header("视觉板子")] public List<Transform> visualPlates = new();

        [Header("板子间距 (与 Editor 中 plateLength 对应)")]
        public float plateLength = 0.6f;

        [Header("卷轴初始半径")] public float rollRadius = 0.9f;
        [Header("板子厚度（每缠一层增加的半径）")] public float plateThickness = 0.15f;

        [Range(0f, 1f)] public float deploy = 0f;

        // 运行时稳定性参数（可调）
        [Header("释放迟滞距离 (m)")] public float releaseMargin = 0.25f;
        [Header("释放速度映射因子")] public float releaseVelocityFactor = 1.0f;
        [Header("释放速度上限 (m/s)")] public float maxInitialVelocity = 3.0f;
        [Header("释放时额外下坠分量 (m/s)")] public float downwardBoost = 1.2f;

        [Header("运行时：是否提升物理求解迭代（提高稳定性）")] public bool increaseSolverIterations = true;

        [Header("运行时：设置的求解迭代（仅在 increaseSolverIterations 为 true 时应用）")]
        public int solverIterations = 12;

        [Header("[Debug] 外层半径 (effectiveRadius)")]
        public float effectiveRadius = 0;

        [Header("[Debug] 缠绕圈数 (含部分圈)")] public float wrapCount = 0;
        [Header("[Debug] 当前被缠绕长度 (m)")] public float debugWrappedLength = 0f;

        // 内部状态
        private float _prevWrappedLength = 0f;
        private bool[] _wasKinematic = null;
        private Rigidbody[] _plateRbCache = null;

        private int _oldSolverIterations = -1;
        private int _oldVelocityIterations = -1;

        // 缓存 drum 的 collider（用于短暂忽略碰撞）
        private Collider _drumCollider = null;

        private void OnEnable()
        {
            if (increaseSolverIterations)
            {
                _oldSolverIterations                    = Physics.defaultSolverIterations;
                _oldVelocityIterations                  = Physics.defaultSolverVelocityIterations;
                Physics.defaultSolverIterations         = Mathf.Max(Physics.defaultSolverIterations, solverIterations);
                Physics.defaultSolverVelocityIterations = Mathf.Max(Physics.defaultSolverVelocityIterations, solverIterations / 2);
            }

            CachePlateRigidbodies();
            // 初始化 prevWrappedLength 为当前值，避免第一次大跳
            _prevWrappedLength = GetWrappedLengthFromDeploy();
            debugWrappedLength = _prevWrappedLength;

            if (drum != null)
                _drumCollider = drum.GetComponent<Collider>();
        }

        private void OnDisable()
        {
            if (increaseSolverIterations && _oldSolverIterations >= 0)
            {
                Physics.defaultSolverIterations         = _oldSolverIterations;
                Physics.defaultSolverVelocityIterations = _oldVelocityIterations;
            }
        }

        private void CachePlateRigidbodies()
        {
            if (visualPlates == null) return;
            int n = visualPlates.Count;
            _plateRbCache = new Rigidbody[n];
            _wasKinematic = new bool[n];
            for (int i = 0; i < n; i++)
            {
                var tf = visualPlates[i];
                if (tf != null)
                {
                    _plateRbCache[i] = tf.GetComponent<Rigidbody>();
                    _wasKinematic[i] = _plateRbCache[i] != null ? _plateRbCache[i].isKinematic : true;
                }
                else
                {
                    _plateRbCache[i] = null;
                    _wasKinematic[i] = true;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying) return;
            // DriveDrum();
            UpdateVisualPlates();
        }

        private void Update()
        {
            if (Application.isPlaying) return;
            UpdateVisualPlates(); // 编辑模式预览
        }

        private void DriveDrum()
        {
            if (drum == null) return;
            var drive = drum.xDrive;
            drive.targetVelocity = Mathf.Lerp(rollVelocity, -rollVelocity, deploy);
            drum.xDrive          = drive;
        }

        private float GetWrappedLengthFromDeploy()
        {
            if (visualPlates == null) return 0f;
            float totalLength = visualPlates.Count * plateLength;
            return totalLength * (1f - deploy);
        }

        private void UpdateVisualPlates()
        {
            if (drum == null || visualPlates == null || visualPlates.Count == 0) return;

            // 缓存刚体数组
            if (_plateRbCache == null || _plateRbCache.Length != visualPlates.Count) CachePlateRigidbodies();

            float       thickness = Mathf.Max(1e-6f, plateThickness);
            const float eps       = 1e-5f;

            var drumTf  = drum.transform;
            var drumPos = drumTf.position;
            var axis    = GetFollowDirection((RollDirection)rollAxis, drumTf);
            axis = axis.normalized;
            var defaultRadial = GetFollowDirection((RollDirection)rollDefaultRadial, drumTf).normalized;
            var groundDir     = GetFollowDirection((RollDirection)rollGroundDir, drumTf).normalized;

            // radialRef：使用 firstPlate 作为相位基准（投影到轴平面）
            Vector3 radialRef;
            if (firstPlate != null)
            {
                var v = firstPlate.position - drumPos;
                v         = v - Vector3.Dot(v, axis) * axis;
                radialRef = v.sqrMagnitude < 1e-6f ? defaultRadial : v.normalized;
            }
            else radialRef = defaultRadial;

            float totalLength   = visualPlates.Count * plateLength;
            float wrappedLength = totalLength * (1f - deploy);
            debugWrappedLength = wrappedLength;

            // 计算外层半径 & wrapCount（逐层消耗）
            float remaining            = wrappedLength;
            float rrIter               = rollRadius;
            int   fullLayers           = 0;
            float partialLayerFraction = 0f;
            int   safety               = 0;
            while (remaining > eps && safety++ < 2000)
            {
                float circ = 2f * Mathf.PI * rrIter;
                if (remaining >= circ - eps)
                {
                    remaining -= circ;
                    rrIter    += thickness;
                    fullLayers++;
                }
                else
                {
                    partialLayerFraction = remaining / Mathf.Max(eps, circ);
                    remaining            = 0f;
                    break;
                }
            }

            effectiveRadius = rrIter;
            wrapCount       = fullLayers + partialLayerFraction;

            // contactPoint 用于铺地段的起点
            var contactPoint = drumPos + radialRef * effectiveRadius;

            // 计算本次 wrappedLength 的变化率，用作释放时的线速度参考（m / s）
            float deltaWrapped = (wrappedLength - _prevWrappedLength) / Mathf.Max(Time.fixedDeltaTime, 1e-6f);
            _prevWrappedLength = wrappedLength;

            // 限制 deltaWrapped 带来的突变： clamp 与平滑
            float estimatedLinearSpeed = Mathf.Clamp(Mathf.Abs(deltaWrapped) * releaseVelocityFactor, 0f, maxInitialVelocity);

            float rollSign = GetRollSign();

            for (int i = 0; i < visualPlates.Count; i++)
            {
                var plateTf = visualPlates[i];
                if (plateTf == null) continue;

                Rigidbody rb = _plateRbCache != null && i < _plateRbCache.Length ? _plateRbCache[i] : plateTf.GetComponent<Rigidbody>();

                float plateDist = i * plateLength;
                float s         = wrappedLength - plateDist; // >0 => 在卷轴上； <=0 => 铺地/自由

                // Hysteresis：在 releaseMargin 区域引入迟滞，避免频繁切换
                bool shouldBeKinematic = s > releaseMargin;

                if (!Application.isPlaying)
                {
                    // 编辑模式：仅设置 transform（预览）
                    PlacePlateEditor(plateTf, s, axis, radialRef, drumPos, thickness, contactPoint);
                }
                else
                {
                    if (shouldBeKinematic)
                    {
                        // 缠绕段：运动学精确对齐
                        ComputeLayerAndRem(s, rollRadius, thickness, out var rrLayer, out var rem);

                        float angleRad = rem == 0f ? 0f : rem / Mathf.Max(1e-6f, rrLayer);
                        float angleDeg = angleRad * Mathf.Rad2Deg * rollSign;
                        var   radial   = Quaternion.AngleAxis(angleDeg, axis) * radialRef;
                        var   pos      = drumPos + radial * rrLayer;
                        var   tangent  = Vector3.Cross(axis, radial).normalized * rollSign;
                        var   rot      = Quaternion.LookRotation(tangent, radial);

                        if (rb != null)
                        {
                            if (!rb.isKinematic)
                            {
                                // 从动态切回运动学时，清零速度避免残留
                                rb.linearVelocity        = Vector3.zero;
                                rb.angularVelocity = Vector3.zero;
                            }

                            rb.isKinematic = true;
                        }

                        // 运动学：直接设置 transform（保持 kinematic）
                        plateTf.position = pos;
                        plateTf.rotation = rot;
                        _wasKinematic[i] = true;
                    }
                    else
                    {
                        // 铺地 / 自由段：由物理驱动，但给刚释放的刚体一个合理初速度避免瞬移/崩飞
                        float flatDist  = plateDist - wrappedLength;
                        var   targetPos = contactPoint + groundDir * flatDist;
                        var   targetRot = Quaternion.LookRotation(groundDir, Vector3.up);

                        if (rb != null)
                        {
                            if (rb.isKinematic)
                            {
                                // 刚被释放（从运动学到动态）
                                rb.isKinematic = false;

                                // 初始速度：切线速度 + 向下分量，使用 estimatedLinearSpeed 但不直接把物体瞬移到 targetPos
                                Vector3 currentToDrum = plateTf.position - drumPos;
                                Vector3 curRadial     = currentToDrum - Vector3.Dot(currentToDrum, axis) * axis;
                                float   curR          = curRadial.magnitude;
                                if (curR < 1e-5f) curRadial = radialRef;
                                else curRadial              = curRadial / curR;

                                Vector3 tangent = Vector3.Cross(axis, curRadial).normalized * rollSign;

                                Vector3 initVel = tangent * estimatedLinearSpeed + Vector3.down * downwardBoost;

                                if (initVel.magnitude > maxInitialVelocity)
                                    initVel = initVel.normalized * maxInitialVelocity;

                                // 平滑移动到接近目标位置（避免穿透）
                                Vector3 smoothPos = Vector3.Lerp(rb.position, targetPos, 0.25f);

                                // 使用物理友好的 MovePosition/MoveRotation（在 FixedUpdate，也可用 rb.position 但 Move 更好）
                                rb.MovePosition(smoothPos);
                                rb.MoveRotation(targetRot);

                                // 先短暂忽略与卷轴的碰撞，避免瞬间穿透产生巨大冲量
                                if (_drumCollider != null)
                                {
                                    var plateCol = rb.GetComponent<Collider>();
                                    if (plateCol != null)
                                        StartCoroutine(TemporarilyIgnoreCollision(plateCol, _drumCollider, 0.06f));
                                }

                                // 清零角速度然后设初速度
                                rb.angularVelocity = Vector3.zero;
                                rb.linearVelocity        = initVel;
                            }
                            else
                            {
                                // 已经是动态刚体：保持物理仿真（必要时可进行小幅平滑修正）
                            }
                        }
                        else
                        {
                            // 没有刚体则回退到直接设置 transform
                            plateTf.position = targetPos;
                            plateTf.rotation = targetRot;
                        }

                        _wasKinematic[i] = false;
                    }
                }
            }
        }

        // 在释放时短暂忽略两个 collider 之间的碰撞，避免穿透瞬间产生巨大冲量
        private IEnumerator TemporarilyIgnoreCollision(Collider a, Collider b, float duration)
        {
            if (a == null || b == null) yield break;
            Physics.IgnoreCollision(a, b, true);
            yield return new WaitForSeconds(duration);
            if (a != null && b != null) Physics.IgnoreCollision(a, b, false);
        }

        private void PlacePlateEditor(Transform plateTf, float s, Vector3 axis, Vector3 radialRef, Vector3 drumPos, float thickness, Vector3 contactPoint)
        {
            const float eps = 1e-5f;
            if (s > 0f)
            {
                ComputeLayerAndRem(s, rollRadius, thickness, out var rr, out var rem);

                float angleRad = rem == 0f ? 0f : rem / Mathf.Max(eps, rr);
                float angleDeg = angleRad * Mathf.Rad2Deg;
                var   radial   = Quaternion.AngleAxis(angleDeg, axis) * radialRef;
                var   pos      = drumPos + radial * rr;
                var   tangent  = Vector3.Cross(axis, radial).normalized;
                plateTf.position = pos;
                plateTf.rotation = Quaternion.LookRotation(tangent, radial);
            }
            else
            {
                float flatDist = -s; // plateDist - wrappedLength
                plateTf.position = contactPoint + GetFollowDirection((RollDirection)platePosDir, drum.transform).normalized * flatDist;
                plateTf.rotation = Quaternion.LookRotation(GetFollowDirection((RollDirection)plateRotDir, drum.transform), Vector3.up);
            }
        }

        // 逐层消耗得到当前层半径 rr 以及在该层上的弧长 rem
        private void ComputeLayerAndRem(float s, float baseRadius, float thickness, out float rr, out float rem)
        {
            const float eps = 1e-6f;
            rr  = baseRadius;
            rem = s;
            int layer       = 0;
            int localSafety = 0;
            while (localSafety++ < 2000)
            {
                float circ = 2f * Mathf.PI * rr;
                if (rem > circ + eps)
                {
                    rem -= circ;
                    rr  += thickness;
                    layer++;
                }
                else
                {
                    if (rem <= eps)
                    {
                        if (layer > 0)
                        {
                            rr  -= thickness;
                            rem =  2f * Mathf.PI * rr;
                        }
                        else rem = 0f;
                    }
                    else if (rem >= circ - eps)
                    {
                        rem = circ;
                    }

                    break;
                }
            }
        }

        public int rollAxis = 4;
        public int rollDefaultRadial = 0;
        public int rollGroundDir = 2;
        public int platePosDir = 3;
        public int plateRotDir = 3;

        private static Vector3 GetFollowDirection(RollDirection direction, Transform tf)
        {
            return direction switch
            {
                RollDirection.Forward  => tf.forward,
                RollDirection.Backward => -tf.forward,
                RollDirection.Right    => tf.right,
                RollDirection.Left     => -tf.right,
                RollDirection.Up       => tf.up,
                RollDirection.Down     => -tf.up,
                _                      => tf.forward
            };
        }

        private float GetRollSign()
        {
            return rollDirection switch
            {
                RollDirection.Forward  => 1f,
                RollDirection.Backward => -1f,
                RollDirection.Right    => 1f,
                RollDirection.Left     => -1f,
                _                      => 1f
            };
        }
    }
}