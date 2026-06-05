using System;
using UnityEngine;

namespace Demos.示例_UI曲面叠加滚动
{
    public class ParamsTest : MonoBehaviour
    {
        [ContextMenu("正常参数")]
        private void TestParams1()
        {
            TestParams("正常参数", "hello", new Vector3(1, 2, 3));
        }

        [ContextMenu("委托参数")]
        private void TestParams2()
        {
            TestParams("委托参数", "hello", (Action<string>)action);
            return;

            void action(string s)
            {
                Debug.Log(s);
            }
        }

        [ContextMenu("Params拆分参数")]
        private void TestParams3()
        {
        
            object[] args = { "参数_1", "参数_2", "参数_3" , (Action<string>)action };
        
        
            @params("Params拆分参数", "参数_1", "参数_2", "参数_3", (Action<string>)action);
            return;

            void @params(string msg, params object[] args)
            {
                TestParams(msg, args[0], args);
            }

            void action(string s)
            {
                Debug.Log(s);
            }
        }

        private static void TestParams(string msg, params object[] args)
        {
            Debug.Log($"<color=yellow>==== {msg}【{args.Length}】====</color>");
            PrintArgs(args);
        }

        private static void PrintArgs(params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case Action<string>:
                        ((Action<string>)args[i])($"i={i}, arg=委托测试");
                        break;
                    case object[] array:
                        Debug.Log("<color=green>----- 内部数组 -----</color>");
                        PrintArgs(array);
                        break;
                    default:
                        Debug.Log($"i={i}, arg={args[i]}");
                        break;
                }
            }
        }
    }
}