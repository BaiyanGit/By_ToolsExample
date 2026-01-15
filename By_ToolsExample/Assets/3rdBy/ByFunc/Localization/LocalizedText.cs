namespace _3rdBy.ByFunc.Localization
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        private Text _text;

        [Header("表内Key")]
        public string key;

        private void Awake()
        {
            _text = gameObject.GetComponent<Text>();

            Localization.AddText(this);
            SetText();
        }

        public void SetText()
        {
            var text = Localization.GetString(key);
            if (!string.IsNullOrEmpty(text))
            {
                _text.text = Localization.GetString(key);
            }
        }
    }
}