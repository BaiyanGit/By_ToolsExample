namespace _3rdBy.ByFramework.Guide.Sample.GuideLine
{
    using UnityEngine;

    /// <summary>
    /// 引导线
    /// </summary>
    public class GuidePathLine : MonoBehaviour
    {
        public float _flowSpeed = 10; //流动速度
        private LineRenderer _line;
        private Material _lineMat;

        private Vector3[] _pathArray;

        void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _lineMat = _line.material;
        }

        void Update()
        {
            UpdateOffset();
        }

        private void UpdateOffset()
        {
            _lineMat.mainTextureOffset += Vector2.left * Time.deltaTime * _flowSpeed; //方向根据贴图调整
            if (_lineMat.mainTextureOffset.x % 1 == 0)
            {
                _lineMat.mainTextureOffset = Vector2.zero;
            }
        }

        public void ShowLine(Vector3[] pathArray, float height = 0.1f)
        {
            gameObject.SetActive(true);

            _pathArray = pathArray;
            if (height != 0)
            {
                for (var i = 0; i < _pathArray.Length; i++)
                {
                    _pathArray[i].y = height;
                }
            }

            _line.positionCount = _pathArray.Length;
            _line.SetPositions(_pathArray);
        }

        public void HideLine()
        {
            gameObject.SetActive(false);
        }
    }
}