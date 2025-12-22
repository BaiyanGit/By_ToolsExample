using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Terrain 笔刷工具（Editor Window）
/// 功能：
/// - 在 Scene 视图直接对 Terrain 的高度和纹理（alphamap）进行笔刷操作
/// - 可调大小、强度、衰减（falloff）、模式（抬升/降低/平滑/设置高度/纹理）
/// - 支持 Undo
/// 使用：
/// Window -> Terrain Brush 打开面板，选中 Terrain 后在 Scene 视图左键拖动绘制（左键：应用，右键：反向）
/// 兼容性：基于 UnityEditor API（建议 Unity 2019.4+，在高版本上同样可用）
/// </summary>
public class TerrainBrushWindow : EditorWindow
{
    private enum BrushMode
    {
        RaiseLower,
        Smooth,
        SetHeight,
        PaintTexture
    }

    private BrushMode mode = BrushMode.RaiseLower;
    private float brushSize = 20f;   // 世界单位（米）
    private float strength = 0.5f;   // 0..1
    private float falloff = 0.5f;    // 0..1（0 = 硬边，1 = 软边）
    private float targetHeight = 5f; // 用于 SetHeight 模式（世界高度）
    private int textureIndex = 0;    // 用于 PaintTexture 模式
    private bool showPreview = true;

    private bool isPainting = false;
    private Terrain currentTerrain;
    private int lastMouseButton = -1;

    [MenuItem("Window/Terrain Brush")]
    private static void OpenWindow()
    {
        var w = GetWindow<TerrainBrushWindow>("Terrain Brush");
        w.minSize = new Vector2(320, 140);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Undo.undoRedoPerformed   += Repaint;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Undo.undoRedoPerformed   -= Repaint;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Terrain 笔刷", EditorStyles.boldLabel);

        mode        = (BrushMode)EditorGUILayout.EnumPopup("模式", mode);
        brushSize   = EditorGUILayout.Slider("笔刷大小 (m)", brushSize, 1f, 1000f);
        strength    = EditorGUILayout.Slider("强度", strength, 0f, 1f);
        falloff     = EditorGUILayout.Slider("衰减 (0 硬边, 1 软边)", falloff, 0f, 1f);
        showPreview = EditorGUILayout.Toggle("显示预览", showPreview);

        if (mode == BrushMode.SetHeight)
        {
            targetHeight = EditorGUILayout.FloatField("目标高度 (世界坐标Y)", targetHeight);
        }
        else if (mode == BrushMode.PaintTexture)
        {
            var t = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Terrain>() : null;
            if (t == null)
            {
                EditorGUILayout.HelpBox("先在层级视图选择一个 Terrain 来获取纹理列表", MessageType.Info);
            }
            else
            {
                var      layers                                  = t.terrainData.terrainLayers;
                string[] names                                   = new string[layers.Length];
                for (int i = 0; i < layers.Length; i++) names[i] = layers[i] ? layers[i].name : "Layer " + i;
                textureIndex = EditorGUILayout.Popup("纹理图层", Mathf.Clamp(textureIndex, 0, Math.Max(0, names.Length - 1)), names);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("在 Scene 视图中：\n- 按住左键绘制（默认作用）\n- 右键可做相反操作（例如降低高度或减去纹理）\n- 鼠标拖动时会连续绘制\n- 支持 Undo（Ctrl/Cmd+Z）", MessageType.None);
    }

    private void OnSceneGUI(SceneView sv)
    {
        Event e = Event.current;

        // 找到当前选择或激活的 Terrain
        currentTerrain = GetSelectedTerrain();
        if (currentTerrain == null) return;

        // 获取 Ray（注意：SceneView 的鼠标坐标与 GUI 坐标系差异）
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        // 使用 Terrain 的水平投影来计算近似点（更简单且稳健）
        // 先以地形轴对齐的平面求交，再把点投到地形上
        Plane plane = new Plane(Vector3.up, currentTerrain.transform.position);
        if (!plane.Raycast(ray, out float enter)) return;
        Vector3 hitWorld = ray.GetPoint(enter);

        // 确保点在地形范围内
        Vector3     terrainLocal = hitWorld - currentTerrain.transform.position;
        TerrainData td           = currentTerrain.terrainData;
        if (terrainLocal.x < 0 || terrainLocal.x > td.size.x || terrainLocal.z < 0 || terrainLocal.z > td.size.z) return;

        // Draw preview
        if (showPreview)
        {
            Handles.color = new Color(1f, 0.5f, 0f, 0.25f);
            Handles.DrawSolidDisc(new Vector3(hitWorld.x, currentTerrain.SampleHeight(hitWorld) + currentTerrain.transform.position.y, hitWorld.z), Vector3.up, brushSize * 0.5f);
            Handles.color = Color.white;
            SceneView.RepaintAll();
        }

        // Handle mouse input for painting
        bool wantPaint = (e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && (e.button == 0 || e.button == 1);
        if (wantPaint && !e.alt) // Alt 通常用于摄像机
        {
            // 在 MouseDown 时注册 Undo，这样一次拖动为一次撤销单元
            if (e.type == EventType.MouseDown)
            {
                isPainting      = true;
                lastMouseButton = e.button;
                Undo.RegisterCompleteObjectUndo(td, "Terrain Brush Paint");
            }

            // 只在拖动或按下时实际执行绘制
            if (isPainting)
            {
                bool invert = (e.button == 1); // 右键为反向
                ApplyBrushAt(currentTerrain, terrainLocal, hitWorld, brushSize, strength, falloff, invert);
            }

            e.Use();
        }

        // Mouse up -> 停止绘制
        if ((e.type == EventType.MouseUp) && isPainting)
        {
            isPainting      = false;
            lastMouseButton = -1;
            e.Use();
        }
    }

    private Terrain GetSelectedTerrain()
    {
        if (Selection.activeGameObject)
        {
            var t = Selection.activeGameObject.GetComponent<Terrain>();
            if (t) return t;
        }

        // fallback to activeTerrain
        return Terrain.activeTerrain;
    }

    private void ApplyBrushAt(Terrain terrain, Vector3 terrainLocal, Vector3 worldPos, float sizeMeters, float strength, float falloff, bool invert)
    {
        TerrainData td         = terrain.terrainData;
        Vector3     terrainPos = worldPos - terrain.transform.position;

        int hRes      = td.heightmapResolution;
        int alphaResW = td.alphamapWidth;
        int alphaResH = td.alphamapHeight;

        // 归一化位置
        float normX = terrainPos.x / td.size.x;
        float normZ = terrainPos.z / td.size.z;

        // 中心的像素坐标（heightmap）
        int centerX = Mathf.RoundToInt(normX * (hRes - 1));
        int centerZ = Mathf.RoundToInt(normZ * (hRes - 1));

        // 半径对应 heightmap 像素数量
        float radiusInPixels = (sizeMeters / td.size.x) * (hRes - 1) * 0.5f;

        // 计算扫描区域（限制边界）
        int xMin = Mathf.Clamp(Mathf.FloorToInt(centerX - radiusInPixels), 0, hRes - 1);
        int xMax = Mathf.Clamp(Mathf.CeilToInt(centerX + radiusInPixels), 0, hRes - 1);
        int zMin = Mathf.Clamp(Mathf.FloorToInt(centerZ - radiusInPixels), 0, hRes - 1);
        int zMax = Mathf.Clamp(Mathf.CeilToInt(centerZ + radiusInPixels), 0, hRes - 1);

        // 针对不同模式处理
        if (mode == BrushMode.RaiseLower || mode == BrushMode.Smooth || mode == BrushMode.SetHeight)
        {
            int      w       = xMax - xMin + 1;
            int      h       = zMax - zMin + 1;
            float[,] heights = td.GetHeights(xMin, zMin, w, h);

            for (int z = 0; z < h; z++)
            {
                for (int x = 0; x < w; x++)
                {
                    int hx = xMin + x;
                    int hz = zMin + z;
                    // 计算距离归一权重（0..1）
                    float dx   = hx - centerX;
                    float dz   = hz - centerZ;
                    float dist = Mathf.Sqrt(dx * dx + dz * dz);
                    float t    = Mathf.Clamp01(dist / radiusInPixels);
                    // 衰减曲线（平滑step）
                    float weight = 1f - Mathf.Pow(t, Mathf.Max(1f, 1f + falloff * 4f));
                    weight *= strength;

                    float current = heights[z, x];

                    if (mode == BrushMode.RaiseLower)
                    {
                        float delta = (invert ? -1f : 1f) * weight * 0.01f; // 0.01f 是缩放因子，可根据需要调整
                        heights[z, x] = Mathf.Clamp01(current + delta);
                    }
                    else if (mode == BrushMode.SetHeight)
                    {
                        // 将世界高度 targetHeight 转为归一化高度（0..1）
                        float desiredNormalized = (targetHeight - terrain.transform.position.y) / td.size.y;
                        desiredNormalized = Mathf.Clamp01(desiredNormalized);
                        heights[z, x]     = Mathf.Lerp(current, desiredNormalized, weight);
                    }
                    else if (mode == BrushMode.Smooth)
                    {
                        // 简单高斯式平滑：取周围平均
                        // 读取邻域平均（小半径），这里用当前扫描窗口临近像素
                        float sum     = 0f;
                        int   cnt     = 0;
                        int   kRadius = 1;
                        for (int kz = -kRadius; kz <= kRadius; kz++)
                        {
                            for (int kx = -kRadius; kx <= kRadius; kx++)
                            {
                                int sx = x + kx;
                                int sz = z + kz;
                                if (sx >= 0 && sx < w && sz >= 0 && sz < h)
                                {
                                    sum += heights[sz, sx];
                                    cnt++;
                                }
                            }
                        }

                        if (cnt > 0)
                        {
                            float avg = sum / cnt;
                            heights[z, x] = Mathf.Lerp(current, avg, weight);
                        }
                    }
                }
            }

            td.SetHeights(xMin, zMin, heights);
        }
        else if (mode == BrushMode.PaintTexture)
        {
            // alphamap 采用 alphamap分辨率
            // 先把世界位置转换到 alphamap 坐标
            int   alphaX      = Mathf.RoundToInt(normX * (alphaResW - 1));
            int   alphaZ      = Mathf.RoundToInt(normZ * (alphaResH - 1));
            float radiusAlpha = (sizeMeters / td.size.x) * (alphaResW - 1) * 0.5f;
            int   axMin       = Mathf.Clamp(Mathf.FloorToInt(alphaX - radiusAlpha), 0, alphaResW - 1);
            int   axMax       = Mathf.Clamp(Mathf.CeilToInt(alphaX + radiusAlpha), 0, alphaResW - 1);
            int   azMin       = Mathf.Clamp(Mathf.FloorToInt(alphaZ - radiusAlpha), 0, alphaResH - 1);
            int   azMax       = Mathf.Clamp(Mathf.CeilToInt(alphaZ + radiusAlpha), 0, alphaResH - 1);

            int aw = axMax - axMin + 1;
            int ah = azMax - azMin + 1;

            float[,,] alphas     = td.GetAlphamaps(axMin, azMin, aw, ah);
            int       layers     = td.alphamapLayers;
            int       layerIndex = Mathf.Clamp(textureIndex, 0, layers - 1);

            for (int z = 0; z < ah; z++)
            {
                for (int x = 0; x < aw; x++)
                {
                    int ax = axMin + x;
                    int az = azMin + z;
                    // 计算对应世界位置以求距离
                    float px     = (float)ax / (alphaResW - 1) * td.size.x;
                    float pz     = (float)az / (alphaResH - 1) * td.size.z;
                    float dx     = px - terrainLocal.x;
                    float dz     = pz - terrainLocal.z;
                    float dist   = Mathf.Sqrt(dx * dx + dz * dz);
                    float t      = Mathf.Clamp01(dist / (sizeMeters * 0.5f));
                    float weight = 1f - Mathf.Pow(t, Mathf.Max(1f, 1f + falloff * 4f));
                    weight *= strength;
                    if (invert) weight = -weight;

                    // 调整所选图层的 alpha，其他图层归一化
                    float current = alphas[z, x, layerIndex];
                    float newVal  = Mathf.Clamp01(current + weight);
                    alphas[z, x, layerIndex] = newVal;

                    // 保证所有图层和为1：先计算总和并归一化
                    float sum                            = 0f;
                    for (int L = 0; L < layers; L++) sum += alphas[z, x, L];
                    if (sum > 0f)
                    {
                        for (int L = 0; L < layers; L++) alphas[z, x, L] /= sum;
                    }
                }
            }

            td.SetAlphamaps(axMin, azMin, alphas);
        }
    }
}