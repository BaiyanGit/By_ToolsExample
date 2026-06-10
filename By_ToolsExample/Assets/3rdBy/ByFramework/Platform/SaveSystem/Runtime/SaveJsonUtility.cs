//=====================================================
// 文件名称: SaveJsonUtility.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供 SaveSystem Runtime 内部使用的 JSON 序列化工具。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System;
    using _3rdBy.Plugins.LitJson;

    internal static class SaveJsonUtility
    {
        internal static string SerializeObject<TData>(TData data)
        {
            return data == null ? "null" : JsonMapper.ToJson(data);
        }

        internal static TData DeserializeObject<TData>(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "null")
            {
                return default;
            }

            return JsonMapper.ToObject<TData>(json);
        }

        internal static string SerializeEnvelope(SaveDataEnvelope envelope)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            return JsonMapper.ToJson(envelope);
        }

        internal static SaveDataEnvelope DeserializeEnvelope(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Envelope json cannot be null or empty.", nameof(json));
            }

            return JsonMapper.ToObject<SaveDataEnvelope>(json);
        }
    }
}
