using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using _3rdBy.ByTools.TableConvertJson.ExcelDataTool.ExcelData;
using Cysharp.Threading.Tasks;

#region 数据结构
[Serializable]
public partial class ExcelRecorderConfig : ExcelObject, IExcelData
{
    public int Id { get; set; }

	/// <summary>
	/// 运行时平台
	/// </summary>
	public int Platform { get; set; }

	/// <summary>
	/// 配置名称
	/// </summary>
	public string PresetName { get; set; }

	/// <summary>
	/// 显示器
	/// </summary>
	public int DisplayIndex { get; set; }

	/// <summary>
	/// 输出文件前缀
	/// </summary>
	public string OutputFilePrefix { get; set; }

	/// <summary>
	/// 输出文件格式
	/// </summary>
	public int OutputAsWebM { get; set; }

	/// <summary>
	/// ffmpeg可执行文件路径
	/// </summary>
	public string FFmpegExecutablePath { get; set; }

	/// <summary>
	/// 音频采集模式
	/// </summary>
	public int AudioMode { get; set; }

	/// <summary>
	/// 音频编码器
	/// </summary>
	public string AudioCodec { get; set; }

	/// <summary>
	/// 音频码率
	/// </summary>
	public string AudioBitrate { get; set; }

	/// <summary>
	/// 音频采样率
	/// </summary>
	public int AudioSampleRate { get; set; }

	/// <summary>
	/// 音频声道数
	/// </summary>
	public int AudioChannels { get; set; }

	/// <summary>
	/// 录制帧率
	/// </summary>
	public int CaptureFrameRate { get; set; }

	/// <summary>
	/// 视频输出缩放比例
	/// </summary>
	public float OutputScale { get; set; }

	/// <summary>
	/// 视频画质档位
	/// </summary>
	public int VideoCrf { get; set; }

	/// <summary>
	/// 视频像素格式
	/// </summary>
	public string PixelFormat { get; set; }

	/// <summary>
	/// 视频编码器
	/// </summary>
	public string VideoCodec { get; set; }

	/// <summary>
	/// 视频编码预设
	/// </summary>
	public string VideoPreset { get; set; }

	/// <summary>
	/// 视频码率
	/// </summary>
	public string WebmVideoBitrate { get; set; }

	/// <summary>
	/// 视频实时编码模式
	/// </summary>
	public string WebmDeadline { get; set; }

	/// <summary>
	/// 视频CPU使用等级(WebM)
	/// </summary>
	public int WebmCpuUsed { get; set; }

	/// <summary>
	/// 停止录制等待ffmpeg退出超时(毫秒)
	/// </summary>
	public int StopVideoTimeoutMs { get; set; }

	/// <summary>
	/// 等待临时文件释放超时（毫秒）
	/// </summary>
	public int WaitTempFileReadyTimeoutMs { get; set; }

	/// <summary>
	/// 后台合并音视频等待超时（毫秒），<=0 表示不限时
	/// </summary>
	public int MergeTimeoutMs { get; set; }

	/// <summary>
	/// 是否在后台合并完成后删除临时文件
	/// </summary>
	public bool DeleteTempFilesAfterMerge { get; set; }


}

#endregion

#region 数据处理
public partial class ExcelRecorderConfigTable : ExcelTable
{
    public static ExcelRecorderConfigTable Instance;
    
    public readonly List<ExcelRecorderConfig> dataList;
    private Dictionary<int, ExcelRecorderConfig> _dataDict = new Dictionary<int, ExcelRecorderConfig>();

    /// <summary>
    /// 构造函数
    /// </summary>
    public ExcelRecorderConfigTable()
    {
        Instance = this;
    }

    /// <summary>
    /// 通过索引获取数据
    /// </summary>
    /// <param name="index">数据索引</param>
    /// <returns>返回通过索引找到的数据</returns>
    public ExcelRecorderConfig Get(int id)
    {
        _dataDict.TryGetValue(id, out ExcelRecorderConfig value);
        if (value == null)
        {
            Debug.LogError($"配置找不到，配置表名: {nameof(ExcelRecorderConfig)}，配置id: {id}");
        }
        return value;
    }
    
    /// <summary>
    /// 是否包含数据
    /// </summary>
    /// <param name="id">数据索引</param>
    /// <returns>返回是否包含数据</returns>
    public bool Contain(int id)
    {
        return _dataDict.ContainsKey(id);
    }
    
    /// <summary>
    /// 获取所有数据
    /// </summary>
    /// <returns>返回所有数据</returns>
    public Dictionary<int, ExcelRecorderConfig> GetAll()
    {
        return _dataDict;
    }
    
    /// <summary>
    /// 结束后初始化
    /// </summary>
    public override void EndInit()
    {
        foreach (var edItemBase in dataList)
        {
            edItemBase.EndInit();
            _dataDict.Add(edItemBase.Id, edItemBase);
        }
        // 初始化之后要做的处理
        AfterEndInit();
    }
}
#endregion