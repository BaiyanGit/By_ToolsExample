//=====================================================
// 文件名称: EventCallback.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: EventManager 运行时监听事件回调委托定义。
//=====================================================

namespace _3rdBy.ByFramework.EventManager.Core
{
    /// <summary>
    /// 无参数事件回调。
    /// </summary>
    public delegate void CallBack();

    /// <summary>
    /// 单参数事件回调。
    /// </summary>
    /// <param name="arg">事件参数。</param>
    /// <typeparam name="T">参数类型。</typeparam>
    public delegate void CallBack<in T>(T arg);

    /// <summary>
    /// 双参数事件回调。
    /// </summary>
    /// <param name="arg1">第一个事件参数。</param>
    /// <param name="arg2">第二个事件参数。</param>
    /// <typeparam name="T">第一个参数类型。</typeparam>
    /// <typeparam name="TX">第二个参数类型。</typeparam>
    public delegate void CallBack<in T, in TX>(T arg1, TX arg2);

    /// <summary>
    /// 三参数事件回调。
    /// </summary>
    /// <param name="arg1">第一个事件参数。</param>
    /// <param name="arg2">第二个事件参数。</param>
    /// <param name="arg3">第三个事件参数。</param>
    /// <typeparam name="T">第一个参数类型。</typeparam>
    /// <typeparam name="TX">第二个参数类型。</typeparam>
    /// <typeparam name="TY">第三个参数类型。</typeparam>
    public delegate void CallBack<in T, in TX, in TY>(T arg1, TX arg2, TY arg3);

    /// <summary>
    /// 四参数事件回调。
    /// </summary>
    /// <param name="arg1">第一个事件参数。</param>
    /// <param name="arg2">第二个事件参数。</param>
    /// <param name="arg3">第三个事件参数。</param>
    /// <param name="arg4">第四个事件参数。</param>
    /// <typeparam name="T">第一个参数类型。</typeparam>
    /// <typeparam name="TX">第二个参数类型。</typeparam>
    /// <typeparam name="TY">第三个参数类型。</typeparam>
    /// <typeparam name="TZ">第四个参数类型。</typeparam>
    public delegate void CallBack<in T, in TX, in TY, in TZ>(T arg1, TX arg2, TY arg3, TZ arg4);

    /// <summary>
    /// 五参数事件回调。
    /// </summary>
    /// <param name="arg1">第一个事件参数。</param>
    /// <param name="arg2">第二个事件参数。</param>
    /// <param name="arg3">第三个事件参数。</param>
    /// <param name="arg4">第四个事件参数。</param>
    /// <param name="arg5">第五个事件参数。</param>
    /// <typeparam name="T">第一个参数类型。</typeparam>
    /// <typeparam name="TX">第二个参数类型。</typeparam>
    /// <typeparam name="TY">第三个参数类型。</typeparam>
    /// <typeparam name="TZ">第四个参数类型。</typeparam>
    /// <typeparam name="TW">第五个参数类型。</typeparam>
    public delegate void CallBack<in T, in TX, in TY, in TZ, in TW>(T arg1, TX arg2, TY arg3, TZ arg4, TW arg5);
}
