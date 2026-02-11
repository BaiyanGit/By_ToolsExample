namespace Demos.示例_Terrain铲斗掉落石头改变地形
{
    using System.Collections.Generic;
    using UnityEngine;

    public class StoneManager : MonoBehaviour
    {
        public static StoneManager instance;

        [Header("Destroy Settings")] public float delayDestroy = 3f;

        public List<Stone> stones = new List<Stone>();
        private float _timer;

        private void Awake()
        {
            // 单例保护
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        public void RegisterStone(Stone stone)
        {
            if (stone == null) return;

            // 防止重复注册
            if (!stones.Contains(stone))
            {
                stones.Add(stone);
                _timer = delayDestroy; // 新石头进来，重置倒计时
            }
            else
            {
                Debug.Log($"重复：{stone.name}");
            }
        }

        private void Update()
        {
            if (stones.Count == 0) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            // 延迟到达，统一销毁
            for (var i = stones.Count - 1; i >= 0; i--)
            {
                if (stones[i] != null)
                {
                    Destroy(stones[i].gameObject);
                    TerrainHandler.instance.RegisterStone(stones[i].data);
                }
            }

            stones.Clear();
            _timer = 0f;
        }
    }
}