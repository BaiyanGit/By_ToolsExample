namespace Demos.示例_铲斗掉落石头改变地形
{
    using System.Collections.Generic;
    using UnityEngine;

    public class GenerateStoneManager : MonoBehaviour
    {
        public GameObject stonePrefab;
        public int stoneCount = 10;
        public Transform parent;
        public List<Transform> generatePositions = new List<Transform>();
        [Header("一共生成过多少个石头")] public int statisticCount = 0;

        private void GenerateStone()
        {
            for (var i = 0; i < stoneCount; i++)
            {
                var index = i % generatePositions.Count;
                statisticCount++;
                var stone = Instantiate(stonePrefab, generatePositions[index].position, Quaternion.identity);
                stone.transform.SetParent(parent);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GenerateStone();
            }
        }
    }
}