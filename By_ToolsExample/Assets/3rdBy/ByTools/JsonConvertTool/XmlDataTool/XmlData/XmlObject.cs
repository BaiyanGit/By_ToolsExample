namespace _3rdBy.ByTools.TableConvertJson.XmlDataTool.XmlData
{
    /// <summary>
    /// Xml对象
    /// </summary>
    public abstract class XmlObject
    {
        /// <summary>
        /// 结束初始化
        /// </summary>
        public virtual void EndInit()
        {
        }

        /// <summary>
        /// 在结束之后初始化
        /// </summary>
        protected virtual void AfterEndInit()
        {
        }
    }
}