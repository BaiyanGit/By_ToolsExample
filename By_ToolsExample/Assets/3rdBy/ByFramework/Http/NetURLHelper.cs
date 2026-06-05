namespace _3rdBy.ByFramework.Http
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 用于获取URL参数
    /// </summary>
    public static class NetURLHelper
    {
        private static readonly Dictionary<string, string> ParamDic = new Dictionary<string, string>();

#if UNITY_WEBGL
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
#endif
        private static void Init()
        {
#if UNITY_EDITOR
            Debug.LogWarning("[NetURLHelper]:Editor模式跳过");
            return;
#elif UNITY_WEBGL
        try
        {
            Debug.Log("[NetURLHelper]:获取URL参数!!!");
            var url = Application.absoluteURL;
            var paramStr = url.Substring(url.IndexOf('?') + 1);
            var paramArr = paramStr.Split('&');
            foreach (var param in paramArr)
            {
                var paramSplit = param.Split('=');
                var key = paramSplit[0];
                var value = paramSplit[1];
                ParamDic.TryAdd(key, value);
            }
            Debug.Log("[NetURLHelper]:获取URL参数成功!!!");
        }
        catch (Exception e)
        {
            Debug.LogError("[NetURLHelper]:URL参数获取失败!!!");
        }
#endif
        }

        public static string Get(string key)
        {
            ParamDic.TryGetValue(key, out var value);
            return value;
        }

        public static Dictionary<string, string> GetAll()
        {
            return ParamDic;
        }
    }
}