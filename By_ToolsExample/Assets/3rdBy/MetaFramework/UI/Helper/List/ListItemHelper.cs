using UnityEngine;

namespace MetaFramework.Helper
{
    public class ListItemHelper : MonoBehaviour
    {
        protected ListItemData data;
        protected object[] param;

        public T GetData<T>() where T : ListItemData
        {
            return data as T;
        }

        public virtual void SetData(ListItemData data, params object[] param)
        {
            this.data = data;
            this.param = param;
        }
    }
}