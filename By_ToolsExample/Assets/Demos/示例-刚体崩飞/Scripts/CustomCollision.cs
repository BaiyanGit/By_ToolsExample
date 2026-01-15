namespace Demos.示例_刚体崩飞.Scripts
{
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// 碰撞暂停物体
    /// </summary>
    public class CustomCollision : MonoBehaviour
    {
        [Header("刚体")] public Rigidbody rigidbody;

        private void FixedUpdate()
        {
            Debug.Log(rigidbody.IsSleeping());
        }

        private void OnCollisionEnter(Collision collision)
        {
            Assert.IsTrue(collision.gameObject.name == "Plane");
            Debug.Log("静止");
            gameObject.transform.position = new Vector3(0, 0, 0);
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}