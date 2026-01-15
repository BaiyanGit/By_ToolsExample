namespace _3rdBy.ByFunc.LoopScrollRect.Demo.Scripts
{
    using LoopScrollRect.Scripts;
    using UnityEngine;

    [RequireComponent(typeof(LoopScrollRect))]
    [DisallowMultipleComponent]
    public class InitOnStart : MonoBehaviour
    {
        public int totalCount = -1;
        void Start()
        {
            var ls = GetComponent<LoopScrollRect>();
            ls.totalCount = totalCount;
            ls.RefillCells();
        }
    }
}