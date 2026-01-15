namespace Demos.示例_Terrain改变贴图
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [RequireComponent(typeof(Terrain), typeof(TerrainCollider))]
    public class TerrainHandlerTexture : MonoBehaviour
    {
        public enum BrushShape
        {
            [InspectorName("圆形")] Circle,
            [InspectorName("矩形")] Square,
            [InspectorName("长方形")] Rectangle,
            [InspectorName("自定义")] Custom
        }

        [Header("当前层索引")] public string currentLayerName;
        [Header("地形图层")] public List<TerrainLayer> terrainLayersConfig = new();
        [Header("刷子")] public BrushShape brushShape = BrushShape.Circle;

        [Tooltip("使用 custom，则需要 Read/Write enabled")] [Header("自定义刷子")]
        public Texture2D customBrush;

        [Tooltip("单位为米（这是半径，而不是直径）")] [Header("刷子半径")]
        public float brushRadius = 5f; // 米（用于圆形 / 方形的半边）

        [Header("长方形宽度（米，只有选 Rectangle 时生效）")] public float rectWidth = 10f;

        [Header("长方形高度（米，只有选 Rectangle 时生效）")] public float rectHeight = 6f;

        [Header("矩形/长方形圆角（米），0 为硬角，最大为对应半边尺寸")] [Tooltip("矩形/长方形模式下圆角半径，单位米（0 = 无圆角，最大等于半边尺寸）。")]
        public float cornerRadius = 0f;

        [Header("笔刷强度")] [Range(0f, 1f)] public float brushStrength = 0.3f;
        [Header("右键为减弱当前图层")] public bool useRightMouseToSubtract; // 如果为 true，右键为减弱当前图层
        [Header("调试模式")] public bool debug;

        private Terrain _terrain;
        private TerrainData _terrainData;
        private Camera _camera;
        private int _currentLayer = 0; // 默认从 0 开始

        // 如果需要，可以选择缓存的圆形阿尔法纹理（不是严格需要的）
        private Texture2D _cachedCircleBrush;

        private void Awake()
        {
            _camera = Camera.main ?? Camera.current;

            _terrain = GetComponent<Terrain>();
            if (_terrain == null)
            {
                Debug.LogError("未找到地形组件...");
                enabled = false;
                return;
            }

            // 为了不修改原始 asset，克隆一份 runtime TerrainData 并赋回 Terrain
            _terrainData                                = Instantiate(_terrain.terrainData);
            _terrain.terrainData                        = _terrainData;
            GetComponent<TerrainCollider>().terrainData = _terrainData;

            AddLayer();

            if (terrainLayersConfig != null && terrainLayersConfig.Count > 0)
                currentLayerName = terrainLayersConfig[0].name;
            else
                currentLayerName = string.Empty;

            // 预生成圆形笔刷纹理（可用于 custom-less 圆形计算示例）
            _cachedCircleBrush = CreateCircleBrush(64);
        }

        private void AddLayer()
        {
            var currentTerrainLayers = _terrainData.terrainLayers?.ToList() ?? new List<TerrainLayer>();
            if ((terrainLayersConfig == null || terrainLayersConfig.Count == 0) && currentTerrainLayers.Count == 0)
            {
                Debug.LogError("没有可添加的TerrainLayers。");
                return;
            }

            // 如果没有在 inspector 指定 layers，尝试从 terrainData 里读取
            if ((terrainLayersConfig == null || terrainLayersConfig.Count == 0) && currentTerrainLayers.Count > 0)
            {
                terrainLayersConfig = new List<TerrainLayer>(currentTerrainLayers);
                return;
            }

            // 如果挂的 terrainData 没有 layer，则使用外部提供的 terrainLayers（或从 terrainData 读取）
            foreach (var layer in terrainLayersConfig)
            {
                var layerName = layer.name;
                var isExists  = currentTerrainLayers.Exists(t => t.name.Equals(layerName));
                if (isExists) continue; // 如果已经存在，跳过
                currentTerrainLayers.Add(layer);
            }

            _terrainData.terrainLayers = currentTerrainLayers.ToArray();
        }

        private void OnDestroy()
        {
            if (_cachedCircleBrush != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(_cachedCircleBrush);
#else
                Destroy(_cachedCircleBrush);
#endif
            }
        }

        private void Update()
        {
            // 切换图层（中键切换，避免与绘制冲突）
            if (Input.GetMouseButtonDown(2))
            {
                if (terrainLayersConfig != null && terrainLayersConfig.Count > 0)
                {
                    _currentLayer = (_currentLayer + 1) % terrainLayersConfig.Count;
                    Debug.Log($"切换图层：{terrainLayersConfig[_currentLayer].name} (index {_currentLayer})");
                }
                else
                {
                    Debug.LogWarning("无需切换地形图层.");
                }
            }

            // 滚轮调节半径（仅影响圆形/方形；长方形请在 Inspector 修改 rectWidth/rectHeight）
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0f)
            {
                brushRadius = Mathf.Clamp(brushRadius + scroll * 2f, 0.01f, Mathf.Max(_terrainData.size.x, _terrainData.size.z));
            }

            // 左键绘制，右键可选择减弱（如 useRightMouseToSubtract = true）
            if (Input.GetMouseButton(0) || (useRightMouseToSubtract && Input.GetMouseButton(1)))
            {
                Paint(Input.GetMouseButton(1)); // 传入是否按下右键（作为 subtract）
            }
        }

        private void Paint(bool subtract = false)
        {
            if (terrainLayersConfig == null || terrainLayersConfig.Count == 0) return;
            if (_camera == null) return;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return;

            var hitTerrain = hit.collider != null ? hit.collider.GetComponent<Terrain>() : null;
            if (hitTerrain != _terrain) return; // 只在当前 Terrain 上生效

            var terrainPos = hit.point - _terrain.transform.position;

            // alphamap 信息
            int alphaW = _terrainData.alphamapWidth;
            int alphaH = _terrainData.alphamapHeight;
            int layers = _terrainData.alphamapLayers;
            if (layers == 0 || alphaW <= 0 || alphaH <= 0) return;

            // 归一化到 [0,1]
            float normX = Mathf.InverseLerp(0f, _terrainData.size.x, terrainPos.x);
            float normZ = Mathf.InverseLerp(0f, _terrainData.size.z, terrainPos.z);

            // 映射到 alphamap 坐标（使用 alphaW - 1 与 alphaH - 1）
            float fx      = normX * (alphaW - 1f);
            float fz      = normZ * (alphaH - 1f);
            int   centerX = Mathf.RoundToInt(fx);
            int   centerZ = Mathf.RoundToInt(fz);

            // 计算笔刷在像素坐标下的半径（分别计算 X/Z，确保地形非方形时不会误算）
            float safeSizeX = Mathf.Max(0.0001f, _terrainData.size.x);
            float safeSizeZ = Mathf.Max(0.0001f, _terrainData.size.z);
            // 对于圆形/方形，使用 brushRadius；对于 rect，用 rectWidth/rectHeight 半边计算像素范围
            float radiusPixelsX, radiusPixelsZ;
            if (brushShape == BrushShape.Rectangle)
            {
                float halfW = Mathf.Max(0.0001f, rectWidth * 0.5f);
                float halfH = Mathf.Max(0.0001f, rectHeight * 0.5f);
                radiusPixelsX = (halfW / safeSizeX) * (alphaW - 1f);
                radiusPixelsZ = (halfH / safeSizeZ) * (alphaH - 1f);
            }
            else
            {
                float half = Mathf.Max(0.0001f, brushRadius);
                radiusPixelsX = (half / safeSizeX) * (alphaW - 1f);
                radiusPixelsZ = (half / safeSizeZ) * (alphaH - 1f);
            }

            // 限制半径，避免超出整个图（可按需调整最大值）
            radiusPixelsX = Mathf.Clamp(radiusPixelsX, 0f, alphaW);
            radiusPixelsZ = Mathf.Clamp(radiusPixelsZ, 0f, alphaH);

            int xMin = Mathf.Clamp(Mathf.FloorToInt(centerX - radiusPixelsX), 0, alphaW - 1);
            int xMax = Mathf.Clamp(Mathf.CeilToInt(centerX + radiusPixelsX), 0, alphaW - 1);
            int zMin = Mathf.Clamp(Mathf.FloorToInt(centerZ - radiusPixelsZ), 0, alphaH - 1);
            int zMax = Mathf.Clamp(Mathf.CeilToInt(centerZ + radiusPixelsZ), 0, alphaH - 1);
            int w    = xMax - xMin + 1;
            int h    = zMax - zMin + 1;

            if (w <= 0 || h <= 0) return;

            if (debug)
            {
                // 可开启下面日志查看参数
                // Debug.Log($"alphamap: {alphaW}x{alphaH}, center=({centerX},{centerZ}), pxRadius=({radiusPixelsX:F1},{radiusPixelsZ:F1}), rect=({xMin},{zMin})-({xMax},{zMax})");
            }

            float[,,] alphas = _terrainData.GetAlphamaps(xMin, zMin, w, h);

            int layerIndex = Mathf.Clamp(_currentLayer, 0, layers - 1);
            if (terrainLayersConfig != null && terrainLayersConfig.Count > 0)
                currentLayerName = terrainLayersConfig[_currentLayer].name;

            // safe values
            float safeCircleRadius = Mathf.Max(0.0001f, brushRadius);
            float halfRectW        = Mathf.Max(0.0001f, rectWidth * 0.5f);
            float halfRectH        = Mathf.Max(0.0001f, rectHeight * 0.5f);
            float corner           = Mathf.Clamp(cornerRadius, 0f, Mathf.Max(safeCircleRadius, Mathf.Max(halfRectW, halfRectH)));

            for (int z = 0; z < h; z++)
            {
                for (int x = 0; x < w; x++)
                {
                    int ax = xMin + x;
                    int az = zMin + z;

                    // 把 alphamap 像素映射回世界坐标（注意 alphaW-1 的使用）
                    float px    = (float)ax / Mathf.Max(1f, (alphaW - 1f)) * _terrainData.size.x;
                    float pz    = (float)az / Mathf.Max(1f, (alphaH - 1f)) * _terrainData.size.z;
                    float dx    = px - terrainPos.x;
                    float dz    = pz - terrainPos.z;
                    float dist  = Mathf.Sqrt(dx * dx + dz * dz);
                    float absDx = Mathf.Abs(dx);
                    float absDz = Mathf.Abs(dz);

                    // 计算 tShape（0..1），>1 表示超出刷子区域
                    float tShape = 1f;

                    switch (brushShape)
                    {
                        case BrushShape.Circle:
                            tShape = dist / safeCircleRadius;
                            break;

                        case BrushShape.Square:
                        {
                            // 方形（以 brushRadius 为半边）
                            float half = Mathf.Max(0.0001f, brushRadius);
                            if (corner <= 0f)
                            {
                                // 硬角正方形（Chebyshev）
                                tShape = Mathf.Max(absDx, absDz) / half;
                            }
                            else
                            {
                                float   inner    = Mathf.Max(0f, half - corner); // 中心方块半边
                                Vector2 q        = new Vector2(Mathf.Max(0f, absDx - inner), Mathf.Max(0f, absDz - inner));
                                float   dToOuter = q.magnitude; // 0..corner
                                tShape = (corner <= 0f) ? 1f : (dToOuter / corner);
                            }
                        }
                            break;

                        case BrushShape.Rectangle:
                        {
                            // 长方形，rectWidth/rectHeight 为完整尺寸
                            if (corner <= 0f)
                            {
                                // 硬角长方形：按归一化轴向距离判断
                                float nx = absDx / halfRectW;
                                float nz = absDz / halfRectH;
                                tShape = Mathf.Max(nx, nz);
                            }
                            else
                            {
                                // 圆角长方形：中心矩形半边尺寸
                                float   innerX   = Mathf.Max(0f, halfRectW - corner);
                                float   innerY   = Mathf.Max(0f, halfRectH - corner);
                                Vector2 q        = new Vector2(Mathf.Max(0f, absDx - innerX), Mathf.Max(0f, absDz - innerY));
                                float   dToOuter = q.magnitude; // 0..corner
                                tShape = (corner <= 0f) ? 1f : (dToOuter / corner);
                            }
                        }
                            break;

                        case BrushShape.Custom:
                        {
                            // Custom：以最大轴向距离为基准（支持自定义纹理掩码）
                            if (corner <= 0f)
                            {
                                // 使用 brushRadius 的中心正方形基准（与 Square 类似）
                                float half = Mathf.Max(0.0001f, brushRadius);
                                tShape = Mathf.Max(absDx, absDz) / half;
                            }
                            else
                            {
                                float   inner    = Mathf.Max(0f, brushRadius - corner);
                                Vector2 q        = new Vector2(Mathf.Max(0f, absDx - inner), Mathf.Max(0f, absDz - inner));
                                float   dToOuter = q.magnitude;
                                tShape = (corner <= 0f) ? 1f : (dToOuter / corner);
                            }
                        }
                            break;
                    }

                    float weight = 0f;
                    if (tShape <= 1f)
                    {
                        // 衰减曲线（可替换为高斯）
                        float falloff = 0.8f;
                        float curve   = 1f - Mathf.Pow(Mathf.Clamp01(tShape), Mathf.Max(1f, 1f + falloff * 4f));
                        weight = curve * brushStrength;

                        // 自定义纹理采样：针对不同形状映射 UV
                        if (brushShape == BrushShape.Custom && customBrush != null)
                        {
                            if (!customBrush.isReadable)
                            {
                                if (debug) Debug.LogWarning("customBrush is not readable. Enable Read/Write in import settings.");
                                weight = 0f;
                            }
                            else
                            {
                                float u = 0.5f, v = 0.5f;
                                if (brushShape == BrushShape.Rectangle)
                                {
                                    // rect mapping: dx in [-halfRectW,halfRectW] -> [0,1]
                                    u = dx / (rectWidth) + 0.5f;
                                    v = dz / (rectHeight) + 0.5f;
                                }
                                else if (brushShape == BrushShape.Square)
                                {
                                    // square mapping: dx in [-half,half] -> [0,1]
                                    float half = Mathf.Max(0.0001f, brushRadius);
                                    u = dx / (half * 2f) + 0.5f;
                                    v = dz / (half * 2f) + 0.5f;
                                }
                                else // Circle / Custom default mapping based on brushRadius
                                {
                                    float half = Mathf.Max(0.0001f, brushRadius);
                                    u = dx / (half * 2f) + 0.5f;
                                    v = dz / (half * 2f) + 0.5f;
                                }

                                u = Mathf.Clamp01(u);
                                v = Mathf.Clamp01(v);
                                float mask = customBrush.GetPixelBilinear(u, v).a;
                                weight *= mask;
                            }
                        }

                        // Circle 模式额外确保超出圆半径的点排除（当其他基准使用轴向时）
                        if (brushShape == BrushShape.Circle && dist > safeCircleRadius)
                        {
                            weight = 0f;
                        }

                        // Rectangle 额外确保超出矩形外部点被排除（当基准使用 corner 距离时）
                        if (brushShape == BrushShape.Rectangle)
                        {
                            bool outside;
                            if (corner <= 0f)
                            {
                                outside = (absDx > halfRectW) || (absDz > halfRectH);
                            }
                            else
                            {
                                float   innerX = Mathf.Max(0f, halfRectW - corner);
                                float   innerY = Mathf.Max(0f, halfRectH - corner);
                                Vector2 q      = new Vector2(Mathf.Max(0f, absDx - innerX), Mathf.Max(0f, absDz - innerY));
                                outside = (q.magnitude > corner);
                            }

                            if (outside) weight = 0f;
                        }
                    }
                    else
                    {
                        weight = 0f;
                    }

                    // 如果是减弱模式或按住右键要减弱，则把 weight 取负
                    bool isSubtract        = subtract && useRightMouseToSubtract;
                    if (isSubtract) weight = -weight;

                    // 应用到选中层
                    float original = alphas[z, x, layerIndex];
                    float newVal   = Mathf.Clamp01(original + weight);
                    alphas[z, x, layerIndex] = newVal;

                    // 归一化所有层，保证总和为 1（防止数值漂移）
                    float sum                            = 0f;
                    for (int l = 0; l < layers; l++) sum += alphas[z, x, l];

                    if (sum <= 0f)
                    {
                        // 若总和为 0，则均分（避免黑洞）
                        float fill                                       = 1f / layers;
                        for (int l = 0; l < layers; l++) alphas[z, x, l] = fill;
                    }
                    else
                    {
                        // 归一化
                        for (int l = 0; l < layers; l++) alphas[z, x, l] /= sum;
                    }
                }
            }

            // 写回 alphamap（只写修改区域）
            _terrainData.SetAlphamaps(xMin, zMin, alphas);

            if (debug)
                Debug.DrawRay(hit.point, Vector3.up * 2f, Color.red, 0.1f);
        }

        private Texture2D CreateCircleBrush(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            var   center = new Vector2(size * 0.5f, size * 0.5f);
            float r      = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    float a = Mathf.Clamp01(1f - d / r);
                    a = Mathf.SmoothStep(0f, 1f, a);
                    tex.SetPixel(x, y, new Color(a, a, a, a));
                }
            }

            tex.Apply();
            return tex;
        }
    }
}