namespace BySceneSnap_v1._0._0
{
    using UnityEngine;

    /// <summary>
    /// 基础数据类
    /// </summary>
    public class ObjectDataSnap : MonoBehaviour
    {
        [Header("文件名称")] public EFileName addType;
        [Header("对象Id")] public int id;
        [Header("对象名称")] public string objName;
        [Header("对象激活")] public bool isActive;
        [Header("对象位置")] public Vector3 position;
        [Header("对象旋转")] public Vector3 rotation;
        [Header("对象缩放")] public Vector3 localScale;

        [Header("基础数据")] public BaseData baseData;

        /// <summary>
        /// 获取对象属性
        /// </summary>
        public virtual void GetObjectAttributes()
        {
            id = SceneSnap.Instance.CreateNewId(gameObject);
            isActive = gameObject.activeSelf;
            objName = gameObject.name;
            position = transform.position;
            rotation = transform.eulerAngles;
            localScale = transform.localScale;

            // 数据转换
            baseData.id = id;
            baseData.isActive = isActive;
            baseData.objName = objName;
            baseData.position = position.ToVector3();
            baseData.rotation = rotation.ToVector3();
            baseData.localScale = localScale.ToVector3();
            SceneSnap.Instance.AddObjectData(gameObject, baseData, ref baseData.id);
        }

        /// <summary>
        /// 保存对象属性
        /// </summary>
        public void SaveObjectAttributes()
        {
            SceneSnap.Instance.WriteToFile(addType);
        }
    }

    [System.Serializable]
    public class BaseData
    {
        [Header("对象Id")] public int id;
        [Header("对象名称")] public string objName;
        [Header("对象激活")] public bool isActive;
        [Header("对象位置")] public float[] position;
        [Header("对象旋转")] public float[] rotation;
        [Header("对象缩放")] public float[] localScale;
    }
}