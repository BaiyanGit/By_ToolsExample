namespace Demos.示例_滚轮测试.Scripts
{
    using UnityEngine;

    public class RollTest : MonoBehaviour
    {
        [Range(1, 10)] public int speed = 1;
        private Vector3 _originPos;
        private Rigidbody _rigidbody;

        private void Start()
        {
            _originPos = transform.position;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                _rigidbody.linearVelocity = new Vector3(0, 0, speed);
            }

            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                _rigidbody.linearVelocity = new Vector3(0, 0, -speed);
            }

            if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
            {
                _rigidbody.linearVelocity = new Vector3(0, 0, 0);
            }

            if (Input.GetKey(KeyCode.R))
            {
                Transform transform1;
                (transform1 = transform).gameObject.SetActive(false);
                transform1.position = _originPos;
                transform.gameObject.SetActive(true);
            }
        }
    }
}