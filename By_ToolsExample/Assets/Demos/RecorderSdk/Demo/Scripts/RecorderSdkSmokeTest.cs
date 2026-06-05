//=====================================================
// 文件名称: RecorderSdkSmokeTest
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 提供 Recorder SDK 第 1 阶段最小冒烟测试，便于在 Unity 场景中手动验收状态、错误码、事件顺序和 SessionHistory。
//=====================================================

namespace Demos.RecorderSdk.Demo.Scripts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Core.Commands;
    using Core.Config;
    using Core.Events;
    using Core.Runtime;
    using UI.Settings;
    using UnityEngine;

    /// <summary>
    /// Recorder SDK 冒烟测试脚本。
    /// </summary>
    public class RecorderSdkSmokeTest : MonoBehaviour
    {
        [Header("启动时自动运行")] public bool runOnStart;
        [Header("是否执行真实 Start/Stop")] public bool runRealStartStop = true;
        [Header("等待录制启动秒数")] public float startWaitSeconds = 2f;
        [Header("等待后台合并秒数")] public float mergeWaitSeconds = 10f;

        [Header("测试事件日志")] public readonly List<string> eventLogs = new();
        [Header("测试事件类型")] public readonly List<RecorderSessionEventType> eventTypes = new();

        private CrossPlatformScreenRecorder _recorder;

        /// <summary>
        /// 启动时按需运行冒烟测试。
        /// </summary>
        private void Start()
        {
            if (runOnStart) RunSmokeTest();
        }

        /// <summary>
        /// Inspector 右键入口，运行完整冒烟测试。
        /// </summary>
        [ContextMenu("Run Smoke Test")]
        public void RunSmokeTest()
        {
            StopAllCoroutines();
            StartCoroutine(RunSmokeTestCoroutine());
        }

        /// <summary>
        /// 顺序执行最小测试用例。
        /// </summary>
        private IEnumerator RunSmokeTestCoroutine()
        {
            eventLogs.Clear();
            eventTypes.Clear();
            _recorder = ResolveRecorder();
            if (_recorder == null)
            {
                LogError("未找到 CrossPlatformScreenRecorder，无法运行测试。");
                yield break;
            }

            SubscribeEvents(_recorder);
            Log("开始 Recorder SDK 冒烟测试。");

            yield return TestStopWhenNotRecording();
            TestInvalidFFmpegPath();
            TestStartFailureEventOrder();
            TestEmptyOutputDirectoryFallback();
            TestSessionHistoryLimit();
            TestClearSessionHistory();
            TestConfigRegistry();
            TestAudioGainCommandBuilder();
            yield return TestMergeFailureSimulation();
            yield return TestMergingStartStateCompatibility();

            if (runRealStartStop)
            {
                yield return TestStartTwice();
                yield return TestNormalStartStop();
            }
            else
            {
                Log("已跳过真实 Start/Stop 测试。");
            }

            Log($"冒烟测试结束，SessionHistory.Count={_recorder.SessionHistory.Count}。");
            UnsubscribeEvents(_recorder);
        }

        /// <summary>
        /// 测试未录制时 Stop。
        /// </summary>
        private IEnumerator TestStopWhenNotRecording()
        {
            int errorCountBefore = CountEventType(RecorderSessionEventType.ErrorOccurred);
            var task = _recorder.StopRecordingAsync();
            yield return WaitTask(task);
            AssertResult(task.Result, false, RecorderErrorCode.NotRecording, "未录制时 Stop");
            AssertCondition(CountEventType(RecorderSessionEventType.ErrorOccurred) == errorCountBefore, "非法 Stop 触发 Warning 而不是 Error");
        }

        /// <summary>
        /// 测试 FFmpeg 路径错误校验。
        /// </summary>
        private void TestInvalidFFmpegPath()
        {
            var config = CreateValidationConfig();
            var displays = RecorderDisplayProvider.GetDisplays();
            var result = RecorderConfigValidator.ValidateForStart(config, "Z:/invalid_ffmpeg_path/ffmpeg.exe", displays, RecorderPathService.GetDefaultVideoDirectory());
            AssertCondition(!result.isValid && result.GetFirstErrorCodeOrDefault(RecorderErrorCode.None) == RecorderErrorCode.FFmpegPathMissing, "FFmpeg 路径错误校验");
        }

        /// <summary>
        /// 测试 Start 失败事件顺序是否包含 StartFailed 和 ErrorOccurred。
        /// </summary>
        private void TestStartFailureEventOrder()
        {
            int startFailedBefore = CountHistoryEvent(RecorderSessionEventType.StartFailed);
            int errorBefore = CountEventType(RecorderSessionEventType.ErrorOccurred);
            InvokePrivateFailStart(RecorderErrorCode.FFmpegPathMissing, "SmokeTest 模拟 Start 失败。");
            AssertCondition(CountHistoryEvent(RecorderSessionEventType.StartFailed) > startFailedBefore, "Start 失败历史包含 StartFailed");
            AssertCondition(CountEventType(RecorderSessionEventType.ErrorOccurred) > errorBefore, "Start 失败事件包含 ErrorOccurred");
            SetPrivateField("<State>k__BackingField", RecorderState.Ready);
        }

        /// <summary>
        /// 测试输出目录为空时回退默认 Videos 路径。
        /// </summary>
        private void TestEmptyOutputDirectoryFallback()
        {
            var config = CreateValidationConfig();
            config.videoSaveDirectory = string.Empty;
            string existingFileAsFakeFFmpeg = Path.Combine(Application.dataPath, "../ProjectSettings/ProjectVersion.txt");
            var displays = RecorderDisplayProvider.GetDisplays();
            var result = RecorderConfigValidator.ValidateForStart(config, existingFileAsFakeFFmpeg, displays, string.Empty);
            bool hasOnlyDisplayProblem = !result.isValid && result.GetFirstErrorCodeOrDefault(RecorderErrorCode.None) == RecorderErrorCode.DisplayNotFound;
            AssertCondition(result.isValid || hasOnlyDisplayProblem, "输出目录为空时使用默认 Videos 路径");
        }

        /// <summary>
        /// 测试连续 Start 两次。
        /// </summary>
        private IEnumerator TestStartTwice()
        {
            if (!HasDefaultFFmpeg())
            {
                Log("跳过连续 Start 两次测试：默认 FFmpeg 不存在。");
                yield break;
            }

            var firstTask = _recorder.StartRecordingAsync();
            var secondTask = _recorder.StartRecordingAsync();
            yield return WaitTask(secondTask);
            AssertResult(secondTask.Result, false, RecorderErrorCode.AlreadyRunning, "连续 Start 第二次");
            yield return WaitTask(firstTask);
            if (firstTask.Result.success) AssertCondition(ContainsEventType(RecorderSessionEventType.StartSucceeded), "Start 成功事件顺序包含 StartSucceeded");

            if (firstTask.Result.success)
            {
                yield return new WaitForSeconds(startWaitSeconds);
                var stopTask = _recorder.StopRecordingAsync();
                yield return WaitTask(stopTask);
                yield return WaitForMerge();
            }
        }

        /// <summary>
        /// 测试正常 Start -> Stop。
        /// </summary>
        private IEnumerator TestNormalStartStop()
        {
            if (!HasDefaultFFmpeg())
            {
                Log("跳过正常 Start->Stop 测试：默认 FFmpeg 不存在。");
                yield break;
            }

            var startTask = _recorder.StartRecordingAsync();
            yield return WaitTask(startTask);
            if (!startTask.Result.success)
            {
                LogWarning($"正常 Start->Stop 测试无法继续，Start 失败：{startTask.Result.errorCodeText} {startTask.Result.message}");
                yield break;
            }

            yield return new WaitForSeconds(startWaitSeconds);
            var stopTask = _recorder.StopRecordingAsync();
            yield return WaitTask(stopTask);
            AssertResult(stopTask.Result, true, RecorderErrorCode.None, "正常 Stop");
            AssertCondition(ContainsEventType(RecorderSessionEventType.StopRequested) && ContainsEventType(RecorderSessionEventType.StopSucceeded), "Stop 成功事件顺序包含 StopRequested/StopSucceeded");
            yield return WaitForMerge();
        }

        /// <summary>
        /// 通过反射模拟后台合并失败，验证 MergeFailed 与 SessionHistory。
        /// </summary>
        private IEnumerator TestMergeFailureSimulation()
        {
            int historyBefore = _recorder.SessionHistory.Count;
            var session = new RecorderSession
            {
                sessionId = Guid.NewGuid().ToString("N"),
                startTime = DateTime.Now,
                outputPath = Path.Combine(Application.temporaryCachePath, "RecorderSmoke_MergeFail.mp4"),
                videoTempPath = Path.Combine(Application.temporaryCachePath, "RecorderSmoke_MissingVideo.mp4"),
                audioTempPath = Path.Combine(Application.temporaryCachePath, "RecorderSmoke_MissingAudio.wav"),
                isStreaming = false,
                configName = "SmokeTest",
                displayName = "SmokeDisplay"
            };

            SetPrivateField("_activeMergeJobs", 1);
            SetPrivateField("_currentProcessingSession", session);
            InvokePrivateMerge(session);

            yield return null;
            yield return null;

            AssertCondition(_recorder.SessionHistory.Count > historyBefore, "合并失败模拟写入 SessionHistory");
            AssertCondition(_recorder.LastSession != null && _recorder.LastSession.sessionId == session.sessionId, "合并失败模拟更新 LastSession");
            AssertCondition(ContainsHistoryEvent(RecorderSessionEventType.MergeFailed), "合并失败模拟写入 MergeFailed");
        }

        /// <summary>
        /// 测试后台合并期间再次开始录制的状态兼容性，以及旧合并完成不覆盖新录制状态。
        /// </summary>
        private IEnumerator TestMergingStartStateCompatibility()
        {
            AssertCondition(RecorderStateTransition.CanTransit(RecorderState.Merging, RecorderState.Starting), "Merging -> Starting 合法");

            var oldSession = new RecorderSession
            {
                sessionId = Guid.NewGuid().ToString("N"),
                startTime = DateTime.Now,
                outputPath = "rtmp://example/live/old",
                videoTempPath = "rtmp://example/live/old",
                isStreaming = true,
                configId = "smoke_old",
                configName = "Smoke Old",
                displayName = "Smoke Display"
            };

            var newSession = new RecorderSession
            {
                sessionId = Guid.NewGuid().ToString("N"),
                startTime = DateTime.Now,
                outputPath = "rtmp://example/live/new",
                videoTempPath = "rtmp://example/live/new",
                isStreaming = true,
                configId = "smoke_new",
                configName = "Smoke New",
                displayName = "Smoke Display"
            };

            SetPrivateField("_activeMergeJobs", 1);
            SetPrivateField("_currentProcessingSession", oldSession);
            SetPrivateState(RecorderState.Merging);
            InvokePrivateMerge(oldSession);
            SetPrivateField("_currentSession", newSession);
            SetPrivateState(RecorderState.Starting);
            SetPrivateState(RecorderState.Recording);

            yield return null;
            yield return null;

            AssertCondition(_recorder.State == RecorderState.Recording, "旧 Merge 完成不覆盖新 Recording 状态");
            SetPrivateField("_currentSession", null);
            SetPrivateField("_activeMergeJobs", 0);
            SetPrivateField("<State>k__BackingField", RecorderState.Ready);
        }

        /// <summary>
        /// 测试 SessionHistory 最大数量限制。
        /// </summary>
        private void TestSessionHistoryLimit()
        {
            int oldMaxCount = _recorder.maxSessionHistoryCount;
            _recorder.maxSessionHistoryCount = 3;
            _recorder.ClearSessionHistory();
            for (int i = 0; i < 5; i++)
            {
                _recorder.StopRecordingAsync();
            }

            AssertCondition(_recorder.GetSessionHistory().Count <= 3, "SessionHistory 最大数量限制生效");
            _recorder.maxSessionHistoryCount = oldMaxCount;
        }

        /// <summary>
        /// 测试清空 SessionHistory。
        /// </summary>
        private void TestClearSessionHistory()
        {
            _recorder.ClearSessionHistory();
            AssertCondition(_recorder.GetSessionHistory().Count == 0, "ClearSessionHistory 生效");
        }

        /// <summary>
        /// 测试配置注册表扫描、迁移、修复、回退、克隆、删除和保存格式。
        /// </summary>
        private void TestConfigRegistry()
        {
            string directory = Path.Combine(Application.temporaryCachePath, "RecorderRegistrySmokeConfigs");
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
            string defaultTemplateDirectory = Path.Combine(directory, "DefaultTemplate");
            string customTemplateDirectory = Path.Combine(directory, "CustomTemplate");
            string useTemplateDirectory = Path.Combine(directory, "UseTemplate");
            Directory.CreateDirectory(defaultTemplateDirectory);
            Directory.CreateDirectory(customTemplateDirectory);
            Directory.CreateDirectory(useTemplateDirectory);

            WriteConfig(defaultTemplateDirectory, "Template_Win_Medium.json", CreateRegistryConfig("template_win_medium", "Win 中配置", true));
            WriteConfig(defaultTemplateDirectory, "Template_Win_Low.json", CreateRegistryConfig("template_win_low", "Win 低配置", true));
            WriteConfig(customTemplateDirectory, "create_win_dup_a.json", CreateRegistryConfig("create_win_dup", "重复A", false));
            WriteConfig(customTemplateDirectory, "create_win_dup_b.json", CreateRegistryConfig("create_win_dup", "重复B", false));
            File.WriteAllText(Path.Combine(customTemplateDirectory, "create_win_legacy.json"),
                "{\n" +
                "  \"configName\": \"旧配置显示名\",\n" +
                "  \"fileName\": \"Create_Win_Legacy.json\",\n" +
                "  \"displayName\": \"\\\\\\\\.\\\\DISPLAY9\",\n" +
                "  \"platform\": \"Windows\",\n" +
                "  \"captureFrameRate\": 0,\n" +
                "  \"outputScale\": 0,\n" +
                "  \"streamReconnectCount\": -1,\n" +
                "  \"streamReconnectIntervalMs\": 0\n" +
                "}");
            File.WriteAllText(Path.Combine(useTemplateDirectory, "UseWinRecordConfig.json"), "{ \"schemaVersion\": 2, \"platform\": \"Win\", \"currentConfigId\": \"missing_config\" }");

            var registry = new RecorderConfigRegistry(directory, "Win");
            var scan = registry.Scan();
            AssertCondition(scan.success, "Registry 扫描成功");
            AssertCondition(registry.Configs.Count >= 5, "Registry 加载所有模板和用户配置");
            AssertCondition(scan.warnings.Count > 0, "Registry 重复 configId / AutoFix 产生 warning");

            var current = registry.GetCurrentConfig();
            AssertCondition(current.success && current.configId == "template_win_medium", "currentConfigId 找不到时回退 medium");

            var legacy = FindConfig(registry, "create_win_legacy");
            AssertCondition(legacy != null && legacy.displayName == "旧配置显示名", "旧配置 configName 迁移到 displayName");
            AssertCondition(legacy != null && !string.IsNullOrWhiteSpace(legacy.captureDisplayName), "旧 displayName 迁移到 captureDisplayName");
            AssertCondition(legacy != null && legacy.captureFrameRate > 0 && legacy.outputScale > 0f, "旧配置非法参数自动修复");

            var empty = registry.CreateUserConfig("");
            AssertCondition(empty.success && !string.IsNullOrWhiteSpace(empty.config.displayName), "空 displayName 自动修复");

            var noId = CreateRegistryConfig("", "No Id", false);
            var saveNoId = registry.SaveConfig(noId);
            AssertCondition(saveNoId.success && !string.IsNullOrWhiteSpace(saveNoId.configId), "空 configId 自动修复");

            var clone = registry.CloneConfig("template_win_medium", "克隆模板");
            AssertCondition(clone.success && clone.configId.StartsWith("Custom_", StringComparison.OrdinalIgnoreCase), "克隆 template 成 user profile");

            var deleteTemplate = registry.DeleteUserConfig("template_win_medium");
            AssertCondition(!deleteTemplate.success && deleteTemplate.errorCode == RecorderErrorCode.ConfigDeleteFailed, "禁止删除 template 配置");

            var deleteUser = registry.DeleteUserConfig(clone.configId);
            AssertCondition(deleteUser.success, "删除用户配置");

            var saved = registry.SaveConfig(CreateRegistryConfig("Custom_Win_干净保存", "干净保存", false));
            string savedPath = Path.Combine(customTemplateDirectory, "Custom_Win_干净保存.json");
            string savedJson = File.Exists(savedPath) ? File.ReadAllText(savedPath) : string.Empty;
            AssertCondition(!savedJson.Contains("\"configName\"") && !savedJson.Contains("\"fileName\""), "保存后 JSON 不含 configName/fileName");
        }

        /// <summary>
        /// 写入测试配置。
        /// </summary>
        /// <summary>
        /// 测试录制音量增益会生成 volume 和 limiter 音频滤镜。
        /// </summary>
        private void TestAudioGainCommandBuilder()
        {
            var config = CreateValidationConfig();
            config.audioMode = 1;
            config.enableAudioGain = true;
            config.audioGainDb = 6f;
            config.audioLimiterEnabled = true;

            string filter = FFmpegCommandBuilder.BuildAudioFilter(config, true);
            AssertCondition(filter.Contains("volume=6dB"), "音量增强命令包含 volume=6dB");
            AssertCondition(filter.Contains("alimiter=limit=0.95"), "开启限幅器时命令包含 alimiter=limit=0.95");
            string mergedFilter = FFmpegCommandBuilder.BuildAudioFilter(config, true, "aresample=48000");
            AssertCondition(mergedFilter == "aresample=48000,volume=6dB,alimiter=limit=0.95", "已有音频滤镜时合并为单个 -af 表达式");

            config.enableAudioGain = false;
            AssertCondition(string.IsNullOrWhiteSpace(FFmpegCommandBuilder.BuildAudioFilter(config, true)), "关闭音量增强时不拼接音频滤镜");
            config.enableAudioGain = true;
            AssertCondition(string.IsNullOrWhiteSpace(FFmpegCommandBuilder.BuildAudioFilter(config, false)), "没有音频输入时不拼接音频滤镜");
            TestAudioGainLegacyDefaults();
        }

        /// <summary>
        /// 测试旧配置缺少音量增益字段时默认值正确。
        /// </summary>
        private void TestAudioGainLegacyDefaults()
        {
            string json = "{ \"schemaVersion\": 2, \"configId\": \"legacy_audio_gain\", \"displayName\": \"Legacy\" }";
            var config = JsonUtility.FromJson<RecorderParamsConfig>(json);
            RecorderConfigMigrator.MigrateIdentity(config, json, "legacy_audio_gain.json");
            AssertCondition(config.enableAudioGain == false, "旧配置缺少 enableAudioGain 时默认关闭");
            AssertCondition(Mathf.Approximately(config.audioGainDb, 0f), "旧配置缺少 audioGainDb 时默认 0dB");
            AssertCondition(config.audioLimiterEnabled, "旧配置缺少 audioLimiterEnabled 时默认开启限幅器");
        }

        private static void WriteConfig(string directory, string fileName, RecorderParamsConfig config)
        {
            File.WriteAllText(Path.Combine(directory, fileName), JsonUtility.ToJson(config, true));
        }

        /// <summary>
        /// 创建配置注册表测试配置。
        /// </summary>
        private static RecorderParamsConfig CreateRegistryConfig(string configId, string displayName, bool isTemplate)
        {
            var config = CreateValidationConfig();
            config.configId = configId;
            config.displayName = displayName;
            config.platform = "Win";
            config.fileName = string.IsNullOrWhiteSpace(configId) ? string.Empty : configId + ".json";
            config.isDefault = isTemplate;
            config.schemaVersion = 2;
            return config;
        }

        /// <summary>
        /// 查找指定配置。
        /// </summary>
        private static RecorderParamsConfig FindConfig(RecorderConfigRegistry registry, string configId)
        {
            foreach (var config in registry.Configs)
            {
                if (string.Equals(config.configId, configId, StringComparison.OrdinalIgnoreCase)) return config;
            }

            return null;
        }

        /// <summary>
        /// 创建用于校验器测试的配置。
        /// </summary>
        private static RecorderParamsConfig CreateValidationConfig()
        {
            return new RecorderParamsConfig
            {
                schemaVersion = 2,
                configId = "smoke_test",
                displayName = "Smoke Test",
                platform = Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer ? "Linux" : "Win",
                useMode = 0,
                outputFilePrefix = "Smoke",
                outputAsWebm = false,
                displayIndex = 0,
                captureDisplayName = string.Empty,
                captureFrameRate = 25,
                outputScale = 1f,
                videoCodec = "libx264",
                videoPreset = "ultrafast",
                videoCrf = 23,
                pixelFormat = "yuv420p",
                audioMode = 0,
                audioCodec = "aac",
                audioBitrate = "128k",
                audioSampleRate = 48000,
                audioChannels = 2,
                enableAudioGain = false,
                audioGainDb = 0f,
                audioLimiterEnabled = true,
                streamUrl = string.Empty
            };
        }

        /// <summary>
        /// 获取或创建录制器实例。
        /// </summary>
        private static CrossPlatformScreenRecorder ResolveRecorder()
        {
            if (CrossPlatformScreenRecorder.ins != null) return CrossPlatformScreenRecorder.ins;
            var go = new GameObject("[RecorderSdkSmokeTest] Recorder");
            return go.AddComponent<CrossPlatformScreenRecorder>();
        }

        /// <summary>
        /// 判断默认 FFmpeg 是否存在。
        /// </summary>
        private static bool HasDefaultFFmpeg()
        {
            string path = RecorderPathService.GetPlatformDefaultFFmpegPath();
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        }

        /// <summary>
        /// 等待 Task 完成。
        /// </summary>
        private static IEnumerator WaitTask(Task task)
        {
            while (task != null && !task.IsCompleted) yield return null;
        }

        /// <summary>
        /// 等待后台合并结束。
        /// </summary>
        private IEnumerator WaitForMerge()
        {
            float endTime = Time.realtimeSinceStartup + mergeWaitSeconds;
            while (_recorder.IsMerging && Time.realtimeSinceStartup < endTime) yield return null;
        }

        /// <summary>
        /// 订阅测试关心的事件。
        /// </summary>
        private void SubscribeEvents(CrossPlatformScreenRecorder recorder)
        {
            recorder.OnRecorderStateChanged += OnRecorderEvent;
            recorder.OnRecorderError += OnRecorderEvent;
            recorder.OnRecorderWarning += OnRecorderEvent;
            recorder.OnRecorderProgress += OnRecorderEvent;
            recorder.OnMergeStarted += OnRecorderEvent;
            recorder.OnMergeCompleted += OnRecorderEvent;
            recorder.OnConfigChanged += OnRecorderEvent;
            recorder.OnRecordStarted += OnRecordStarted;
            recorder.OnRecordStopped += OnRecordStopped;
        }

        /// <summary>
        /// 取消事件订阅。
        /// </summary>
        private void UnsubscribeEvents(CrossPlatformScreenRecorder recorder)
        {
            recorder.OnRecorderStateChanged -= OnRecorderEvent;
            recorder.OnRecorderError -= OnRecorderEvent;
            recorder.OnRecorderWarning -= OnRecorderEvent;
            recorder.OnRecorderProgress -= OnRecorderEvent;
            recorder.OnMergeStarted -= OnRecorderEvent;
            recorder.OnMergeCompleted -= OnRecorderEvent;
            recorder.OnConfigChanged -= OnRecorderEvent;
            recorder.OnRecordStarted -= OnRecordStarted;
            recorder.OnRecordStopped -= OnRecordStopped;
        }

        /// <summary>
        /// 记录统一事件。
        /// </summary>
        private void OnRecorderEvent(RecorderEventArgs args)
        {
            eventTypes.Add(args.eventType);
            Log($"Event type={args.eventType}, state={args.state}, error={args.errorCodeText}, session={args.sessionId}, message={args.message}");
        }

        /// <summary>
        /// 记录开始事件。
        /// </summary>
        private void OnRecordStarted(string outputPath)
        {
            Log("OnRecordStarted: " + outputPath);
        }

        /// <summary>
        /// 记录停止事件。
        /// </summary>
        private void OnRecordStopped(string outputPath)
        {
            Log("OnRecordStopped: " + outputPath);
        }

        /// <summary>
        /// 验证 RecorderResult。
        /// </summary>
        private void AssertResult(RecorderResult result, bool expectedSuccess, RecorderErrorCode expectedErrorCode, string title)
        {
            bool passed = result != null && result.success == expectedSuccess && result.errorCode == expectedErrorCode;
            AssertCondition(passed, $"{title}: success={result?.success}, error={result?.errorCodeText}, message={result?.message}");
        }

        /// <summary>
        /// 输出断言结果。
        /// </summary>
        private void AssertCondition(bool condition, string message)
        {
            if (condition) Log("[PASS] " + message);
            else LogError("[FAIL] " + message);
        }

        /// <summary>
        /// 通过反射设置私有字段。
        /// </summary>
        private void SetPrivateField(string fieldName, object value)
        {
            typeof(CrossPlatformScreenRecorder)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(_recorder, value);
        }

        /// <summary>
        /// 通过反射设置状态。
        /// </summary>
        private void SetPrivateState(RecorderState state)
        {
            typeof(CrossPlatformScreenRecorder)
                .GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(_recorder, new object[] { state });
        }

        /// <summary>
        /// 通过反射调用后台合并，制造失败路径。
        /// </summary>
        private void InvokePrivateMerge(RecorderSession session)
        {
            typeof(CrossPlatformScreenRecorder)
                .GetMethod("MergeSessionInBackground", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(_recorder, new object[] { session });
        }

        /// <summary>
        /// 通过反射模拟 Start 失败。
        /// </summary>
        private void InvokePrivateFailStart(RecorderErrorCode errorCode, string message)
        {
            typeof(CrossPlatformScreenRecorder)
                .GetMethod("FailStart", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(_recorder, new object[] { errorCode, message });
        }

        /// <summary>
        /// 判断事件类型是否已出现。
        /// </summary>
        private bool ContainsEventType(RecorderSessionEventType eventType)
        {
            return eventTypes.Contains(eventType);
        }

        /// <summary>
        /// 统计事件类型出现次数。
        /// </summary>
        private int CountEventType(RecorderSessionEventType eventType)
        {
            int count = 0;
            foreach (var item in eventTypes)
            {
                if (item == eventType) count++;
            }

            return count;
        }

        /// <summary>
        /// 判断历史中是否包含指定事件。
        /// </summary>
        private bool ContainsHistoryEvent(RecorderSessionEventType eventType)
        {
            foreach (var item in _recorder.GetSessionHistory())
            {
                if (item.eventType == eventType) return true;
            }

            return false;
        }

        /// <summary>
        /// 统计历史事件类型出现次数。
        /// </summary>
        private int CountHistoryEvent(RecorderSessionEventType eventType)
        {
            int count = 0;
            foreach (var item in _recorder.GetSessionHistory())
            {
                if (item.eventType == eventType) count++;
            }

            return count;
        }

        /// <summary>
        /// 记录普通日志。
        /// </summary>
        private void Log(string message)
        {
            eventLogs.Add(message);
            Debug.Log("[RecorderSdkSmokeTest] " + message);
        }

        /// <summary>
        /// 记录警告日志。
        /// </summary>
        private void LogWarning(string message)
        {
            eventLogs.Add(message);
            Debug.LogWarning("[RecorderSdkSmokeTest] " + message);
        }

        /// <summary>
        /// 记录错误日志。
        /// </summary>
        private void LogError(string message)
        {
            eventLogs.Add(message);
            Debug.LogError("[RecorderSdkSmokeTest] " + message);
        }
    }
}
