namespace _3rdBy.ByFunc.SoundRecord.Scripts
{
    using System;
    using Cysharp.Threading.Tasks;
    using Extension.ExtendComponent;
    using MetaFramework.HttpNetwork;
    using UnityEngine;

    public enum ChatType
    {
        Text,
        Voice,
    }


    public class TestVoiceMsg : MonoBehaviour
    {
        private VoiceRequestData _voiceRequestData = new()
        {
            userid   = 0,
            username = "123"
        };

        /// <summary>
        /// 加入房间
        /// </summary>
        private async UniTaskVoid SendJoinRoom()
        {
            var resData = await NetworkHelper.PostJsonAsync<VoiceRequestData, VoiceResponseData>(
                              "", _voiceRequestData, 5, null);
            // Debug.Log($"Code:{resData.code} Msg:{resData.msg}");
        }


        [SerializeField] private AudioClip audioClip;

        private void Start()
        {
            SendJoinRoom().Forget();
            SendChatMsg(audioClip).Forget();
            // SendChatMsg("这一段测试文本发送的内容").Forget();
        }

        /// <summary>
        /// 发送聊天消息（文本）
        /// </summary>
        /// <param name="text"></param>
        private async UniTaskVoid SendChatMsg(string text)
        {
            var chatRequestData = new VoiceUploadChatRequestData()
            {
                type   = (int)ChatType.Text,
                userid = 0,
                data   = text
            };

            var resData = await NetworkHelper.PostJsonAsync<RequestDataStruct, ResponseDataStruct>(
                              "", chatRequestData, 5);
            // Debug.Log($"Text Code:{resData.code} Msg:{resData.msg}");
        }

        /// <summary>
        /// 发送聊天消息（语音）
        /// </summary>
        private async UniTaskVoid SendChatMsg(AudioClip clip)
        {
            var chatRequestData = new VoiceUploadChatRequestData()
            {
                type   = (int)ChatType.Voice,
                userid = 0,
                // data = clip.GetWavByteArray()
                data = Convert.ToBase64String(clip.GetWavByteArray())
            };

            Debug.Log($"Send Voice: {chatRequestData.data}");

            var resData =
                await NetworkHelper.PostJsonAsync<RequestDataStruct, ResponseDataStruct>("", chatRequestData, 5, null);

            Debug.Log($"[CallBack] Voice Code:{resData.code} Msg:{resData.msg}");
            ReceiveChatMsg().Forget();
        }

        private float deltaTime;

        private void Update()
        {
            deltaTime += Time.deltaTime;
            if (!(deltaTime > 200)) return;
            // ReceiveChatMsg().Forget();
            deltaTime = 0;
        }


        /// <summary>
        /// 异步接收聊天消息
        /// </summary>
        [Obsolete("Obsolete")]
        private async UniTaskVoid ReceiveChatMsg()
        {
            var resData =
                await NetworkHelper.PostJsonAsync<VoiceRequestData, VoiceUploadChatResponseData>(
                    "", _voiceRequestData, 5);
            //Debug.Log($"Voice Code:{resData.code} Msg:{resData.msg}");

            if ((ChatType)resData.infos.type == ChatType.Text)
            {
                var textMsg = resData.infos.data;
                Debug.Log($"[CallBack] Text:{textMsg}");
            }
            else
            {
                Debug.Log(resData.infos.data);
                var audioData = Convert.FromBase64String(resData.infos.data);
                var path      = AudioExtendMethods.GetSaveLocalFullPath("测试");
                var audio     = AudioExtendMethods.ConvertByteArrayToAudioClip(audioData);
                await audio.SaveAudioAsWavAsync(path);


                // await AudioExtendMethods.SaveAudioAsWavAsync(audioData, path);
                Debug.Log("保存完成" + path);

                // var ad = new GameObject().AddComponent<AudioSource>();
                // ad.loop = true;
                // ad.Play();
            }
        }
    }
}