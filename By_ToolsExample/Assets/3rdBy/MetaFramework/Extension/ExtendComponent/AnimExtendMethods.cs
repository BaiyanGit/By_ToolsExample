namespace _3rdBy.MetaFramework.Extension.ExtendComponent
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// 动画的扩展方法
    /// </summary>
    public static class AnimExtendMethods
    {
        /// <summary>
        /// 异步播放动画
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateName"></param>
        /// <param name="cancellationToken"></param>
        public static async UniTask PlayAnimAsync(this Animator animator, string stateName,
            CancellationToken cancellationToken = default)
        {
            animator.Play(stateName);
            await UniTask.Yield();
            var length = animator.GetCurrentAnimatorStateInfo(0).length;
            await UniTask.Delay(TimeSpan.FromSeconds(length), cancellationToken: cancellationToken);
        }
    }
}