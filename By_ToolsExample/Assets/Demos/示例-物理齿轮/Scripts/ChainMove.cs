namespace Demos.示例_物理齿轮.Scripts
{
    using UnityEngine;

    /// <summary>
    /// 铰链移动
    /// </summary>
    public class ChainMove : MonoBehaviour
    {
        public Vector3 startPos = Vector3.zero;
        public Vector3 endPos = Vector3.zero;
        public bool isMove;
        public float moveSpeed = 1;

        private bool _isBack;
        private Transform _moveTarget;

        private void Awake()
        {
            _moveTarget = transform;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
                isMove = !isMove;

            if (!isMove) return;

            var moveOffset = Vector3.forward * (moveSpeed * Time.deltaTime);
            _moveTarget.Translate(moveOffset);

            var localPosition = _moveTarget.localPosition;
            var endDis = Vector3.Distance(localPosition, endPos);
            var staDis = Vector3.Distance(localPosition, startPos);
            if (endDis < 0.01f)
            {
                moveSpeed *= -1; //反方向
                // Debug.LogError($"dis:{endDis}     pos:{localPosition}");
            }

            if (staDis < 0.01f)
            {
                moveSpeed = -moveSpeed;
            }
        }
    }
}