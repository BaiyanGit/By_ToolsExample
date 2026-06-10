//=====================================================
// 文件名称: FrameworkConfigJsonUtility.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供 FrameworkConfig Runtime 内部使用的 JSON 解析、合并与模块读取工具。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;
    using _3rdBy.Plugins.LitJson;

    internal static class FrameworkConfigJsonUtility
    {
        internal static string NormalizeJson(string json)
        {
            JsonData data = Parse(json);
            return JsonMapper.ToJson(data);
        }

        internal static JsonData Parse(string json)
        {
            string normalizedJson = string.IsNullOrWhiteSpace(json) ? "{}" : json;
            JsonData data = JsonMapper.ToObject(normalizedJson);
            if (!data.IsObject)
            {
                throw new InvalidOperationException("[FrameworkConfig] Config root node must be a JSON object.");
            }

            return data;
        }

        internal static JsonData CreateObject()
        {
            JsonData data = new JsonData();
            data.SetJsonType(JsonType.Object);
            return data;
        }

        internal static JsonData DeepCopy(JsonData source)
        {
            return JsonMapper.ToObject(JsonMapper.ToJson(source));
        }

        internal static JsonData DeepMerge(JsonData baseData, JsonData overrideData)
        {
            if (overrideData == null)
            {
                return baseData == null ? CreateObject() : DeepCopy(baseData);
            }

            if (baseData == null)
            {
                return DeepCopy(overrideData);
            }

            if (!baseData.IsObject || !overrideData.IsObject)
            {
                return DeepCopy(overrideData);
            }

            JsonData mergedData = DeepCopy(baseData);
            foreach (string key in overrideData.Keys)
            {
                JsonData overrideValue = overrideData[key];
                if (mergedData.ContainsKey(key))
                {
                    mergedData[key] = DeepMerge(mergedData[key], overrideValue);
                }
                else
                {
                    mergedData[key] = DeepCopy(overrideValue);
                }
            }

            return mergedData;
        }

        internal static bool TryReadModuleConfig<TConfig>(string mergedJson, out TConfig config)
            where TConfig : class
        {
            config = null;

            JsonData root;
            try
            {
                root = Parse(mergedJson);
            }
            catch
            {
                return false;
            }

            if (!root.ContainsKey("modules"))
            {
                return false;
            }

            JsonData modules = root["modules"];
            if (modules == null || !modules.IsObject)
            {
                return false;
            }

            string[] candidateKeys =
            {
                typeof(TConfig).FullName,
                typeof(TConfig).Name,
            };

            for (int index = 0; index < candidateKeys.Length; index++)
            {
                string candidateKey = candidateKeys[index];
                if (string.IsNullOrWhiteSpace(candidateKey) || !modules.ContainsKey(candidateKey))
                {
                    continue;
                }

                string moduleJson = JsonMapper.ToJson(modules[candidateKey]);
                try
                {
                    config = JsonMapper.ToObject<TConfig>(moduleJson);
                    return config != null;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}
