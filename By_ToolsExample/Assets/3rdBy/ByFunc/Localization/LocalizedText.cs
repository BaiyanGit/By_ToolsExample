using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ZCustom
{
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