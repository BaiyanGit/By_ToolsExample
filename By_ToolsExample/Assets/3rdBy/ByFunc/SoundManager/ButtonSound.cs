namespace _3rdBy.ByFunc.SoundManager
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public enum ButtonSoundType
    {
        /// <summary>
        /// 名字形式
        /// </summary>
        AudioClipName,

        /// <summary>
        /// 挂载声源形式
        /// </summary>
        AudioClipMount,

        /// <summary>
        /// 索引形式
        /// </summary>
        AudioClipIndex
    }

    public class ButtonSound : MonoBehaviour, IPointerClickHandler
    {
        public ButtonSoundType buttonSoundType;
        public int audioIndex;
        public string audioName = "按钮音效1";
        public AudioClip audioClip;

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (buttonSoundType)
            {
                case ButtonSoundType.AudioClipMount:
                    SoundManager.Instance.PlayClip(audioClip);
                    break;
                case ButtonSoundType.AudioClipName:
                    SoundManager.Instance.PlayClip(audioName);
                    break;
                case ButtonSoundType.AudioClipIndex:
                    SoundManager.Instance.PlayClip(audioIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}