//=====================================================
// 文件名称: RecorderDisplayInfo
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-13
// 描    述: 录制目标显示器信息数据结构，用于保存显示器索引、名称、分辨率、偏移和主屏标记。
//=====================================================

using System;
using UnityEngine;

[Serializable]
public class RecorderDisplayInfo
{
    [Header("显示器索引")] public int index;

    [Header("显示器名称")] public string name;

    [Header("显示器宽度")] public int width;

    [Header("显示器高度")] public int height;

    [Header("显示器相对虚拟桌面的X偏移")] public int offsetX;

    [Header("显示器相对虚拟桌面的Y偏移")] public int offsetY;

    [Header("是否为主显示器")] public bool isPrimary;

    /// <summary>
    /// 获取用于下拉框显示的文本。
    /// </summary>
    /// <returns>显示器选项文本。</returns>
    public string ToOptionText()
    {
        string primaryMark = isPrimary ? " (主)" : "";
        return $"{name}{primaryMark} - {width}x{height}";
    }
}