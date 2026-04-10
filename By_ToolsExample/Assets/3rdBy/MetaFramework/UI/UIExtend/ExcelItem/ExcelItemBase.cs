using _3rdBy.ByTools.TableConvertJson.ExcelDataTool.ExcelData;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Excel数据列表项基类
/// </summary>
public abstract class ExcelItemBase : MonoBehaviour
{
    /// <summary>
    /// 数据(public作用于当作回调参数传递)
    /// </summary>
    public ExcelObject itemData;

    /// <summary>
    /// 点击事件
    /// </summary>
    protected UnityAction<ExcelObject> onClickAction;

    /// <summary>
    /// 获取类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetType<T>() where T : ExcelObject
    {
        return itemData as T;
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="clickAction"></param>
    /// <typeparam name="T"></typeparam>
    public abstract void InitData<T>(T data, UnityAction<T> clickAction) where T : ExcelObject;

    /// <summary>
    /// 设置选中状态
    /// </summary>
    /// <param name="isSelected"></param>
    public virtual void SetItemState(bool isSelected)
    {
    }
}