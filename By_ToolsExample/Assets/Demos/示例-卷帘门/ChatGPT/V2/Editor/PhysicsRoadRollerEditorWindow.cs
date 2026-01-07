using UnityEditor;
using UnityEngine;

namespace Demos.示例_卷帘门.ChatGPT.Editor
{
    public class PhysicsRoadRollerEditorWindow : EditorWindow
    {
        [MenuItem("ByTools/压路机制造型工具（物理版）")]
        static void Open()
        {
            GetWindow<PhysicsRoadRollerEditorWindow>("压路机 · 物理版");
        }

        private GameObject _root;

        [Header("结构参数")]
        private int   _plateCount     = 20;
        private float _plateLength    = 0.6f;
        private float _plateWidth     = 2.5f;
        private float _plateThickness = 0.15f;
        private float _rollRadius     = 0.9f;

        [Header("物理参数")]
        private float _drumMass       = 300f;
        private float _plateMass      = 20f;
        private float _plateDamping   = 6f;

        [Header("关节阻尼（无弹簧）")]
        private float _angularDamper  = 40f;
        private float _angularForce   = 2000f;

        private Vector2 _scroll;

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField("真实物理卷帘门构建器", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            _root = (GameObject)EditorGUILayout.ObjectField(
                "根节点", _root, typeof(GameObject), true);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("结构参数", EditorStyles.boldLabel);
            _plateCount     = EditorGUILayout.IntSlider("板子数量", _plateCount, 5, 100);
            _plateLength    = EditorGUILayout.FloatField("板子长度", _plateLength);
            _plateWidth     = EditorGUILayout.FloatField("板子宽度", _plateWidth);
            _plateThickness = EditorGUILayout.FloatField("板子厚度", _plateThickness);
            _rollRadius     = EditorGUILayout.FloatField("卷轴半径", _rollRadius);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("物理参数", EditorStyles.boldLabel);
            _drumMass     = EditorGUILayout.FloatField("卷轴质量", _drumMass);
            _plateMass    = EditorGUILayout.FloatField("单块板质量", _plateMass);
            _plateDamping = EditorGUILayout.FloatField("板子角阻尼", _plateDamping);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("关节阻尼", EditorStyles.boldLabel);
            _angularDamper = EditorGUILayout.FloatField("角阻尼", _angularDamper);
            _angularForce  = EditorGUILayout.FloatField("最大约束力", _angularForce);

            EditorGUILayout.Space(15);

            if (GUILayout.Button("构建（真实物理）", GUILayout.Height(32)))
                Build();

            if (_root && GUILayout.Button("清除", GUILayout.Height(24)))
            {
                Undo.RegisterFullObjectHierarchyUndo(_root, "Clear Roller");
                foreach (Transform c in _root.transform)
                    DestroyImmediate(c.gameObject);
            }

            EditorGUILayout.EndScrollView();
        }

        // ============================
        // Build
        // ============================

        void Build()
        {
            if (_root == null)
                _root = new GameObject("RoadRollerRoot");

            Undo.RegisterFullObjectHierarchyUndo(_root, "Build Roller");

            foreach (Transform c in _root.transform)
                DestroyImmediate(c.gameObject);

            // ---------- Drum ----------
            var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            drum.name = "卷轴";
            drum.transform.SetParent(_root.transform);
            drum.transform.rotation = Quaternion.Euler(0, 0, 90);
            drum.transform.localScale =
                new Vector3(_rollRadius * 2f, _plateWidth * 0.5f, _rollRadius * 2f);

            var drumRb = drum.AddComponent<Rigidbody>();
            drumRb.mass = _drumMass;
            drumRb.angularDamping = 6f;
            drumRb.interpolation = RigidbodyInterpolation.Interpolate;

            var hinge = drum.AddComponent<HingeJoint>();
            hinge.axis = Vector3.right;
            hinge.useMotor = true;

            var motor = hinge.motor;
            motor.force = 8000f;
            motor.targetVelocity = 0f;   // ⚠️ 初始不转
            hinge.motor = motor;

            Rigidbody prevRb = drumRb;

            // ---------- Plates ----------
            for (int i = 0; i < _plateCount; i++)
            {
                var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = $"Plate_{i:00}";
                plate.transform.SetParent(_root.transform);
                plate.transform.localScale =
                    new Vector3(_plateWidth, _plateThickness, _plateLength);

                plate.transform.position =
                    drum.transform.position - Vector3.forward * (_plateLength * (i + 1));

                var rb = plate.AddComponent<Rigidbody>();
                rb.mass = _plateMass;
                rb.angularDamping = _plateDamping;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                var joint = plate.AddComponent<ConfigurableJoint>();
                joint.connectedBody = prevRb;
                joint.autoConfigureConnectedAnchor = true;
                joint.anchor = new Vector3(0, 0, -_plateLength * 0.5f);

                // 锁定位移
                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;

                // 允许角度（无弹簧）
                joint.angularXMotion = ConfigurableJointMotion.Limited;
                joint.angularYMotion = ConfigurableJointMotion.Limited;
                joint.angularZMotion = ConfigurableJointMotion.Limited;

                joint.angularXDrive = new JointDrive
                {
                    positionSpring = 0,
                    positionDamper = _angularDamper,
                    maximumForce   = _angularForce
                };

                joint.slerpDrive = new JointDrive
                {
                    positionSpring = 0,
                    positionDamper = _angularDamper * 0.75f,
                    maximumForce   = _angularForce
                };

                prevRb = rb;
            }

            Selection.activeGameObject = _root;
        }
    }
}
