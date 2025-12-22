using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using LitJson;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace XFramework.ExcelData
{
    /// <summary>
    /// json文件读取器，如果后读取的文件已存在，则会覆盖前文件！！！
    /// </summary>
    public class ExcelDataLoader : MonoBehaviour
    {
        public static ExcelDataLoader Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            var container = new GameObject("[Excel]");
            Instance = container.AddComponent<ExcelDataLoader>();
            DontDestroyOnLoad(container);
        }

        private List<Type> _allConfigTypes;
        private Dictionary<Type, object> _allConfigDict;
        private const string FileSeparator = "Table";
        private const string FileFolder = "ExcelData";
        private const string FileSuffix = ".json";

        private void Awake()
        {
            _allConfigTypes = GetAllAttributeTypes();
            _allConfigDict = new Dictionary<Type, object>();
        }

        private List<Type> GetAllAttributeTypes()
        {
            //标签查找
            var assembly = Assembly.GetAssembly(typeof(ExcelConfigAttribute));
            var types = assembly.GetExportedTypes();

            bool IsMyAttribute(IEnumerable<Attribute> o)
            {
                return o.OfType<ExcelConfigAttribute>().Any();
            }

            var typeIes = types.Where(o => IsMyAttribute(Attribute.GetCustomAttributes(o, true)));
            //去除abstract父类
            return typeIes.Where(o => o.IsAbstract == false).ToList();
        }

        private string ResourcesPath(Type type)
        {
            var jsonName = type.Name.Replace(FileSeparator, "");
            var jsonPath = $"{FileFolder}/{jsonName}";
            return jsonPath;
        }

        private string StreamingPath(Type type)
        {
            var jsonName = type.Name.Replace(FileSeparator, "");
            var jsonPath = $"{Application.streamingAssetsPath}/{FileFolder}/{jsonName}{FileSuffix}";
            return jsonPath;
        }

        public async UniTask LoadResourcesAsync()
        {
            foreach (var configType in _allConfigTypes)
            {
                _allConfigDict.TryAdd(configType, null);

                object asset = null;
                var jsonPath = ResourcesPath(configType);
                var loadAsync = await Resources.LoadAsync(jsonPath);
                var json = loadAsync as TextAsset;
                if (json != null)
                {
                    asset = JsonMapper.ToObject(json.text, configType);
                    if (asset is ExcelObject excelObject)
                    {
                        excelObject.EndInit();
                    }
                }

                _allConfigDict[configType] = asset;
            }
        }

        public async UniTask LoadStreamingAsync()
        {
            foreach (var configType in _allConfigTypes)
            {
                _allConfigDict.TryAdd(configType, null);

                object asset = null;
                var jsonPath = StreamingPath(configType);
                UnityWebRequest webRequest = UnityWebRequest.Get(jsonPath);
                await webRequest.SendWebRequest();
                if (string.IsNullOrEmpty(webRequest.error))
                {
                    var json = webRequest.downloadHandler.text;
                    //TODO:判定json中是否存在特殊字符
                    if (json.Contains("\ufeff"))
                    {
                        json = json.Replace("\ufeff", "");
                    }

                    asset = JsonMapper.ToObject(json, configType);
                    if (asset is ExcelObject excelObject)
                    {
                        excelObject.EndInit();
                    }
                }

                _allConfigDict[configType] = asset;
            }
        }

        /// <summary>
        /// Resources同步读取
        /// </summary>
        public void LoadResources()
        {
            foreach (var configType in _allConfigTypes)
            {
                _allConfigDict.TryAdd(configType, null);

                object asset = null;
                var jsonPath = ResourcesPath(configType);
                var json = Resources.Load<TextAsset>(jsonPath);
                if (json != null)
                {
                    asset = JsonMapper.ToObject(json.text, configType);
                    if (asset is ExcelObject excelObject)
                    {
                        excelObject.EndInit();
                    }
                }

                _allConfigDict[configType] = asset;
            }
        }
    }
}