namespace _3rdBy.MetaFramework.FSM
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class FSMManager : MonoBehaviour
    {
        public static FSMManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            var container = new GameObject("[FSM]");
            Instance = container.AddComponent<FSMManager>();
            DontDestroyOnLoad(container);
        }

        //状态机列表
        private readonly List<StateMachine> _machines = new List<StateMachine>();

        private void Update()
        {
            for (int i = 0; i < _machines.Count; i++)
            {
                //更新状态机
                _machines[i].OnUpdate();
            }
        }

        /// <summary>
        /// 创建状态机
        /// </summary>
        /// <typeparam name="T">状态机类型</typeparam>
        /// <param name="stateMachineName">状态机名称</param>
        /// <returns>状态机</returns>
        public T Create<T>(string stateMachineName) where T : StateMachine, new()
        {
            Type type = typeof(T);
            stateMachineName = string.IsNullOrEmpty(stateMachineName) ? type.Name : stateMachineName;
            T machine = (T)_machines.Find(m => m.Name == stateMachineName);
            if (machine == null)
            {
                machine = (T)Activator.CreateInstance(type);
                machine.Name = stateMachineName;
                _machines.Add(machine);
                return machine;
            }

            return machine;
        }

        /// <summary>
        /// 销毁状态机
        /// </summary>
        /// <param name="stateMachineName">状态机名称</param>
        /// <returns>true：销毁成功； false：目标状态机不存在，销毁失败</returns>
        public bool Destroy(string stateMachineName)
        {
            var targetMachine = _machines.Find(m => m.Name == stateMachineName);
            if (targetMachine != null)
            {
                targetMachine.OnDestroy();
                _machines.Remove(targetMachine);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 销毁状态机
        /// </summary>
        /// <typeparam name="T">状态机类型</typeparam>
        /// <param name="stateMachine">状态机</param>
        /// <returns>true：销毁成功； false：目标状态机不存在，销毁失败</returns>
        public bool Destroy<T>(T stateMachine) where T : StateMachine, new()
        {
            if (_machines.Contains(stateMachine))
            {
                stateMachine.OnDestroy();
                _machines.Remove(stateMachine);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取状态机
        /// </summary>
        /// <typeparam name="T">状态机类型</typeparam>
        /// <param name="stateMachineName">状态机名称</param>
        /// <returns>状态机</returns>
        public T GetMachine<T>(string stateMachineName) where T : StateMachine
        {
            return (T)_machines.Find(m => m.Name == stateMachineName);
        }
    }
}