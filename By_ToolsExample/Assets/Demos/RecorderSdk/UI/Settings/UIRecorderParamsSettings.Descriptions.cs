namespace Demos.示例_录制视频Recorder.Scripts.UISettings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public partial class UIRecorderParamsSettings
    {
        /// <summary>
        /// 执行 LoadOptionDescriptions 相关逻辑。
        /// </summary>
        private void LoadOptionDescriptions()
        {
            string path = GetOptionDescriptionPath();
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                File.WriteAllText(path, JsonUtility.ToJson(CreateDefaultOptionDescriptionTable(), true));
            }

            try
            {
                _optionDescriptionTable = JsonUtility.FromJson<RecorderOptionDescriptionTable>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning("读取选项说明配置失败，将使用内置说明: " + e.Message);
                _optionDescriptionTable = CreateDefaultOptionDescriptionTable();
            }

            if (_optionDescriptionTable == null || _optionDescriptionTable.groups == null) _optionDescriptionTable = CreateDefaultOptionDescriptionTable();
        }

        /// <summary>
        /// 执行 BindDescButtons 相关逻辑。
        /// </summary>
        private void BindDescButtons()
        {
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (!string.Equals(button.name, "Btn_Desc", StringComparison.OrdinalIgnoreCase) && !string.Equals(button.name, "TogDesc", StringComparison.OrdinalIgnoreCase)) continue;
                string settingName = button.transform.parent != null ? button.transform.parent.name : button.name;
                BindHoverEvent(button.gameObject, EventTriggerType.PointerEnter, _ => ShowOptionDescWindow(settingName, button.GetComponent<RectTransform>()));
                BindHoverEvent(button.gameObject, EventTriggerType.PointerExit, _ => HideOptionDescWindow());
            }
        }

        /// <summary>
        /// 执行 BindHoverEvent 相关逻辑。
        /// </summary>
        private void BindHoverEvent(GameObject target, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            var trigger                  = target.GetComponent<EventTrigger>();
            if (trigger == null) trigger = target.AddComponent<EventTrigger>();
            var entry                    = trigger.triggers.Find(item => item.eventID == eventType);
            if (entry == null)
            {
                entry = new EventTrigger.Entry { eventID = eventType };
                trigger.triggers.Add(entry);
            }

            entry.callback.AddListener(action);
        }

        /// <summary>
        /// 执行 HideOptionDescWindow 相关逻辑。
        /// </summary>
        private void HideOptionDescWindow()
        {
            if (optionDescWindowRoot != null) optionDescWindowRoot.SetActive(false);
        }

        /// <summary>
        /// 执行 ShowOptionDescWindow 相关逻辑。
        /// </summary>
        private void ShowOptionDescWindow(string settingName, RectTransform source)
        {
            EnsureOptionDescWindow();
            if (optionDescWindowRoot == null) return;
            if (txtOptionDescTitle != null) txtOptionDescTitle.text     = $"选项说明 - {settingName}";
            if (txtOptionDescContent != null) txtOptionDescContent.text = BuildOptionDescriptionText(settingName);
            optionDescWindowRoot.SetActive(true);
            RefreshDescWindowSize();
            PlaceDescWindowBelowSource(source);
        }

        /// <summary>
        /// 执行 RefreshDescWindowSize 相关逻辑。
        /// </summary>
        private void RefreshDescWindowSize()
        {
            var windowRect = optionDescWindowRoot != null ? optionDescWindowRoot.GetComponent<RectTransform>() : null;
            if (windowRect == null) return;

            float titleHeight   = txtOptionDescTitle != null ? txtOptionDescTitle.preferredHeight : 0f;
            float contentHeight = txtOptionDescContent != null ? txtOptionDescContent.preferredHeight : 0f;
            float contentWidth  = txtOptionDescContent != null ? txtOptionDescContent.preferredWidth : descWindowMinSize.x;

            float width  = Mathf.Clamp(contentWidth + 48f, descWindowMinSize.x, descWindowMaxSize.x);
            float height = Mathf.Clamp(titleHeight + contentHeight + 52f, descWindowMinSize.y, descWindowMaxSize.y);
            windowRect.sizeDelta = new Vector2(width, height);
            if (txtOptionDescContent != null)
            {
                var contentRect = txtOptionDescContent.GetComponent<RectTransform>();
                contentRect.sizeDelta        = new Vector2(width - 40f, height - 64f);
                contentRect.anchoredPosition = new Vector2(0f, -24f);
            }
        }

        /// <summary>
        /// 执行 BuildOptionDescriptionText 相关逻辑。
        /// </summary>
        private string BuildOptionDescriptionText(string settingName)
        {
            var dropdown = GetDropdownBySettingName(settingName);
            var group    = FindOptionDescriptionGroup(GetOptionDescriptionKey(settingName), settingName);
            var lines    = new List<string>();
            if (dropdown == drConfig)
            {
                foreach (var config in _currentPlatformConfigs)
                {
                    string configName = !string.IsNullOrWhiteSpace(config.displayName) ? config.displayName : config.configId;
                    lines.Add($"{configName}: {config.platform} 平台，{(config.isDefault ? "默认配置，可保存基础设置与 FFmpeg 路径" : "用户配置，可保存覆盖")}");
                }
            }
            else if (dropdown != null)
            {
                foreach (var option in dropdown.options) lines.Add($"{option.text}: {GetConfiguredOptionDescription(group, option.text)}");
            }
            else if (group != null && group.options != null && group.options.Count > 0)
            {
                foreach (var item in group.options)
                {
                    if (item == null) continue;
                    lines.Add($"{item.option}: {item.description}");
                }
            }
            else lines.Add("当前项没有下拉选项，可在说明 JSON 中补充描述。");

            return string.Join("\n", lines);
        }

        /// <summary>
        /// 执行 FindOptionDescriptionGroup 相关逻辑。
        /// </summary>
        private RecorderOptionDescriptionGroup FindOptionDescriptionGroup(string key, string settingName)
        {
            if (_optionDescriptionTable == null || _optionDescriptionTable.groups == null) return null;
            foreach (var group in _optionDescriptionTable.groups)
            {
                if (group == null) continue;
                if (string.Equals(group.key, key, StringComparison.OrdinalIgnoreCase) || string.Equals(group.settingName, settingName, StringComparison.OrdinalIgnoreCase)) return group;
            }

            return null;
        }

        /// <summary>
        /// 执行 getConfiguredOptionDescription 相关逻辑。
        /// </summary>
        private static string GetConfiguredOptionDescription(RecorderOptionDescriptionGroup group, string option)
        {
            if (group != null && group.options != null)
            {
                foreach (var item in group.options)
                    if (item != null && string.Equals(item.option, option, StringComparison.OrdinalIgnoreCase))
                        return item.description;
            }

            return "可在 RecorderOptionDescriptions.json 中配置该选项说明。";
        }

        /// <summary>
        /// 执行 getDropdownBySettingName 相关逻辑。
        /// </summary>
        private Dropdown GetDropdownBySettingName(string n)
        {
            if (n.Contains("配置文件")) return drConfig;
            if (n.Contains("显示器")) return drDisplay;
            if (n.Contains("运行平台")) return drPlatform;
            if (n.Contains("使用方式")) return drUseMode;
            if (n.Contains("停止录制等待")) return stopVideoTimeoutMs;
            if (n.Contains("视频格式")) return drVideoFormat;
            if (n.Contains("音频采集模式")) return drAudioMode;
            if (n.Contains("音频编码器")) return drAudioCoder;
            if (n.Contains("音频码率")) return drAudioBitrate;
            if (n.Contains("音频采样率")) return drAudioSampleRate;
            if (n.Contains("音频声道")) return drAudioChannel;
            if (n.Contains("视频录制帧率")) return videoCaptureFrameRate;
            if (n.Contains("视频输出缩放")) return videoOutputScale;
            if (n.Contains("视频画质")) return videoCrf;
            if (n.Contains("视频像素格式")) return videoPixelFormat;
            if (n.Contains("视频编码器")) return videoCodec;
            if (n.Contains("视频编码预设")) return videoPreset;
            if (n.Contains("视频码率")) return webmVideoBitrate;
            if (n.Contains("实时编码")) return webmVideoDeadlineMode;
            if (n.Contains("CPU")) return webmVideoCpuUsed;
            if (n.Contains("ffmpeg退出超时")) return stopVideoTimeoutMs;
            if (n.Contains("临时文件释放超时")) return waitTempFileReadyTimeoutMs;
            if (n.Contains("后台合并")) return mergeTimeoutMs;
            if (n.Contains("删除临时文件")) return deleteTempFilesAfterMerge;
            return null;
        }

        /// <summary>
        /// 执行 getOptionDescriptionKey 相关逻辑。
        /// </summary>
        private static string GetOptionDescriptionKey(string n)
        {
            if (n.Contains("配置文件")) return "Config";
            if (n.Contains("显示器")) return "Display";
            if (n.Contains("运行平台")) return "platform";
            if (n.Contains("使用方式")) return "useMode";
            if (n.Contains("推流地址")) return "StreamUrl";
            if (n.Contains("推流码率")) return "StreamVideoBitrate";
            if (n.Contains("GOP")) return "StreamGop";
            if (n.Contains("缓冲区")) return "StreamBufferSize";
            if (n.Contains("低延迟")) return "StreamLowLatency";
            if (n.Contains("自动重连")) return "StreamAutoReconnect";
            if (n.Contains("重连次数")) return "StreamReconnectCount";
            if (n.Contains("重连间隔")) return "StreamReconnectInterval";
            if (n.Contains("包含系统音频")) return "StreamIncludeAudio";
            if (n.Contains("文件前缀")) return "OutputFilePrefix";
            if (n.Contains("视频格式")) return "VideoFormat";
            if (n.Contains("音频采集模式")) return "audioMode";
            if (n.Contains("音频编码器")) return "audioCodec";
            if (n.Contains("音频码率")) return "audioBitrate";
            if (n.Contains("音频采样率")) return "audioSampleRate";
            if (n.Contains("音频声道")) return "AudioChannel";
            if (n.Contains("录制音量增强")) return "AudioGainEnabled";
            if (n.Contains("录制音量增益")) return "AudioGainDb";
            if (n.Contains("音频限幅")) return "AudioLimiter";
            if (n.Contains("视频录制帧率")) return "FrameRate";
            if (n.Contains("视频输出缩放")) return "outputScale";
            if (n.Contains("视频画质")) return "videoCrf";
            if (n.Contains("视频像素格式")) return "pixelFormat";
            if (n.Contains("视频编码器")) return "videoCodec";
            if (n.Contains("视频编码预设")) return "videoPreset";
            if (n.Contains("视频码率")) return "VideoBitrate";
            if (n.Contains("实时编码")) return "Deadline";
            if (n.Contains("CPU")) return "CpuUsed";
            if (n.Contains("停止录制等待")) return "StopTimeout";
            if (n.Contains("ffmpeg退出超时")) return "StopTimeout";
            if (n.Contains("临时文件释放超时")) return "TempFileTimeout";
            if (n.Contains("后台合并")) return "MergeTimeout";
            if (n.Contains("删除临时文件")) return "DeleteTempFiles";
            return n;
        }

        /// <summary>
        /// 执行 PlaceDescWindowBelowSource 相关逻辑。
        /// </summary>
        private void PlaceDescWindowBelowSource(RectTransform source)
        {
            if (source == null || optionDescWindowRoot == null) return;
            var canvas     = optionDescWindowRoot.GetComponentInParent<Canvas>();
            var canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
            var windowRect = optionDescWindowRoot.GetComponent<RectTransform>();
            if (canvasRect == null || windowRect == null) return;
            var corners = new Vector3[4];
            source.GetWorldCorners(corners);
            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[2]);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.worldCamera, out var localPoint))
            {
                var   targetPos  = localPoint + new Vector2(windowRect.sizeDelta.x * 0.5f + 12f, -windowRect.sizeDelta.y * 0.5f);
                var   halfCanvas = canvasRect.rect.size * 0.5f;
                float minX       = -halfCanvas.x + windowRect.sizeDelta.x * 0.5f + 12f;
                float maxX       = halfCanvas.x - windowRect.sizeDelta.x * 0.5f - 12f;
                float minY       = -halfCanvas.y + windowRect.sizeDelta.y * 0.5f + 12f;
                float maxY       = halfCanvas.y - windowRect.sizeDelta.y * 0.5f - 12f;

                if (targetPos.x > maxX)
                {
                    var leftPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, leftPoint, canvas.worldCamera, out var leftLocal))
                    {
                        targetPos.x = leftLocal.x - windowRect.sizeDelta.x * 0.5f - 12f;
                    }
                }

                windowRect.anchoredPosition = new Vector2(Mathf.Clamp(targetPos.x, minX, maxX), Mathf.Clamp(targetPos.y, minY, maxY));
            }
        }

        /// <summary>
        /// 执行 CreateDefaultOptionDescriptionTable 相关逻辑。
        /// </summary>
        private RecorderOptionDescriptionTable CreateDefaultOptionDescriptionTable()
        {
            return new RecorderOptionDescriptionTable
            {
                groups = new List<RecorderOptionDescriptionGroup>
                {
                    CreateDescGroup("VideoFormat", "视频格式", "webm", "适合 Unity 内播放和跨平台分发。", "mp4", "兼容性更广。"),
                    CreateDescGroup("useMode", "使用方式", "存储本地", "将录制内容保存为本地视频文件，需要配置视频文件保存路径。", "视频推流", "将录制画面实时推送到 rtmp:// 或 rtmps:// 地址，适合直播服务器或流媒体网关。"),
                    CreateDescGroup("StreamUrl", "推流地址", "rtmp://", "常用 RTMP 推流地址，例如 rtmp://服务器/app/streamKey。", "rtmps://", "加密 RTMP 推流地址，服务器支持 TLS 时使用。"),
                    CreateDescGroup("StreamVideoBitrate", "推流码率", "1M", "低带宽或低性能设备优先。", "2M", "720p 或低负载推荐。", "3M", "常规 1080p 均衡值。", "5M", "画质更好但网络和 CPU 压力更高。", "8M", "高画质，需要充分测试。"),
                    CreateDescGroup("StreamGop", "推流GOP", "20", "关键帧更频繁，延迟更低但码率压力更高。", "50", "25fps 下约 2 秒关键帧间隔，直播常用。", "60", "30fps 下约 2 秒关键帧间隔。", "120", "更省码率但拖动和恢复更慢。"),
                    CreateDescGroup("StreamBufferSize", "推流缓冲区", "2M", "低延迟优先。", "4M", "低码率场景。", "6M", "常规推流推荐。", "10M", "网络波动较大时更稳。", "16M", "高码率推流使用。"),
                    CreateDescGroup("StreamLowLatency", "推流低延迟", "开", "启用 zerolatency，降低延迟并减少排队。", "关", "允许编码器做更多缓冲，画质可能更稳但延迟更高。"),
                    CreateDescGroup("StreamAutoReconnect", "推流自动重连", "开", "保留重连配置，后续可用于断流恢复策略。", "关", "网络失败时直接结束推流。"),
                    CreateDescGroup("StreamReconnectCount", "推流重连次数", "0", "不重连。", "3", "常规推荐。", "5", "网络不稳定时使用。", "10", "长时间无人值守场景使用。"),
                    CreateDescGroup("StreamReconnectInterval", "推流重连间隔", "1000", "快速重试。", "3000", "默认推荐。", "5000", "降低服务器压力。", "10000", "弱网或服务器恢复较慢时使用。"),
                    CreateDescGroup("StreamIncludeAudio", "推流系统音频", "开", "Windows 推流包含系统音频需要当前 FFmpeg 支持 wasapi 输入；本地 MP4/WebM 录屏音频由 WASAPILoopbackRecorder.dll 录 WAV 后再 Merge，和实时推流音频不是同一路径。Linux/国产系统按 pulse/alsa/pipewire 或当前系统 FFmpeg 编译能力判断。", "关", "只推送画面，默认推荐先验证视频推流链路；Windows 当前 FFmpeg 不支持 wasapi 时请保持关闭。Linux 不使用 wasapi 判断。"),
                    CreateDescGroup("OutputFilePrefix", "文件前缀", "说明", "生成视频文件名前缀，用于区分不同业务或不同录制来源；建议使用字母、数字、下划线或短横线，避免使用系统文件名非法字符。"),
                    CreateDescGroup("audioMode", "音频采集模式", "静音录制", "只录制画面。", "系统音频", "本地录制时录制系统输出声音；推流时需同时开启推流系统音频。"),
                    CreateDescGroup("audioCodec", "音频编码器", "aac", "MP4 和 RTMP 推流常用编码器。", "libvorbis", "WebM 默认推荐。", "libopus", "WebM 低码率质量更好。"),
                    CreateDescGroup("AudioGainEnabled", "录制音量增强", "使用", "录制声音偏小时启用，会在 FFmpeg 音频链路中追加 volume 滤镜。", "不使用", "不改变录制音量，保持原始采集音量。"),
                    CreateDescGroup("AudioGainDb", "录制音量增益",
                        "-20 dB", "大幅降低音量，适合原始声音过大、明显爆音或系统输出电平过高的场景。",
                        "-12 dB", "明显降低音量，适合原始声音偏大但仍希望保留较清晰动态的场景。",
                        "-9 dB", "中等降低音量，适合轻微过载或需要给后续混音留余量的场景。",
                        "-6 dB", "轻度降低音量，常用于避免峰值过高，保留原始听感。",
                        "-3 dB", "小幅降低音量，适合只需要略微压低电平的场景。",
                        "0 dB", "不增不减，保持原始采集音量；不确定时可先使用该值。",
                        "3 dB", "轻微增大音量，适合录制声音略小但整体仍清晰的场景。",
                        "6 dB", "明显增大音量，常用起始值；建议同时开启音频限幅器。",
                        "9 dB", "较大增益，适合声音明显偏小的场景；需要实测是否产生削波或噪声放大。",
                        "12 dB", "高增益，适合非常小的声音；更容易放大底噪或产生破音，建议谨慎使用。",
                        "20 dB", "极高增益，只建议临时排查或特殊弱音源场景使用，必须实测并建议开启限幅器。"),
                    CreateDescGroup("AudioLimiter", "音频限幅器", "使用", "增益后追加 alimiter=limit=0.95，降低爆音和削波风险；若 FFmpeg 不支持会自动降级。", "不使用", "只使用 volume 增益，性能更简单但更容易过载。"),
                    CreateDescGroup("FrameRate", "视频录制帧率", "20", "低负载。", "25", "均衡。", "30", "默认推荐。", "45", "更流畅。", "60", "性能消耗更高。"),
                    CreateDescGroup("outputScale", "视频输出缩放比例", "0.5", "半分辨率。", "0.75", "均衡。", "1", "原始分辨率。"),
                    CreateDescGroup("videoCrf", "视频画质档位", "20", "更清晰。", "23", "均衡推荐。", "26", "文件更小。", "30", "压缩更强。"),
                    CreateDescGroup("videoCodec", "视频编码器", "libx264", "MP4 和 RTMP 推流推荐。", "libx265", "MP4 可选，兼容性较差。", "libvpx", "WebM VP8。", "libvpx-vp9", "WebM VP9。"),
                    CreateDescGroup("StopTimeout", "停止录制等待超时", "5000", "最多等待 5 秒，适合短视频或快速失败场景。", "8000", "最多等待 8 秒，适合轻量录制。", "15000", "最多等待 15 秒，推荐默认值。", "30000", "最多等待 30 秒，适合机器较慢或输出文件较大时使用。"),
                    CreateDescGroup("DeleteTempFiles", "删除临时文件", "是", "合并成功后删除。", "否", "保留方便排查。")
                }
            };
        }

        /// <summary>
        /// 执行 CreateDescGroup 相关逻辑。
        /// </summary>
        private RecorderOptionDescriptionGroup CreateDescGroup(string key, string settingName, params string[] options)
        {
            var group = new RecorderOptionDescriptionGroup { key = key, settingName = settingName, options = new List<RecorderOptionDescriptionItem>() };
            for (int i = 0; i + 1 < options.Length; i += 2)
            {
                group.options.Add(new RecorderOptionDescriptionItem { option = options[i], description = options[i + 1] });
            }

            return group;
        }
    }
}
