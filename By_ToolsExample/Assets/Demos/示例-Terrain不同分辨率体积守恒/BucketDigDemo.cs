namespace Demos.示例_Terrain不同分辨率体积守恒
{
    using UnityEngine;

    public class BucketDigDemo : MonoBehaviour
    {
        [Header("地形大小")] public Vector3 terrainSize = Vector3.one;
        [Header("场景地形")] public Terrain terrain;
        [Header("铲斗半径m")] public float bucketRadiusWorld = 10f;
        [Header("单次下挖深度m")] public float digDepthWorld = 1f;
        [Header("铲斗容量m³")] public float bucketCapacity = 500f;

        private BucketLoad _bucket;
        private Vector3 _terrainSize;

        private void Awake()
        {
            // Terrain 实例化，避免污染原始数据
            var td = Instantiate(terrain.terrainData);
            terrainSize         = td.size;
            _terrainSize        = td.size;
            terrain.terrainData = td;

            terrain.GetComponent<TerrainCollider>().terrainData = td;
            _bucket                                             = new BucketLoad(bucketCapacity);
        }

        private void ChangeSize()
        {
            if (_terrainSize != terrainSize)
            {
                _terrainSize             = terrainSize;
                terrain.terrainData.size = terrainSize;
            }
        }

        private Vector3 _rayPointCenter;

        private void OnDrawGizmos()
        {
            Debug.DrawRay(_rayPointCenter, Vector3.up * bucketRadiusWorld, Color.red, 0f, false);

            const int segments = 100;
            Gizmos.color = Color.yellow;
            float angleStep = 360f / segments;
            var   prev      = _rayPointCenter + new Vector3(bucketRadiusWorld, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                var   next  = _rayPointCenter + new Vector3(Mathf.Cos(angle) * bucketRadiusWorld, 0f, Mathf.Sin(angle) * bucketRadiusWorld);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        private void Update()
        {
            ChangeSize();

            if (!Camera.main) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return;

            if (hit.collider.GetComponent<Terrain>() != terrain) return;

            // 在hit.point位置以bucketRadiusWorld为半径绘制一个圆形
            _rayPointCenter = hit.point;


            if (!Input.GetMouseButton(0)) return;

            DigWithBucket(hit.point);
        }

        // ===================== 核心流程 =====================

        private void DigWithBucket(Vector3 hitWorldPos)
        {
            // 1️⃣ 铲斗理论体积（与 Terrain 无关）
            float theoreticalVolume = Mathf.PI * bucketRadiusWorld * bucketRadiusWorld * digDepthWorld;

            // 2️⃣ Terrain 实际能给的体积
            float terrainTaken = DigTerrainVolume(hitWorldPos, bucketRadiusWorld);

            // 3️⃣ 装进铲斗
            float loaded = _bucket.AddVolume(terrainTaken);

            // 4️⃣ Terrain 不够 → 虚拟补齐
            float missing = theoreticalVolume - loaded;
            if (missing > 0f)
            {
                _bucket.AddVolume(missing);
            }

            Debug.Log(
                $"【挖掘结果】\n" +
                $"理论体积 = {theoreticalVolume:F3} m³\n" +
                $"Terrain提供 = {terrainTaken:F3} m³\n" +
                $"铲斗当前 = {_bucket.currentVolume:F3} m³"
            );
        }

        // ===================== Terrain 挖掘 =====================

        private float DigTerrainVolume(Vector3 hitWorldPos, float radiusWorld)
        {
            var data = terrain.terrainData;

            int resolution = data.heightmapResolution;
            var size       = data.size;
            var pos        = terrain.transform.position;

            float pixelSizeX = size.x / (resolution - 1);
            float pixelSizeZ = size.z / (resolution - 1);
            float pixelArea  = pixelSizeX * pixelSizeZ;

            float heightScale = size.y;

            // 世界 → 像素
            int cx = Mathf.RoundToInt((hitWorldPos.x - pos.x) / pixelSizeX);
            int cz = Mathf.RoundToInt((hitWorldPos.z - pos.z) / pixelSizeZ);

            int pixelRadius = Mathf.CeilToInt(radiusWorld / pixelSizeX);

            float[,] heights = data.GetHeights(0, 0, resolution, resolution);

            float totalVolume = 0f;

            for (int z = cz - pixelRadius; z <= cz + pixelRadius; z++)
            for (int x = cx - pixelRadius; x <= cx + pixelRadius; x++)
            {
                if (x < 0 || z < 0 || x >= resolution || z >= resolution)
                    continue;

                float dx   = (x - cx) * pixelSizeX;
                float dz   = (z - cz) * pixelSizeZ;
                float dist = Mathf.Sqrt(dx * dx + dz * dz); // 距离中心点的距离

                if (dist > radiusWorld)
                    continue;

                float oldWorldHeight = heights[z, x] * heightScale;
                float newWorldHeight = Mathf.Max(oldWorldHeight - digDepthWorld, 0f);

                float delta = oldWorldHeight - newWorldHeight;
                if (delta <= 0f)
                    continue;

                totalVolume   += delta * pixelArea;
                heights[z, x] =  newWorldHeight / heightScale;
            }

            data.SetHeights(0, 0, heights);
            return totalVolume;
        }
    }

// ===================== 铲斗装载模型 =====================

    public class BucketLoad
    {
        public float capacity;
        public float currentVolume;

        public BucketLoad(float capacity)
        {
            this.capacity = capacity;
            currentVolume = 0f;
        }

        public float AddVolume(float volume)
        {
            float free  = capacity - currentVolume;
            float added = Mathf.Min(volume, free);
            currentVolume += added;
            return added;
        }

        public float DumpAll()
        {
            float v = currentVolume;
            currentVolume = 0f;
            return v;
        }
    }
}