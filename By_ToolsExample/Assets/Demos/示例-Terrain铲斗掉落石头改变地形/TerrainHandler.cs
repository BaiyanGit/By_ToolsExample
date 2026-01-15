namespace Demos.示例_Terrain铲斗掉落石头改变地形
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TerrainHandler : MonoBehaviour
    {
        public Terrain terrain;
        public TerrainData terrainData;
        public int resolution;

        public static TerrainHandler instance;

        [Header("批处理设置")] public float batchInterval = 0.1f;

        [Header("土体影响设置")] public float stoneWorldRadius = 0.5f; // 单个石头影响半径（世界单位）
        public float stoneEffectiveHeight = 0.3f;                // 单个石头可转化为“土”的有效世界高度
        public int smoothIterations = 1;                         // 土体扩散次数（>=1 才像土）

        private float _timer;
        private readonly List<StoneData> stoneDataRequest = new();

        private void Awake()
        {
            instance = this;

            terrainData         = Instantiate(terrain.terrainData);
            terrain.terrainData = terrainData;

            resolution = terrainData.heightmapResolution;
        }

        public void RegisterStone(StoneData data)
        {
            if (data == null) return;
            stoneDataRequest.Add(data);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= batchInterval && stoneDataRequest.Count > 0)
            {
                _timer = 0f;
                StartCoroutine(ApplyAllRequestsCoroutine());
            }
        }

        private IEnumerator ApplyAllRequestsCoroutine()
        {
            // 1. 拷贝请求
            var stoneDatas = new List<StoneData>(stoneDataRequest);
            stoneDataRequest.Clear();

            // 2. 高度增量缓存（一维索引 → height01 delta）
            var deltaMap = new Dictionary<int, float>();

            int minX = resolution, minY = resolution;
            int maxX = 0,          maxY = 0;

            var pixelRadius = Mathf.CeilToInt(stoneWorldRadius / terrainData.size.x * resolution);

            foreach (var data in stoneDatas)
            {
                if (data == null) continue;

                WorldToTerrain(data.position, out var cx, out var cy);

                // ① 世界高度 → heightmap 归一化高度
                var height01Total = stoneEffectiveHeight / terrainData.size.y;

                // ② 先计算权重和（保证土量守恒）
                var weightSum = 0f;

                for (var dy = -pixelRadius; dy <= pixelRadius; dy++)
                for (var dx = -pixelRadius; dx <= pixelRadius; dx++)
                {
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > pixelRadius) continue;

                    weightSum += 1f - dist / pixelRadius;
                }

                if (weightSum <= 0f) continue;

                // ③ 分布高度
                for (var dy = -pixelRadius; dy <= pixelRadius; dy++)
                for (var dx = -pixelRadius; dx <= pixelRadius; dx++)
                {
                    var x = cx + dx;
                    var y = cy + dy;

                    if (x < 0 || y < 0 || x >= resolution || y >= resolution)
                        continue;

                    var dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > pixelRadius) continue;

                    var weight  = 1f - dist / pixelRadius;
                    var delta01 = height01Total * (weight / weightSum);

                    var index = y * resolution + x;

                    if (!deltaMap.TryAdd(index, delta01))
                        deltaMap[index] += delta01;

                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            if (deltaMap.Count == 0)
                yield break;

            var width  = maxX - minX + 1;
            var height = maxY - minY + 1;

            // 4. 一次性获取高度
            var heightmap = terrainData.GetHeights(minX, minY, width, height);

            // 5. 应用 delta
            foreach (var pair in deltaMap)
            {
                var index = pair.Key;
                var delta = pair.Value;

                var x = index % resolution - minX;
                var y = index / resolution - minY;

                heightmap[y, x] += delta;
            }

            // 6. 土体扩散 / 填补空隙
            if (smoothIterations > 0)
            {
                SmoothHeightmap(heightmap, smoothIterations);
            }

            // 7. 一次性写回地形
            terrainData.SetHeightsDelayLOD(minX, minY, heightmap);

            yield return null;
        }

        private static void SmoothHeightmap(float[,] map, int iterations)
        {
            var h = map.GetLength(0);
            var w = map.GetLength(1);

            for (var it = 0; it < iterations; it++)
            {
                var copy = (float[,])map.Clone();

                for (var y = 1; y < h - 1; y++)
                for (var x = 1; x < w - 1; x++)
                {
                    map[y, x] = (copy[y, x] +
                                 copy[y - 1, x] +
                                 copy[y + 1, x] +
                                 copy[y, x - 1] +
                                 copy[y, x + 1]) / 5f;
                }
            }
        }

        private void WorldToTerrain(Vector3 worldPos, out int x, out int y)
        {
            x = Mathf.RoundToInt((worldPos.x - terrain.transform.position.x) / terrainData.size.x * resolution);
            y = Mathf.RoundToInt((worldPos.z - terrain.transform.position.z) / terrainData.size.z * resolution);
        }
    }
}