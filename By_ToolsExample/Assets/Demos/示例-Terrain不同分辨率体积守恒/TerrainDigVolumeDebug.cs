using UnityEngine;

namespace Demos.示例_Terrain不同分辨率体积守恒
{
    public class TerrainDigVolumeDebug : MonoBehaviour
    {
        [Header("References")] public Terrain terrain;

        [Header("Bucket Params (World Units)")]
        public float bucketRadius = 10f;

        public float digDepth = 1f;

        // === 累计统计 ===
        private float _theoreticalVolumeSum;
        private float _actualTerrainVolumeSum;

        private TerrainData _data;
        private int _resolution;
        private Vector3 _terrainPos;
        private Vector3 _terrainSize;

        private float _pixelSizeX;
        private float _pixelSizeZ;
        private float _pixelArea;
        private float _heightScale;

        private void Awake()
        {
            // 实例化 TerrainData，避免污染原始数据
            _data                                               = Instantiate(terrain.terrainData);
            terrain.terrainData                                 = _data;
            terrain.GetComponent<TerrainCollider>().terrainData = _data;

            _resolution  = _data.heightmapResolution;
            _terrainPos  = terrain.transform.position;
            _terrainSize = _data.size;

            _pixelSizeX  = _terrainSize.x / (_resolution - 1);
            _pixelSizeZ  = _terrainSize.z / (_resolution - 1);
            _pixelArea   = _pixelSizeX * _pixelSizeZ;
            _heightScale = _terrainSize.y;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            if (!Camera.main)
                return;

            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
                return;

            if (hit.collider.GetComponent<Terrain>() != terrain)
                return;

            Dig(hit.point);
        }

        // ================= 核心验证 =================

        private void Dig(Vector3 hitWorldPos)
        {
            // 1️⃣ 理论请求体积（与 Terrain 无关）
            float theoretical = Mathf.PI * bucketRadius * bucketRadius * digDepth;
            _theoreticalVolumeSum += theoretical;

            // 2️⃣ Terrain 实际减少体积（唯一真实来源）
            float actual = DigTerrainAndGetVolume(hitWorldPos);
            _actualTerrainVolumeSum += actual;
        }

        private float DigTerrainAndGetVolume(Vector3 hitWorldPos)
        {
            float[,] heights = _data.GetHeights(0, 0, _resolution, _resolution);

            int cx = Mathf.RoundToInt((hitWorldPos.x - _terrainPos.x) / _pixelSizeX);
            int cz = Mathf.RoundToInt((hitWorldPos.z - _terrainPos.z) / _pixelSizeZ);

            int pixelRadius = Mathf.CeilToInt(bucketRadius / _pixelSizeX);

            float volume = 0f;

            for (int z = cz - pixelRadius; z <= cz + pixelRadius; z++)
            for (int x = cx - pixelRadius; x <= cx + pixelRadius; x++)
            {
                if (x < 0 || z < 0 || x >= _resolution || z >= _resolution)
                    continue;

                float dx   = (x - cx) * _pixelSizeX;
                float dz   = (z - cz) * _pixelSizeZ;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);

                if (dist > bucketRadius)
                    continue;

                float oldWorldH = heights[z, x] * _heightScale;
                float newWorldH = Mathf.Max(oldWorldH - digDepth, 0f);

                float delta = oldWorldH - newWorldH;
                if (delta <= 0f)
                    continue;

                volume        += delta * _pixelArea;
                heights[z, x] =  newWorldH / _heightScale;
            }

            _data.SetHeights(0, 0, heights);
            return volume;
        }

        // ================= Debug Overlay =================

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 420, 140), GUI.skin.box);
            GUILayout.Label("<b>Bucket Dig Volume Debug</b>");

            GUILayout.Label($"理论请求体积 (Σπr²h):  {_theoreticalVolumeSum:F2} m³");
            GUILayout.Label($"Terrain真实减少体积:   {_actualTerrainVolumeSum:F2} m³");

            float diff  = _theoreticalVolumeSum - _actualTerrainVolumeSum;
            float ratio = _theoreticalVolumeSum > 0f ? diff / _theoreticalVolumeSum * 100f : 0f;

            GUILayout.Label($"差值(离散误差):         {diff:F3} m³");
            GUILayout.Label($"误差比例:               {ratio:F3} %");

            GUILayout.EndArea();
        }
    }
}