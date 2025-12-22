//
// using UnityEngine;
// using UnityEngine.TerrainTools;
// public class TerrainDeformation2 : MonoBehaviour
// {
//     public float deformationRadius = 1f; // 坑的半径
//     public float deformationStrength = 1f; // 坑的深度
//
//     private Terrain terrain;
//     private TerrainData terrainData;
//     private int heightMapResolution;
//
//     private void Start()
//     {
//         // 获取当前场景中的地形对象
//         terrain = Terrain.activeTerrain;
//         terrainData = terrain.terrainData;
//         heightMapResolution = terrainData.heightmapResolution;
//     }
//
//     private void OnCollisionEnter(Collision collision)
//     {
//         // 检查碰撞对象是否为地形
//         if (collision.gameObject.CompareTag("Terrain"))
//         {
//             // 获取碰撞点的世界坐标
//             Vector3 collisionPoint = collision.GetContact(0).point;
//
//             // 将世界坐标转换为地形坐标
//             Vector3 terrainCoordinates = collision.gameObject.transform.InverseTransformPoint(collisionPoint);
//
//             // 计算碰撞点在高度图中的索引
//             int xIndex = (int)(terrainCoordinates.x * heightMapResolution / terrainData.size.x);
//             int zIndex = (int)(terrainCoordinates.z * heightMapResolution / terrainData.size.z);
//
//             // 修改高度图
//             TerrainToolkit.DeformTerrain(terrainData, xIndex, zIndex, deformationRadius, deformationStrength);
//
//             // 更新地形数据
//             terrainData.SetHeights(0, 0, terrainData.heights);
//         }
//     }
// }