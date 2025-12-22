using System;
using System.Collections.Generic;

namespace ZCustom
{
    public class ListComponent<T> : List<T>, IDisposable
    {
        public static ListComponent<T> Create()
        {
            return TypePool.Instance.Fetch(typeof(ListComponent<T>)) as ListComponent<T>;
        }

        public void Dispose()
        {
            this.Clear();
            TypePool.Instance.Recycle(this);
        }
    }
}