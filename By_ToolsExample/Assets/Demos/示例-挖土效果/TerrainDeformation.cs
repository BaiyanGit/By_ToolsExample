// 脚本名称：TerrainDeformation.cs
// 脚本功能：实现地形挖土效果

using UnityEngine;

namespace Demos.示例_挖土效果
{
    public class TerrainDeformation : MonoBehaviour
    {
        [Header("变形地形")] public Terrain terrain;
        [Header("变形半径")] public float deformationRadius = 2f;
        [Header("变形深度")] public float deformationDepth = 1f;

        /// <summary>
        /// 碰撞体进入触发器时调用
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            // 输出碰撞体名称
            Debug.Log("OnTriggerEnter" + other.name);
        }

        /// <summary>
        /// 碰撞体进入碰撞器时调用
        /// </summary>
        /// <param name="other"></param>
        private void OnCollisionEnter(Collision other)
        {
            // 输出碰撞体所属游戏对象的名称
            Debug.Log("OnCollisionEnter" + other.gameObject.name);
        }

        // 每帧更新时调用
        private void Update()
        {
            // 射线检测检,测挖掘机铲斗与地形的碰撞
            if (!Physics.Raycast(transform.position, Vector3.down, out var hit)) return;
            if (hit.collider.gameObject != terrain.gameObject) return; // 判断碰撞体是否为地形
            var deformationPosition = hit.point; // 获取碰撞点在地形上的位置
            DeformTerrain(deformationPosition); // 修改地形高度
        }

        /// <summary>
        /// 变形地形
        /// </summary>
        /// <param name="position"></param>
        private void DeformTerrain(Vector3 position)
        {
            var terrainData = terrain.terrainData; // 获取地形数据
            var terrainPos = terrain.transform.position; // 获取地形位置

            // 计算碰撞点在地形上的坐标
            var xPos = Mathf.FloorToInt((position.x - terrainPos.x) / terrainData.heightmapScale.x);
            var zPos = Mathf.FloorToInt((position.z - terrainPos.z) / terrainData.heightmapScale.z);

            // 计算地形高度数据的起始坐标和宽度
            // 注释：此函数计算地形高度数据的起始坐标和宽度。
            var xHeight = xPos - Mathf.CeilToInt(deformationRadius);
            var yHeight = zPos - Mathf.CeilToInt(deformationRadius);
            var xWidth = Mathf.CeilToInt(deformationRadius * 2);
            var yWidth = Mathf.CeilToInt(deformationRadius * 2);

            var heights = terrainData.GetHeights(xHeight, yHeight, xWidth, yWidth); // 获取地形高度数据

            // 修改地形高度数据
            for (var x = 0; x < heights.GetLength(0); x++)
            {
                for (var z = 0; z < heights.GetLength(1); z++)
                {
                    // 计算点到中心点的距离
                    var v1 = new Vector2(x, z);
                    var v2 = new Vector2(xPos - xHeight, zPos - yHeight);
                    var distance = Vector2.Distance(v1, v2);

                    // 判断点是否在变形范围内
                    if (distance <= deformationRadius)
                    {
                        // 根据距离计算变形深度
                        heights[x, z] -= deformationDepth * (1 - distance / deformationRadius);
                    }
                }
            }

            terrainData.SetHeights(xHeight, yHeight, heights); // 将修改后的高度数据写回地形
            terrain.Flush(); // 刷新地形
        }
    }
}