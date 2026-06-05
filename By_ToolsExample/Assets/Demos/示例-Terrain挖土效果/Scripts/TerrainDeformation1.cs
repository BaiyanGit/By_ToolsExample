namespace Demos.示例_Terrain挖土效果.Scripts
{
    using UnityEngine;

    public class TerrainDeformation1 : MonoBehaviour
    {
        public float deformationRadius = 1f; // 坑的半径
        public float deformationStrength = 1f; // 坑的深度

        private Terrain _terrain;
        private TerrainData _terrainData;
        private int _heightMapResolution;
        private float[,] _heights;

        private void Start()
        {
            // 获取当前场景中的地形对象
            _terrain = Terrain.activeTerrain;
            _terrainData = _terrain.terrainData;
            _heightMapResolution = _terrainData.heightmapResolution;

            // 获取初始高度数据
            _heights = _terrainData.GetHeights(0, 0, _heightMapResolution, _heightMapResolution);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // 检查碰撞对象是否为地形
            if (collision.gameObject.CompareTag("Terrain"))
            {
                // 获取碰撞点的世界坐标
                var collisionPoint = collision.GetContact(0).point;

                // 将世界坐标转换为地形坐标
                var terrainCoordinates = collision.gameObject.transform.InverseTransformPoint(collisionPoint);

                // 计算碰撞点在高度图中的索引
                var xIndex = (int)(terrainCoordinates.x * _heightMapResolution / _terrainData.size.x);
                var zIndex = (int)(terrainCoordinates.z * _heightMapResolution / _terrainData.size.z);

                // 修改高度图
                DeformTerrain(xIndex, zIndex);

                // 更新地形数据
                _terrainData.SetHeights(0, 0, _heights);
            }
        }

        private void DeformTerrain(int xIndex, int zIndex)
        {
            for (var x = 0; x < _heightMapResolution; x++)
            {
                for (var z = 0; z < _heightMapResolution; z++)
                {
                    var distance = Vector2.Distance(new Vector2(xIndex, zIndex), new Vector2(x, z));
                    if (!(distance <= deformationRadius)) continue;
                    var deformation = deformationStrength * (1 - distance / deformationRadius);

                    _heights[z, x] = Mathf.Clamp(_heights[z, x] - deformation, 0, 2); // 将高度值限制在0到2的范围内
                }
            }
        }

        // private void DeformTerrain(int xIndex, int zIndex)
        // {
        //     for (int x = 0; x < heightMapResolution; x++)
        //     {
        //         for (int z = 0; z < heightMapResolution; z++)
        //         {
        //             float distance = Vector2.Distance(new Vector2(xIndex, zIndex), new Vector2(x, z));
        //             if (distance <= deformationRadius)
        //             {
        //                 float deformation = deformationStrength * (1 - distance / deformationRadius);
        //                 heights[z, x] -= deformation;
        //             }
        //         }
        //     }
        // }
    }
}