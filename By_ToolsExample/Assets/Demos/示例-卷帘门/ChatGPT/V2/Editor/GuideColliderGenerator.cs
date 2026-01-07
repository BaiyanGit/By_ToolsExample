using UnityEditor;
using UnityEngine;

public class GuideColliderGenerator : EditorWindow
{
    [MenuItem("ByTools/卷帘门/导向 Collider 生成器")]
    static void Open()
    {
        GetWindow<GuideColliderGenerator>("导向 Collider");
    }

    private Transform drum;

    // =========================
    // 语义尺寸（不再出现 XYZ）
    // =========================

    [Header("导向片数量")] public int guideCount = 10;

    [Header("导向片尺寸（语义）")] [Tooltip("导向片厚度（沿物体本地 X）")]
    public float guideThickness = 0.15f;

    [Tooltip("导向片宽度（沿物体本地 Y / 卷轴宽）")] public float guideWidth = 1.0f;

    [Tooltip("导向片高度（沿物体本地 Z）")] public float guideHeight = 0.6f;

    [Header("位置")] [Tooltip("相对卷轴半径的外扩偏移")]
    public float radiusOffset = 0.08f;

    [Header("螺旋趋势")] [Tooltip("导向片倾斜角度（决定是否能卷）")]
    public float tiltAngle = 12f;

    [Header("摩擦")] public float staticFriction = 0.9f;
    public float dynamicFriction = 0.8f;

    void OnGUI()
    {
        EditorGUILayout.LabelField("导向 Collider 一键生成工具（语义版）", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        EditorGUI.BeginChangeCheck();

        drum = (Transform)EditorGUILayout.ObjectField(
            "卷轴 Transform", drum, typeof(Transform), true);

        EditorGUILayout.Space(8);

        guideCount = EditorGUILayout.IntSlider("导向片数量", guideCount, 6, 16);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("导向片尺寸", EditorStyles.boldLabel);
        guideThickness = EditorGUILayout.FloatField("厚度", guideThickness);
        guideWidth     = EditorGUILayout.FloatField("宽度", guideWidth);
        guideHeight    = EditorGUILayout.FloatField("高度", guideHeight);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("位置 & 趋势", EditorStyles.boldLabel);
        radiusOffset = EditorGUILayout.FloatField("半径偏移", radiusOffset);
        tiltAngle    = EditorGUILayout.Slider("螺旋斜角", tiltAngle, 5f, 25f);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("摩擦", EditorStyles.boldLabel);
        staticFriction  = EditorGUILayout.Slider("Static Friction", staticFriction, 0f, 1f);
        dynamicFriction = EditorGUILayout.Slider("Dynamic Friction", dynamicFriction, 0f, 1f);

        bool changed = EditorGUI.EndChangeCheck();

        EditorGUILayout.Space(10);

        GUI.enabled = drum != null;
        if (GUILayout.Button("生成导向 Collider", GUILayout.Height(30)))
        {
            Generate();
        }

        GUI.enabled = true;

        // 参数变化 → 实时生效（非运行态）
        if (changed && drum != null && !Application.isPlaying)
        {
            Generate();
        }
    }

    void Generate()
    {
        if (drum == null || Application.isPlaying)
            return;

        Undo.RegisterFullObjectHierarchyUndo(drum.gameObject, "Update Guide Colliders");

        // 清理旧的
        var old = drum.Find("Guides");
        if (old != null)
            DestroyImmediate(old.gameObject);

        var root = new GameObject("Guides");
        root.transform.SetParent(drum);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;

        // 摩擦材质
        var mat = new PhysicsMaterial("GuideFriction")
        {
            staticFriction  = staticFriction,
            dynamicFriction = dynamicFriction,
            frictionCombine = PhysicsMaterialCombine.Maximum
        };

        float radius = drum.lossyScale.x * 0.5f + radiusOffset;

        for (int i = 0; i < guideCount; i++)
        {
            float angle = 360f / guideCount * i;

            var g = new GameObject($"Guide_{i:00}");
            g.transform.SetParent(root.transform);

            // 位置：绕卷轴一圈
            g.transform.localPosition =
                Quaternion.Euler(0, angle, 0) * Vector3.right * radius;

            // 朝向 + 螺旋趋势
            g.transform.localRotation =
                Quaternion.Euler(0, angle, tiltAngle);

            // Collider（注意：这里是“语义 → 本地轴”映射）
            var col = g.AddComponent<BoxCollider>();
            col.size = new Vector3(
                guideThickness, // local X
                guideWidth,     // local Y
                guideHeight     // local Z
            );
            col.material = mat;
        }

        Selection.activeGameObject = root;
    }
}