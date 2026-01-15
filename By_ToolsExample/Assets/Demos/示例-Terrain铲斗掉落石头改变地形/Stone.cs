namespace Demos.示例_Terrain铲斗掉落石头改变地形
{
    using UnityEngine;

    public class StoneData
    {
        public Vector3 position = Vector3.zero;
    }

    public class Stone : MonoBehaviour
    {
        public bool isExit;
        public bool isStop;

        [Header("Physics")] public float stableSpeedThreshold = 0.1f;
        public float stableTime = 0.3f;

        private Rigidbody _rb;
        private float _stableTimer;
        private int _mirrorLayer;
        private bool _isInsideMirror;

        public readonly StoneData data = new StoneData();

        private void Awake()
        {
            _rb          = GetComponent<Rigidbody>();
            _mirrorLayer = LayerMask.NameToLayer("Mirror");

            if (_rb == null)
            {
                Debug.LogError($"{name} 没有 Rigidbody");
                enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == _mirrorLayer)
            {
                _isInsideMirror = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == _mirrorLayer)
            {
                _isInsideMirror = false;
                isExit          = true;
                Debug.Log($"离开容器：{name}");
            }
        }

        private void Update()
        {
            // 🔒 兜底：如果从来没在容器里，但当前已经不在容器区域
            if (!isExit && !_isInsideMirror)
            {
                isExit = true;
            }

            if (!isExit || isStop) return;

            if (_rb.IsSleeping() || _rb.linearVelocity.magnitude < stableSpeedThreshold)
            {
                _stableTimer += Time.deltaTime;

                if (_stableTimer >= stableTime)
                {
                    isStop        = true;
                    data.position = _rb.position;
                    if (StoneManager.instance != null)
                    {
                        StoneManager.instance.RegisterStone(this);
                    }
                }
            }
            else
            {
                _stableTimer = 0f;
            }
        }
    }
}