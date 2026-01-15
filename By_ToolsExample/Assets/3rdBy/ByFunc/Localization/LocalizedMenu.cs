namespace _3rdBy.ByFunc.Localization
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Dropdown))]
    public class LocalizedMenu : MonoBehaviour
    {
        [Header("语言Keys，和Dropdown排序一致")]
        public List<string> keys;
    }
}