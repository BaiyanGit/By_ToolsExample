namespace _3rdBy.ByFramework.SoundManager
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class ButtonColor : MonoBehaviour, IPointerDownHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        public Color normal = new Color(0.18f, 0.42f, 1f);
        public Color hover = Color.white;
        public Color pressed = new Color(0.18f, 0.42f, 1f);
        public Text text;

        public void OnPointerEnter(PointerEventData eventData)
        {
            text.color = hover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            text.color = normal;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            text.color = pressed;
        }
    }
}