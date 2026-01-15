using UnityEngine;

namespace _3rdBy.ByFunc.LoopScrollRect.Demo.Scripts
{
    using UnityEngine.UI;

    public class ScrollIndexCallback3 : MonoBehaviour
    {
        public Text text;
        void ScrollCellIndex(int idx)
        {
            string name = "Cell " + idx.ToString();
            if (text != null)
            {
                text.text = name;
            }
        }
    }
}
