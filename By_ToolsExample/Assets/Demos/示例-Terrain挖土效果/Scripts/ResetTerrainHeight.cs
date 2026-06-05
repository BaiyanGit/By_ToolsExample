namespace Demos.示例_Terrain挖土效果.Scripts
{
    using UnityEngine;

    /// <summary>
    /// 重置地形
    /// </summary>
    public class ResetTerrainHeight : MonoBehaviour
    {
        public Terrain terrain; // 需要在Inspector面板中分配要修改的地形对象

        private float[,] _backupHeights; // 用于存储备份的高度数据

        private void Start()
        {
            // 备份原始地形高度数据
            BackupTerrainHeights();
        }

        private void BackupTerrainHeights()
        {
            // 获取原始地形数据
            var originalTerrainData = terrain.terrainData;

            // 备份原始地形高度数据
            _backupHeights = originalTerrainData.GetHeights(0, 0, originalTerrainData.heightmapResolution,
                originalTerrainData.heightmapResolution);
        }

        private void OnApplicationQuit()
        {
            ResetTerrainHeights();
        }

        [ContextMenu("ResetTerrainHeights")]
        public void ResetTerrainHeights()
        {
            // 获取地形数据
            var terrainData = terrain.terrainData;

            // 从备份数据中还原地形高度
            terrainData.SetHeights(0, 0, _backupHeights);

            // 应用修改后的地形数据
            terrain.terrainData = terrainData;
        }
    }
}