namespace _3rdBy.ByFunc.SoundRecord.Scripts
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class ButtonClickState : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Sprite pressState;
        public Sprite defaultState;
        public UnityAction<bool> pressCallback;

        private Image _img;

        private void Awake()
        {
            _img = GetComponent<Image>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _img.sprite = pressState;
            pressCallback?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _img.sprite = defaultState;
            pressCallback?.Invoke(false);
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                pressCallback?.Invoke(true);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                pressCallback?.Invoke(false);
            }
        }
    }
}