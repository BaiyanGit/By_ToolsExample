namespace Demos.示例_记录编辑场景对象.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using _3rdBy.Plugins.LitJson;
    using UnityEngine;

    public class SceneSnap : MonoBehaviour
    {
        #region 单例

        private static SceneSnap _instance;

        public static SceneSnap Instance
        {
            get
            {
                if (_instance == null)
                {
                    var sceneSnap = FindAnyObjectByType<SceneSnap>();
                    if (sceneSnap == null)
                    {
                        var go = new GameObject("SceneSnap");
                        _instance = go.AddComponent<SceneSnap>();
                    }
                    else
                    {
                        _instance = sceneSnap;
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private readonly Dictionary<GameObject, BaseData> _objectDataSnaps = new();

        #endregion

        /// <summary>
        /// 添加对象数据
        /// </summary>
        /// <param name="go"></param>
        /// <param name="data"></param>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        public void AddObjectData<T>(GameObject go, T data, ref int id) where T : BaseData
        {
            if (data is T snap)
            {
                if (_objectDataSnaps.ContainsKey(go) && _objectDataSnaps.Count > 0)
                {
                    Debug.LogError($"Id：{snap.id}\n" +
                                   $"名称：{snap.objName}\n" +
                                   $"激活：{snap.isActive}\n" +
                                   $"位置：({string.Join(",", snap.position)})\n" +
                                   $"旋转：({string.Join(",", snap.rotation)})\n" +
                                   $"缩放：({string.Join(",", snap.localScale)})");
                    return;
                }

                Debug.Log("AddObjectData\n" +
                          $"Id：{snap.id}\n" +
                          $"名称：{snap.objName}\n" +
                          $"激活：{snap.isActive}\n" +
                          $"位置：({string.Join(",", snap.position)})\n" +
                          $"旋转：({string.Join(",", snap.rotation)})\n" +
                          $"缩放：({string.Join(",", snap.localScale)})");
                id      = _objectDataSnaps.Count;
                snap.id = id;
                _objectDataSnaps.Add(go, snap);
            }
            else
            {
                Debug.LogError($"类型不匹配，期望类型：{typeof(T).Name}，实际类型：{data.GetType().Name}");
            }
        }

        /// <summary>
        /// 创建新Id
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CreateNewId(GameObject obj)
        {
            return !_objectDataSnaps.TryGetValue(obj, out var snap) ? _objectDataSnaps.Count : snap.id;
        }

        #region Find ObjectDataSnap

        /// <summary>
        /// 查找指定GameObject数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public BaseData GetObjectData(GameObject obj)
        {
            return _objectDataSnaps.GetValueOrDefault(obj);
        }

        /// <summary>
        /// 查找类型GameObject数据
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objectDataSnap"></param>
        public void GetObjectData(GameObject obj, out BaseData objectDataSnap)
        {
            objectDataSnap = _objectDataSnaps.GetValueOrDefault(obj);
        }

        /// <summary>
        /// 查找指定类型GameObject数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseData GetObjectData(int id)
        {
            return _objectDataSnaps.Values.FirstOrDefault(snap => snap.id == id);
        }

        /// <summary>
        /// 查找类型GameObject数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="objectDataSnap"></param>
        public void GetObjectData(int id, out BaseData objectDataSnap)
        {
            objectDataSnap = _objectDataSnaps.Values.FirstOrDefault(snap => snap.id == id);
        }

        #endregion

        /// <summary>
        /// 获取所有GameObject数据
        /// </summary>
        /// <returns></returns>
        private List<BaseData> GetAllObjectData()
        {
            var list = new List<BaseData>(_objectDataSnaps.Values);

            #region Log

            var log = "";
            foreach (var snap in list)
            {
                log += $"Id：{snap.id}\n" +
                       $"名称：{snap.objName}\n" +
                       $"激活：{snap.isActive}\n" +
                       $"位置：({string.Join(",", snap.position)})\n" +
                       $"旋转：({string.Join(",", snap.rotation)})\n" +
                       $"缩放：({string.Join(",", snap.localScale)})";

                // 判定snap是不是最后一个
                if (snap != list[^1])
                {
                    log += "\n\n";
                }
            }

            if (log.Length > 0)
            {
                log = $"\n\n{log}";
            }

            Debug.Log($"TotalCount：{list.Count}{log}");

            #endregion

            return list;
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="fileName"></param>
        public void WriteToFile(EFileName fileName)
        {
            var list = GetAllObjectData();
            var json = JsonMapper.ToJson(list);
            Debug.Log(json);
            var folderPath = $"{Application.streamingAssetsPath}/SceneSnap";
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            var filePath = $"{folderPath}/{fileName.ToString()}.json";
            Debug.Log($"WriteToFile：{filePath}");

            System.IO.File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Vector3转float数组
        /// </summary>
        /// <param name="vct"></param>
        /// <param name="arr"></param>
        public static void Vector3ToFloatArray(Vector3 vct, ref float[] arr)
        {
            arr    = new float[3];
            arr[0] = vct.x;
            arr[1] = vct.y;
            arr[2] = vct.z;
        }
    }

    public enum EFileName
    {
        // Tips: 请确保文件名与枚举值名称一致
        [InspectorName("场景1.json")] Scene1,
        [InspectorName("场景2.json")] Scene2,
        [InspectorName("场景3.json")] Scene3,
    }


    public static class ExtensionMethods
    {
        public static Vector3 ToFloatArray(this float[] arr)
        {
            if (arr == null || arr.Length < 3)
            {
                return Vector3.zero;
            }

            return new Vector3(arr[0], arr[1], arr[2]);
        }

        public static float[] ToVector3(this Vector3 vct)
        {
            return new[] { vct.x, vct.y, vct.z };
        }
    }
}