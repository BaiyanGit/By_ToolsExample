using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;

/// <summary>
/// Terrain 纹理绘制工具（EditorTool）
/// - 不继承 EditorWindow；作为 EditorTool 显示在工具栏或在选中 Terrain 时可用
/// - 支持笔刷大小、强度、衰减（falloff）、图层选择、预览、Undo
/// - 左键绘制，右键反向（扣除）；按住拖动连续绘制
/// 使用：将此脚本放到 Assets/Editor 下，选中 Terrain 后在 Scene 视图启用 "Terrain Texture Painter" 工具即可
/// </summary>
[EditorTool("Terrain Texture Painter", typeof(Terrain))]
public class TerrainTexturePainterTool : EditorTool
{
    // UI 参数（可根据需要改为 ScriptableObject 保存设置）
    float brushSize = 10f; // 笔刷直径（米）
    float strength = 0.5f; // 0..1
    float falloff = 0.5f;  // 0..1 (0 硬边, 1 软边)
    bool showPreview = true;

    int textureIndex = 0;

    bool isPainting = false;
    int lastButton = -1;

    // 快捷键：T 切换并选择此工具（可选）
    [Shortcut("Tools/Terrain Texture Painter", KeyCode.T)]
    static void SelectTool()
    {
        ToolManager.SetActiveTool<OnDemandToolMarker>();
    }

    // A trivial on-demand marker tool to let Shortcut select our EditorTool (workaround)
    class OnDemandToolMarker : EditorTool
    {
    }

    // public override GUIContent toolbarIcon
    // {
    //     get
    //     {
    //         return EditorGUIUtility.IconContent("TerrainInspector.TerrainTool"); // 使用内置图标
    //     }
    // }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView)) return;

        Event e = Event.current;

        // 获取被选中的 Terrain（如果是多选，取第一个）
        Terrain terrain = GetSelectedTerrain();
        if (terrain == null) return;

        TerrainData td = terrain.terrainData;
        if (td == null) return;

        // 绘制场景界面上的简单悬浮面板（左上角）
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 300, 160), "Terrain Painter", "Window");
        EditorGUI.BeginChangeCheck();
        brushSize   = EditorGUILayout.Slider(new GUIContent("Brush Size (m)"), brushSize, 0.5f, Mathf.Max(td.size.x, td.size.z));
        strength    = EditorGUILayout.Slider(new GUIContent("Strength"), strength, 0f, 1f);
        falloff     = EditorGUILayout.Slider(new GUIContent("Falloff"), falloff, 0f, 1f);
        showPreview = EditorGUILayout.Toggle("Show Preview", showPreview);

        // 纹理图层选择
        var layers = td.terrainLayers;
        if (layers != null && layers.Length > 0)
        {
            string[] names                                   = new string[layers.Length];
            for (int i = 0; i < layers.Length; i++) names[i] = layers[i] ? layers[i].name : ("Layer " + i);
            textureIndex = EditorGUILayout.Popup("Paint Layer", Mathf.Clamp(textureIndex, 0, layers.Length - 1), names);
        }
        else
        {
            EditorGUILayout.HelpBox("No TerrainLayers found on this Terrain.", MessageType.Warning);
        }

        if (EditorGUI.EndChangeCheck())
        {
            // 参数改动时强制重绘 SceneView 以刷新预览
            SceneView.RepaintAll();
        }

        GUILayout.EndArea();
        Handles.EndGUI();

        // 处理鼠标与绘制
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        // 与地形平面求交，得到近似世界位置
        Plane plane = new Plane(Vector3.up, terrain.transform.position);
        if (!plane.Raycast(ray, out float enter)) return;
        Vector3 hitWorld = ray.GetPoint(enter);

        // 检查是否在地形范围内
        Vector3 localPos = hitWorld - terrain.transform.position;
        if (localPos.x < 0f || localPos.x > td.size.x || localPos.z < 0f || localPos.z > td.size.z) return;

        // 获取精确地形表面高度以便将预览贴合表面
        float   surfaceY   = terrain.SampleHeight(hitWorld) + terrain.transform.position.y;
        Vector3 previewPos = new Vector3(hitWorld.x, surfaceY + 0.02f, hitWorld.z); // 稍微抬高防止 Z-fighting

        // 预览显示
        if (showPreview)
        {
            Handles.color = new Color(1f, 1f, 0f, 0.25f);
            Handles.DrawSolidDisc(previewPos, Vector3.up, brushSize * 0.5f);
            Handles.color = Color.white;
            SceneView.RepaintAll();
        }

        // 只有当鼠标在 SceneView 且不按住 Alt (Alt 控制相机) 时才处理绘画
        bool wantPaint = (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && (e.button == 0 || e.button == 1);
        if (wantPaint && !e.alt)
        {
            // 在 MouseDown 时记录 Undo
            if (e.type == EventType.MouseDown)
            {
                isPainting = true;
                lastButton = e.button;
                Undo.RegisterCompleteObjectUndo(td, "Terrain Texture Paint");
            }

            if (isPainting)
            {
                bool invert = (e.button == 1);
                ApplyAlphaBrush(terrain, localPos, brushSize, strength, falloff, textureIndex, invert);
            }

            e.Use();
        }

        if (e.type == EventType.MouseUp && isPainting)
        {
            isPainting = false;
            lastButton = -1;
            e.Use();
        }
    }

    Terrain GetSelectedTerrain()
    {
        if (Selection.activeGameObject)
        {
            var t = Selection.activeGameObject.GetComponent<Terrain>();
            if (t != null) return t;
        }

        // fallback to activeTerrain
        return Terrain.activeTerrain;
    }

    void ApplyAlphaBrush(Terrain terrain, Vector3 terrainLocalPos, float brushDiameter, float strength, float falloff, int layerIndex, bool invert)
    {
        TerrainData td = terrain.terrainData;
        if (td == null) return;

        int alphaW = td.alphamapWidth;
        int alphaH = td.alphamapHeight;
        int layers = td.alphamapLayers;
        if (layers == 0) return;
        layerIndex = Mathf.Clamp(layerIndex, 0, layers - 1);

        // 计算归一化坐标（0..1）
        float normX = terrainLocalPos.x / td.size.x;
        float normZ = terrainLocalPos.z / td.size.z;

        // 对应 alpha 像素坐标
        float fx      = normX * (alphaW - 1);
        float fz      = normZ * (alphaH - 1);
        int   centerX = Mathf.RoundToInt(fx);
        int   centerZ = Mathf.RoundToInt(fz);

        // 半径（像素）
        float radius = (brushDiameter / td.size.x) * (alphaW - 1) * 0.5f;
        int   xMin   = Mathf.Clamp(Mathf.FloorToInt(centerX - radius), 0, alphaW - 1);
        int   xMax   = Mathf.Clamp(Mathf.CeilToInt(centerX + radius), 0, alphaW - 1);
        int   zMin   = Mathf.Clamp(Mathf.FloorToInt(centerZ - radius), 0, alphaH - 1);
        int   zMax   = Mathf.Clamp(Mathf.CeilToInt(centerZ + radius), 0, alphaH - 1);

        int width  = xMax - xMin + 1;
        int height = zMax - zMin + 1;

        // 读取区域 alphamaps
        float[,,] alphas = td.GetAlphamaps(xMin, zMin, width, height);

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int ax = xMin + x;
                int az = zMin + z;

                // 计算像素对应的世界坐标（局部）
                float px   = (float)ax / (alphaW - 1) * td.size.x;
                float pz   = (float)az / (alphaH - 1) * td.size.z;
                float dx   = px - terrainLocalPos.x;
                float dz   = pz - terrainLocalPos.z;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);

                float t = Mathf.Clamp01(dist / (brushDiameter * 0.5f));
                // 衰减：使用指数平滑
                float w = 1f - Mathf.Pow(t, Mathf.Max(1f, 1f + falloff * 4f));
                w *= strength;
                if (invert) w = -w;

                float original = alphas[z, x, layerIndex];
                float newVal   = Mathf.Clamp01(original + w);

                // 写入变化到选中图层
                alphas[z, x, layerIndex] = newVal;

                // 重新归一化所有图层，使之和为 1
                float sum                            = 0f;
                for (int L = 0; L < layers; L++) sum += alphas[z, x, L];
                if (sum <= 0f)
                {
                    // 如果被扣为 0，则把其他图层平均分配
                    float fill                                       = 1f / layers;
                    for (int L = 0; L < layers; L++) alphas[z, x, L] = fill;
                }
                else
                {
                    for (int L = 0; L < layers; L++) alphas[z, x, L] /= sum;
                }
            }
        }

        td.SetAlphamaps(xMin, zMin, alphas);
    }
}