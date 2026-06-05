namespace _3rdBy.ByFramework.Extension.ExtendComponent
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// 声音组件扩展方法
    /// </summary>
    public static class AudioExtendMethods
    {
        /// <summary>
        /// 格式化名字后返回一个路径
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="prefix"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string GetSaveLocalFullPath(string prefix, string format = ".wav")
        {
            var date = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss");
            date = $"{prefix}{date}{format}";
            var path = "";
#if UNITY_EDITOR
            path = Application.dataPath;
#else
            path = Application.streamingAssetsPath;
#endif
            return Path.Combine(path, date);
        }

        /// <summary>
        /// 格式化音频文件名（前缀+日期+格式）
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="prefix"></param>
        /// <param name="format"></param>
        public static void FormatAudioClipName(this AudioClip clip, string prefix = "", string format = ".wav")
        {
            var date = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss");
            clip.name = $"{prefix}{date}{format}";
        }

        /// <summary>
        /// 获取音频文件要保存本地的路径
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static string SaveLocalFullPath(this AudioClip clip)
        {
            var path = "";
#if UNITY_EDITOR
            path = Application.dataPath;
#else
            path = Application.streamingAssetsPath;
#endif
            return Path.Combine(path, clip.name);
        }

        /// <summary>
        /// 获取音频时间
        /// </summary>
        /// <param name="audioClip">音频文件</param>
        /// <returns>分、秒、毫秒</returns>
        public static (string minutes, string seconds, string milliseconds) GetAudioClipTime(this AudioClip audioClip)
        {
            var audioLength = audioClip.length;
            var minutes = ((int)audioLength / 60).ToString("D2"); //分钟
            var seconds = ((int)audioLength % 60).ToString("D2"); //秒数
            var milliseconds = ((int)(audioLength * 1000) % 1000).ToString("D2"); //毫秒
            return (minutes, seconds, milliseconds);
        }

        /// <summary>
        /// 异步获取音频文件的原始字节 (MP3、WAV、OGG...)
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async UniTask<IEnumerable<byte>> GetByteArray(this AudioClip audioClip, string url)
        {
            return await File.ReadAllBytesAsync(url);
        }

        /// <summary>
        /// 获取格式为.wav音频的byte数组
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        public static byte[] GetWavByteArray(this AudioClip audioClip)
        {
            var samples = new float[audioClip.samples];
            audioClip.GetData(samples, 0);
            var wavData = new byte[samples.Length * 2];
            for (var i = 0; i < samples.Length; i++)
            {
                var sampleValue = (short)(samples[i] * short.MaxValue);
                wavData[i * 2] = (byte)(sampleValue & 0xFF);
                wavData[i * 2 + 1] = (byte)((sampleValue >> 8) & 0xFF);
            }

            return wavData;
        }

        /// <summary>
        /// 异步保存AudioClip文件.wav格式 (.mp3格式应该也支持)
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="savePath"></param>
        public static async UniTask SaveAudioAsWavAsync(this AudioClip clip, string savePath)
        {
            await using var fileStream = new FileStream(savePath, FileMode.Create); //创建一个文件流，用于将音频数据写入WAV文件
            var samples = new float[clip.samples]; //创建一个浮点数数组，用于存储从AudioClip对象中提取的音频样本。数组长度等于clip.samples，即音频剪辑中的样本数
            clip.GetData(samples, 0); //将音频剪辑中的数据提取到samples数组中。0表示从音频剪辑的第一个样本开始提取

            var sampleCount = samples.Length; //获取音频剪辑中的样本数
            var frequency = clip.frequency; //获取音频剪辑的采样率
            var channels = clip.channels; //获取音频剪辑的通道数

            await UniTask.RunOnThreadPool(() =>
            {
                using var writer = new BinaryWriter(fileStream);
                writer.Write("RIFF".ToCharArray()); //将WAV文件的文件标识符（RIFF）写入文件
                writer.Write((uint)(fileStream.Length - 8)); //写入文件长度减去8字节的偏移量。
                writer.Write("WAVE".ToCharArray()); //将WAV文件的格式标识符（WAVE）写入文件。
                writer.Write("fmt ".ToCharArray()); //将音频格式块的标识符（fmt）写入文件
                writer.Write(16); //写入音频格式块的长度（16字节
                writer.Write((ushort)1); //写入通道数（1表示单通道）
                writer.Write((ushort)channels); //写入采样率（采样点每秒的数量）
                writer.Write(frequency); //写入频率（采样点每秒的数量）
                writer.Write(frequency * channels * 2); //写入每秒数据字节数（采样率乘以通道数乘以每个采样点的字节数）
                writer.Write((ushort)(channels * 2)); //写入每个采样点的字节数（通道数乘以每个采样点的字节数）
                writer.Write((ushort)16); //写入位深度（16位表示每个采样点使用16位表示）
                writer.Write("data".ToCharArray()); //将数据块的标识符（data）写入文件
                writer.Write(sampleCount * 2); //写入数据块的长度（采样点数乘以每个采样点的字节数）

                for (var i = 0; i < sampleCount; i++) //遍历所有采样点
                {
                    writer.Write((short)(samples[i] * 32767)); //将每个采样点转换为16位有符号整数（最大值为32767）并写入文件
                }
            });
        }

        /// <summary>
        /// 字节数组转换成音频数据
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static AudioClip ConvertByteArrayToAudioClip(byte[] rawData)
        {
            var samples = new float[rawData.Length / 2];
            const float rescaleFactor = 32767;

            for (var i = 0; i < rawData.Length; i += 2)
            {
                var st = BitConverter.ToInt16(rawData, i);
                var ft = st / rescaleFactor;
                samples[i / 2] = ft;
            }

            var audioClip = AudioClip.Create("TrimmedClip", samples.Length, 1, 44100, false);
            audioClip.SetData(samples, 0);

            return audioClip;
        }

        /// <summary>
        /// String 转换 AudioClip
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static AudioClip ConvertStringToAudioClip(this string self)
        {
            var audioBytes = Convert.FromBase64String(self);
            return ConvertByteArrayToAudioClip(audioBytes);
        }
    }
}