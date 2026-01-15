namespace Demos.示例_回放功能.Scripts
{
    using UnityEngine;

    public class User
    {
        public string userName;
        public int age;
    }

    public interface IUserService
    {
        void RegisterUser(User user);
    }


    public class TestSerilizable : MonoBehaviour,IUserService
    {
        private void Start()
        {
            User user = new User();
            user.userName = "张三";
            user.age      = 18;
            RegisterUser(user);
        }

        public void RegisterUser(User user)
        {
            Debug.Log(user.userName + "注册成功");
        }
    }
}