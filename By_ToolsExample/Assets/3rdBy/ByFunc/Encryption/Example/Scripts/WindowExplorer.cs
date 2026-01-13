using System;
using System.Runtime.InteropServices;

#if UNITY_STANDALONE_WIN
public abstract class WindowExplorer
{
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class FileBase
{
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName : FileBase
{
    /// <summary>
    /// 结构体大小
    /// </summary>
    public int structSize = 0;

    /// <summary>
    /// 窗口句柄
    /// </summary>
    public IntPtr dlgOwner = IntPtr.Zero;

    /// <summary>
    /// 实例句柄
    /// </summary>
    public IntPtr instance = IntPtr.Zero;

    /// <summary>
    /// 文件筛选器
    /// </summary>
    public string filter = null;

    /// <summary>
    /// 
    /// </summary>
    public string customFilter = null;

    /// <summary>
    /// 
    /// </summary>
    public int maxCustFilter = 0;

    /// <summary>
    /// 文件筛选器索引
    /// </summary>
    public int filterIndex = 0;

    /// <summary>
    /// 文件名
    /// </summary>
    public string file = null;

    /// <summary>
    /// 最大文件名
    /// </summary>
    public int maxFile = 0;

    /// <summary>
    /// 文件标题
    /// </summary>
    public string fileTitle = null;

    /// <summary>
    /// 最大文件标题
    /// </summary>
    public int maxFileTitle = 0;

    /// <summary>
    /// 初始目录
    /// </summary>
    public string initialDir = null;

    /// <summary>
    /// 标题
    /// </summary>
    public string title = null;

    /// <summary>
    /// 标志
    /// </summary>
    public int flags = 0;

    /// <summary>
    /// 文件偏移
    /// </summary>
    public short fileOffset = 0;

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public short fileExtension = 0;

    /// <summary>
    /// 初始目录
    /// </summary>
    public string defExt = null;

    /// <summary>
    /// 自定义数据
    /// </summary>
    public IntPtr custData = IntPtr.Zero;

    /// <summary>
    /// 钩子
    /// </summary>
    public IntPtr hook = IntPtr.Zero;

    /// <summary>
    /// 模板名称
    /// </summary>
    public string templateName = null;

    /// <summary>
    /// 保留指针
    /// </summary>
    public IntPtr reservedPtr = IntPtr.Zero;

    /// <summary>
    /// 保留整数
    /// </summary>
    public int reservedInt = 0;

    /// <summary>
    /// 标志扩展
    /// </summary>
    public int flagsEx = 0;
}

public abstract class Flags
{
    public const int OFN_EXPLORER = 0x00080000; // 使用新的通用对话框样式
    public const int OFN_PATHMUSTEXIST = 0x00000800; // 目录必须存在
    public const int OFN_FILEMUSTEXIST = 0x00001000; // 文件必须存在
    public const int OFN_NOCHANGEDIR = 0x00000008; // 只能输入已经存在的路径名。如果用户输入一个路径名，但该路径不存在，则对话框会显示一个错误消息，并要求用户重新输入。
    public const int OFN_ALLOWMULTISELECT = 0x00000200; // 可以选择多个文件
}
#endif