namespace _3rdBy.ByFramework.FSM
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class FSMManager : MonoBehaviour
    {
        public static FSMManager Instance { get; private set; }

        private static bool _isRegistered;
        private static bool _isInitialized;
        private static bool _isStarted;

        internal static void RegisterForFrameworkEntry()
        {
            _isRegistered = true;
        }

        internal static void InitializeForFrameworkEntry(Transform frameworkRoot)
        {
            if (_isInitialized)
            {
                return;
            }

            if (!_isRegistered)
            {
                Debug.LogError("[ByFramework][FSMManager] Initialize 前必须先执行 Register。");
                return;
            }

            FSMManager existingManager = FindFirstObjectByType<FSMManager>();
            if (existingManager != null)
            {
                Instance = existingManager;
                DontDestroyOnLoad(existingManager.gameObject);
                _isInitialized = true;
                return;
            }

            GameObject container = new("[FSM]");
            Instance = container.AddComponent<FSMManager>();

            if (frameworkRoot != null)
            {
                container.transform.SetParent(frameworkRoot);
            }
            else
            {
                DontDestroyOnLoad(container);
            }

            _isInitialized = true;
        }

        internal static void StartForFrameworkEntry()
        {
            if (!_isInitialized || _isStarted)
            {
                return;
            }

            if (Instance != null)
            {
                Instance.enabled = true;
            }

            _isStarted = true;
        }

        internal static void StopForFrameworkEntry()
        {
            if (!_isStarted)
            {
                return;
            }

            if (Instance != null)
            {
                Instance.enabled = false;
            }

            _isStarted = false;
        }

        internal static void ShutdownForFrameworkEntry()
        {
            if (!_isInitialized)
            {
                _isRegistered = false;
                return;
            }

            StopForFrameworkEntry();

            if (Instance != null)
            {
                Instance.ClearAllMachines();
                Instance = null;
            }

            _isInitialized = false;
            _isRegistered = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            if (Instance != null)
            {
                Instance.ClearAllMachines();
            }

            Instance = null;
            _isRegistered = false;
            _isInitialized = false;
            _isStarted = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ShutdownForFrameworkEntry();
            }
        }

        //状态机列表
        private readonly List<StateMachine> _machines = new List<StateMachine>();

        private void Update()
        {
            if (!_isStarted)
            {
                return;
            }

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
            var type = typeof(T);
            stateMachineName = string.IsNullOrEmpty(stateMachineName) ? type.Name : stateMachineName;
            var machine = (T)_machines.Find(m => m.Name == stateMachineName);
            if (machine == null)
            {
                machine      = (T)Activator.CreateInstance(type);
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

        private void ClearAllMachines()
        {
            for (int i = _machines.Count - 1; i >= 0; i--)
            {
                _machines[i]?.OnDestroy();
            }

            _machines.Clear();
        }
    }
}
