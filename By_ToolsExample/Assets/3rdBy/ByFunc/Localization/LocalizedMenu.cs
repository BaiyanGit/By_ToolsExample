using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZCustom
{
    [RequireComponent(typeof(Dropdown))]
    public class LocalizedMenu : MonoBehaviour
    {
        [Header("语言Keys，和Dropdown排序一致")]
        public List<string> keys;
    }
}