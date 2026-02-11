namespace Demos.示例_UI曲面叠加滚动.Scripts
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 此脚本用于将UI元素的文本内容设置为其父对象的名称。
    /// </summary>
    [ExecuteInEditMode]
    public class UIText : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Text>().text = transform.parent.name;
        }
    }
}