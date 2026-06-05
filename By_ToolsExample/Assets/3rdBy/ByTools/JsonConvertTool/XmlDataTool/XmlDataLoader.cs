/*
 * 作者：王柏雁
 * 日期：2024-8-13
 * 作用：实际上是Xml文件转成Json后，读取的Json文件.
 * 注意：使用时不能在Awake中调用
 */

namespace _3rdBy.ByTools.JsonConvertTool.XmlDataTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using Plugins.LitJson;
    using UnityEngine;
    using UnityEngine.Networking;
    using XmlData;

    /// <summary>
    /// Json文件读取器，如果后读取的文件已存在，则会覆盖前文件！！！
    /// </summary>
    public class XmlDataLoader : MonoBehaviour
    {
        public static XmlDataLoader Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            var container = new GameObject("[Xml]");
            Instance = container.AddComponent<XmlDataLoader>();
            DontDestroyOnLoad(container);
        }

        private List<Type> _allConfigTypes;
        private Dictionary<Type, object> _allConfigDict;
        private const string FileSeparator = "Table"; // 文件分隔符
        private const string FileFolder = "XmlData";  // 文件夹
        private const string FileSuffix = ".json";    // 文件格式

        private void Awake()
        {
            _allConfigTypes = GetAllAttributeTypes();
            _allConfigDict  = new Dictionary<Type, object>();
        }

        private static List<Type> GetAllAttributeTypes()
        {
            var assembly = Assembly.GetAssembly(typeof(XmlByAttribute)); // 标签查找
            var types    = assembly.GetExportedTypes();
            var typeIes  = types.Where(o => isMyAttribute(Attribute.GetCustomAttributes(o, true)));
            return typeIes.Where(o => o.IsAbstract == false).ToList(); // 去除abstract父类

            bool isMyAttribute(IEnumerable<Attribute> o)
            {
                return o.OfType<XmlByAttribute>().Any();
            }
        }

        #region 路径

        private static string ResourcesPath(Type type)
        {
            Debug.LogError(type.Name);
            var jsonName = type.Name.Replace(FileSeparator, "");
            var jsonPath = $"{FileFolder}/{jsonName}";
            return jsonPath;
        }

        private static string StreamingPath(Type type)
        {
            var jsonName = type.Name.Replace(FileSeparator, "");
            var jsonPath = $"{Application.streamingAssetsPath}/{FileFolder}/{jsonName}{FileSuffix}";
            return jsonPath;
        }

        #endregion


        #region 同步加载

        /// <summary>
        /// 同步加载Resources配置文件
        /// </summary>
        public void LoadResources()
        {
            foreach (var configType in _allConfigTypes)
            {
                _allConfigDict.TryAdd(configType, null);

                object asset    = null;
                var    jsonPath = ResourcesPath(configType);
                FileExist(jsonPath);
                var json = Resources.Load<TextAsset>(jsonPath);
                if (json != null)
                {
                    asset = JsonMapper.ToObject(json.text, configType);
                    if (asset is XmlObject excelObject)
                    {
                        excelObject.EndInit();
                    }
                }

                _allConfigDict[configType] = asset;
            }
        }

        #endregion

        #region 异步加载

        /// <summary>
        /// 异步加载Resources配置文件
        /// </summary>
        public async UniTask LoadResourcesAsync()
        {
            foreach (var configType in _allConfigTypes)
            {
                _allConfigDict.TryAdd(configType, null);

                object asset    = null;
                var    jsonPath = ResourcesPath(configType);
                FileExist(jsonPath);
                var loadAsync = await Resources.LoadAsync(jsonPath);
                var json      = loadAsync as TextAsset;
                if (json != null)
                {
                    asset = JsonMapper.ToObject(json.text, configType);
                    if (asset is XmlObject excelObject)
                    {
                        excelObject.EndInit();
                    }
                }

                _allConfigDict[configType] = asset;
            }
        }

        /// <summary>
        /// 异步加载Streaming配置文件
        /// </summary>
        public async UniTask LoadStreamingAsync()
        {
            foreach (var configType in _allConfigTypes)
            {
                _allConfigDict.TryAdd(configType, null);

                object asset    = null;
                var    jsonPath = StreamingPath(configType);
                FileExist(jsonPath);
                var webRequest = UnityWebRequest.Get(jsonPath);
                await webRequest.SendWebRequest();
                if (string.IsNullOrEmpty(webRequest.error))
                {
                    var json = webRequest.downloadHandler.text;

                    //TODO:判定json中是否存在特殊字符
                    if (json.Contains("\ufeff"))
                    {
                        json = json.Replace("\ufeff", "");
                    }

                    asset = JsonMapper.ToObject(json, configType, jsonPath); // 给继承自XmlObject的对象中的dataList反序列化赋值
                    if (asset is XmlObject excelObject)
                    {
                        excelObject.EndInit();
                    }
                }

                _allConfigDict[configType] = asset;
            }
        }

        #endregion

        private static void FileExist(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"[XmlDataLoader] 文件不存在：{path}");
            }
        }
    }
}