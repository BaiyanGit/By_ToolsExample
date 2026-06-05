namespace _3rdBy.ByFramework.Guide
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// 协同调度器
    /// </summary>
    public static class Dispatcher
    {
        private class InnerCoroutine : MonoBehaviour
        {
        }

        private static InnerCoroutine coroutine;

        static Dispatcher()
        {
            if (coroutine == null)
            {
                coroutine = new GameObject("Dispatcher").AddComponent<InnerCoroutine>();
                GameObject.DontDestroyOnLoad(coroutine);
            }
        }

        public static Coroutine StartCoroutineTask(IEnumerator routine)
        {
            return coroutine.StartCoroutine(routine);
        }

        private static IEnumerator StartInnerCoroutine(IEnumerator routine, Action<object> callback)
        {
            yield return StartCoroutineTask(routine);
            callback(routine.Current);
        }

        public static void StartCoroutineTask(IEnumerator routine, Action<object> callback)
        {
            StartCoroutineTask(StartInnerCoroutine(routine, callback));
        }

        public static void StopOtherCoroutine(Coroutine c)
        {
            coroutine.StopCoroutine(c);
        }

        public static void StopCoroutineTask(IEnumerator routine)
        {
            coroutine.StopCoroutine(routine);
        }

        public static void StopAllCoroutineTask()
        {
            coroutine.StopAllCoroutines();
        }

    }
}