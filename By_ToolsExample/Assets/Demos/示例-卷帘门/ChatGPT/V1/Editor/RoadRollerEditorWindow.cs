namespace Demos.示例_卷帘门.ChatGPT.Editor
{
    using System.Collections.Generic;
    using _3rdBy.ByTools.Extension;
    using _3rdBy.MetaFramework.Extension.ExtendComponent;
    using Scripts;
    using UnityEditor;
    using UnityEngine;

    public class RoadRollerEditorWindow : EditorWindow
    {
        private int _plateCount = 20;
        private float _plateLength = 0.6f;
        private float _plateWidth = 2.5f;
        private float _plateThickness = 0.15f;
        private float _rollRadius = 0.9f;

        // Joint tunables exposed in the EditorWindow
        private float _jointPositionSpring = 200f;
        private float _jointPositionDamper = 40f;
        private float _jointMaxForce = 10000f;

        private float _slerpPositionSpring = 150f;
        private float _slerpPositionDamper = 30f;
        private float _slerpMaxForce = 8000f;

        private float _angularLimit = 60f;
        private float _lowAngularXLimit = -60f;
        private float _highAngularXLimit = 60f;

        private bool _enableProjection = true;
        private float _projectionDistance = 0.1f;
        private float _projectionAngle = 10f;

        private int _ignoreNeighborRange = 1;

        private GameObject _targetRoot;

        private const RollDirection EnumType = RollDirection.Forward;

        [MenuItem("ByTools/压路机制造型工具")]
        private static void Open()
        {
            GetWindow<RoadRollerEditorWindow>("压路机");
        }

        private Vector2 _scrollViewPos = Vector2.zero;
        private bool _showJointParams = false;
        private bool _showRollParms = false;

        private void OnGUI()
        {
            GUILayout.Label("Road Roller Builder", EditorStyles.boldLabel);

            _targetRoot    = (GameObject)EditorGUILayout.ObjectField("目标对象下生成板子", _targetRoot, typeof(GameObject), true);
            _scrollViewPos = GUILayout.BeginScrollView(_scrollViewPos);

            GUILayout.Label("板参数", EditorStyles.boldLabel);
            _plateCount     = EditorGUILayout.IntSlider("板子数量", _plateCount, 5, 120);
            _plateLength    = EditorGUILayout.FloatField("板子长度", _plateLength);
            _plateWidth     = EditorGUILayout.FloatField("板子宽度", _plateWidth);
            _plateThickness = EditorGUILayout.FloatField("板子厚度", _plateThickness);
            _rollRadius     = EditorGUILayout.FloatField("卷帘半径", _rollRadius);

            // 折叠
            _showJointParams = EditorGUILayout.Foldout(_showJointParams, "关节参数设置");
            if (_showJointParams)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(8);
                GUILayout.Label("关节参数 (ConfigurableJoint)", EditorStyles.boldLabel);

                _jointPositionSpring = EditorGUILayout.FloatField("angularX positionSpring", _jointPositionSpring);
                _jointPositionDamper = EditorGUILayout.FloatField("angularX positionDamper", _jointPositionDamper);
                _jointMaxForce       = EditorGUILayout.FloatField("angularX maximumForce", _jointMaxForce);

                GUILayout.Space(4);
                _slerpPositionSpring = EditorGUILayout.FloatField("slerp positionSpring", _slerpPositionSpring);
                _slerpPositionDamper = EditorGUILayout.FloatField("slerp positionDamper", _slerpPositionDamper);
                _slerpMaxForce       = EditorGUILayout.FloatField("slerp maximumForce", _slerpMaxForce);

                GUILayout.Space(4);
                GUILayout.Label("角度限制 (degrees)", EditorStyles.label);
                _angularLimit      = EditorGUILayout.FloatField("angularY / Z limit", _angularLimit);
                _lowAngularXLimit  = EditorGUILayout.FloatField("lowAngularXLimit", _lowAngularXLimit);
                _highAngularXLimit = EditorGUILayout.FloatField("highAngularXLimit", _highAngularXLimit);

                GUILayout.Space(4);
                _enableProjection = EditorGUILayout.Toggle("Enable joint projection", _enableProjection);
                if (_enableProjection)
                {
                    _projectionDistance = EditorGUILayout.FloatField("projectionDistance", _projectionDistance);
                    _projectionAngle    = EditorGUILayout.FloatField("projectionAngle", _projectionAngle);
                }

                GUILayout.Space(6);
                GUILayout.Label("碰撞 / 构建选项", EditorStyles.boldLabel);
                _ignoreNeighborRange = EditorGUILayout.IntSlider("忽略相邻碰撞范围 (±N)", _ignoreNeighborRange, 0, 5);

                GUILayout.Space(10);
                EditorGUI.indentLevel--;
            }


            if (_targetRoot != null)
            {
                var ctrl = _targetRoot.GetComponent<RoadRollerController>();
                if (ctrl != null)
                {
                    _showRollParms = EditorGUILayout.Foldout(_showRollParms, "卷轴和板子参数设置");
                    if (_showRollParms)
                    {
                        EditorGUI.indentLevel++;
                        {
                            ctrl.deploy = EditorGUILayout.Slider("deploy 0：全卷 -1：全铺", ctrl.deploy, 0f, 1f);

                            ctrl.rollAxis          = EditorGUILayout.Popup("卷轴轴向", ctrl.rollAxis, EnumType.GetInspectorNameArray());
                            ctrl.rollDefaultRadial = EditorGUILayout.Popup("卷轴默认径向", ctrl.rollDefaultRadial, EnumType.GetInspectorNameArray());
                            ctrl.rollGroundDir     = EditorGUILayout.Popup("卷轴地面方向", ctrl.rollGroundDir, EnumType.GetInspectorNameArray());
                            ctrl.platePosDir       = EditorGUILayout.Popup("板子默认径向", ctrl.platePosDir, EnumType.GetInspectorNameArray());
                            ctrl.plateRotDir       = EditorGUILayout.Popup("板子地面方向", ctrl.plateRotDir, EnumType.GetInspectorNameArray());

                            EditorUtility.SetDirty(ctrl);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }

            // 快速构建与清除场景对象
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("构建滚轮"))
            {
                Build();
            }

            if (GUILayout.Button("清除滚轮"))
            {
                if (_targetRoot != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(_targetRoot, "Clear Road Roller");
                    foreach (Transform child in _targetRoot.transform)
                        DestroyImmediate(child.gameObject);
                }
            }

            GUILayout.EndHorizontal();


            GUILayout.EndScrollView();
        }

        private void Build()
        {
            if (_targetRoot == null)
            {
                _targetRoot = new GameObject("RoadRollerRoot");
            }

            Undo.RegisterFullObjectHierarchyUndo(_targetRoot, "Build Road Roller");

            // 清除之前创建的对象
            foreach (Transform child in _targetRoot.transform)
                DestroyImmediate(child.gameObject);

            // Drum
            var drum = RoadRollSupport.FindGameObjectInActiveSceneByName("卷轴");
            drum      ??= GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            drum.name =   "卷轴";
            drum.transform.SetParent(_targetRoot.transform);
            drum.transform.rotation   = Quaternion.Euler(0, 0, 90);
            drum.transform.localScale = new Vector3(_rollRadius * 2, _plateWidth * 0.5f, _rollRadius * 2);

            var drumAb = drum.AddComponent<ArticulationBody>();
            drumAb.jointType            = ArticulationJointType.RevoluteJoint;
            drumAb.anchorRotation       = Quaternion.Euler(0, 0, 90);
            drumAb.parentAnchorRotation = Quaternion.identity;
            drumAb.mass                 = 300;
            drumAb.angularDamping       = 25;
            drumAb.linearDamping        = 5;
            drumAb.solverIterations     = 32;

            // 第一块板：作为运动学锚点（不使用 ArticulationBody，因为接下来的链条使用 Rigidbody+Joint）
            var firstPlate = RoadRollSupport.FindGameObjectInActiveSceneByName("第一块板");
            firstPlate                    ??= CreatePlate("第一块板", _targetRoot.transform);
            firstPlate.transform.position =   drum.transform.position + Vector3.forward * _rollRadius;
            var rbFirst                  = firstPlate.GetComponent<Rigidbody>();
            if (rbFirst == null) rbFirst = firstPlate.AddComponent<Rigidbody>();
            rbFirst.isKinematic            = true;
            rbFirst.mass                   = 40f;
            rbFirst.interpolation          = RigidbodyInterpolation.Interpolate;
            rbFirst.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // 视觉板（同时作为物理链）
            var visualRoot = RoadRollSupport.FindGameObjectInActiveSceneByName("视觉板架");
            visualRoot ??= new GameObject("视觉板架");
            visualRoot.transform.SetParent(_targetRoot.transform);
            var plates = new List<Transform>();
            var prevRb = rbFirst;

            for (int i = 0; i < _plateCount; i++)
            {
                var p = CreatePlate($"Plate_{i:00}", visualRoot.transform);
                plates.Add(p.transform);

                var index = i % RoadRollSupport.colors.Length;

                // 使用材质（优先使用用户指定）
                var mat  = new Material(Shader.Find("Standard")) { color = RoadRollSupport.colors[index] };
                var rend = p.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = mat;

                // 物理刚体
                var rb             = p.GetComponent<Rigidbody>();
                if (rb == null) rb = p.AddComponent<Rigidbody>();

                rb.mass                   = Mathf.Clamp(10f, 1f, 200f);
                rb.interpolation          = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // ConfigurableJoint 连接到 prevRb（firstPlate 的 rb 或上一个 plate 的 rb）
                var joint                = p.GetComponent<ConfigurableJoint>();
                if (joint == null) joint = p.AddComponent<ConfigurableJoint>();
                joint.connectedBody = prevRb;

                // 让 Unity 自动配置 connectedAnchor，减少坐标系错误导致的巨大约束力
                joint.autoConfigureConnectedAnchor = true;

                // anchor 放到板子后端（本地），让 Unity 帮我们对齐 connectedAnchor
                joint.anchor = new Vector3(0, 0, -_plateLength * 0.5f);

                // 锁定线性自由度
                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;

                // 角度允许一定摆动，但给出阻尼与弹簧以防抖动
                joint.angularXMotion = ConfigurableJointMotion.Limited;
                joint.angularYMotion = ConfigurableJointMotion.Limited;
                joint.angularZMotion = ConfigurableJointMotion.Limited;

                var lim = new SoftJointLimit { limit = _angularLimit };
                joint.lowAngularXLimit  = new SoftJointLimit { limit = _lowAngularXLimit };
                joint.highAngularXLimit = new SoftJointLimit { limit = _highAngularXLimit };
                joint.angularYLimit     = lim;
                joint.angularZLimit     = lim;

                // 阻尼弹簧（使用编辑器调节值）
                var xDrive = new JointDrive { positionSpring = _jointPositionSpring, positionDamper = _jointPositionDamper, maximumForce = _jointMaxForce };
                joint.angularXDrive = xDrive;
                joint.slerpDrive    = new JointDrive { positionSpring = _slerpPositionSpring, positionDamper = _slerpPositionDamper, maximumForce = _slerpMaxForce };

                joint.breakForce  = Mathf.Infinity;
                joint.breakTorque = Mathf.Infinity;

                if (_enableProjection)
                {
                    joint.projectionMode     = JointProjectionMode.PositionAndRotation;
                    joint.projectionDistance = _projectionDistance;
                    joint.projectionAngle    = _projectionAngle;
                }
                else
                {
                    joint.projectionMode = JointProjectionMode.None;
                }

                prevRb = rb;
            }

            // 忽略相邻若干片碰撞以减少自相撞抖动（可调整范围）
            var colliders = visualRoot.GetComponentsInChildren<Collider>();
            for (int a = 0; a < colliders.Length; a++)
            {
                for (int b = 0; b < colliders.Length; b++)
                {
                    if (a == b) continue;
                    if (Mathf.Abs(a - b) <= _ignoreNeighborRange)
                    {
                        Physics.IgnoreCollision(colliders[a], colliders[b], true);
                    }
                    else
                    {
                        // ensure other collisions are enabled
                        Physics.IgnoreCollision(colliders[a], colliders[b], false);
                    }
                }
            }

            // Controller
            var ctrl = _targetRoot.GetOrAddComponent<RoadRollerController>();
            ctrl.drum         = drumAb;
            ctrl.firstPlate   = firstPlate.transform;
            ctrl.visualPlates = plates;
            ctrl.plateLength  = _plateLength;
            ctrl.rollRadius   = _rollRadius;
            // 将编辑器中设置的板厚传给控制器，使层叠效果一致
            ctrl.plateThickness = _plateThickness;

            Selection.activeGameObject = _targetRoot;
        }

        private GameObject CreatePlate(string plateName, Transform parent)
        {
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = plateName;
            plate.transform.SetParent(parent);
            plate.transform.localScale = new Vector3(_plateWidth, _plateThickness, _plateLength);
            return plate;
        }
    }
}