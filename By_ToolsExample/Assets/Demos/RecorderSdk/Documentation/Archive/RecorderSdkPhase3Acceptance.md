# Recorder SDK 第 3 阶段验收说明

## 阶段目标

第 3 阶段只做配置管理稳定化，允许新增 `RecorderConfigRegistry`。

本阶段不做：

- BackendFactory
- Profile 云同步
- 大规模服务层拆分

## 本阶段改动

- 新增 `RecorderConfigRegistry`。
- 新增 `RecorderConfigResult`。
- 补充配置相关 `RecorderErrorCode`。
- 配置主身份统一为 `configId`。
- `RecordConfigProvider` 改为通过 `RecorderConfigRegistry` 读取当前配置。
- 新增配置扫描、索引、当前配置、创建、克隆、删除、重命名、保存和回退能力。
- 增加配置 AutoFix 与 warnings。
- 新版 `UseWinRecordConfig.json` / `UseLinuxRecordConfig.json` 保存时只写 `schemaVersion`、`platform`、`currentConfigId`。

## 配置扫描规则

`RecorderConfigRegistry.Scan()` 会扫描配置目录下的 `.json` 文件。

忽略：

- `UseWinRecordConfig.json`
- `UseLinuxRecordConfig.json`
- 选项说明 JSON，例如 `OptionDescriptions.json`、`RecorderOptionDescriptions.json`

加载后按 `configId` 建索引。`fileName` 只作为运行时路径缓存和旧配置兼容，不作为主身份。

## configId 冲突处理

如果扫描时发现重复 `configId`：

- 后出现的配置会自动追加 `_1`、`_2` 等后缀。
- 操作结果和 Registry warnings 中会记录修复说明。
- 返回错误码不直接失败，属于 `ConfigAutoFixed` 级别的 warning。

## 旧配置迁移规则

`schemaVersion < 2` 视为旧配置：

- `configName -> displayName`
- 旧 `displayName -> captureDisplayName`
- 旧 `fileName` 优先用于推导 `configId`
- 保存后 `schemaVersion = 2`
- 保存后不再写入 `configName`
- 保存后不再写入 `fileName`

## 自动修复规则

AutoFix 会返回 warnings，不静默修复。

- `configId` 为空：自动生成。
- `configId` 非法：规整为小写、数字、下划线。
- `configId` 重复：自动追加 `_1`、`_2`。
- `displayName` 为空：使用 `configId`。
- `videoSaveDirectory` 为空：运行时回退默认 Videos 目录，并给 warning。
- `customFFmpegPath` 为空：运行时回退默认 FFmpeg 路径，并给 warning。
- `captureFrameRate <= 0`：回退 25。
- `outputScale <= 0`：回退 1.0。
- `streamReconnectCount < 0`：回退 0。
- `streamReconnectIntervalMs <= 0`：回退 3000。

## 当前配置选择规则

加载 `UseWinRecordConfig.json` / `UseLinuxRecordConfig.json` 时：

1. 优先使用 `currentConfigId`。
2. 找不到时，使用旧 `fileName` 推导出的 configId。
3. 再找不到时，回退当前平台 `template_*_medium`。
4. 再找不到时，回退任意可用模板。
5. 仍无可用配置时，创建默认 Medium 配置。

新版 Use JSON 保存格式：

```json
{
  "schemaVersion": 2,
  "platform": "Win",
  "currentConfigId": "template_win_medium"
}
```

## 配置操作结果

配置操作统一返回 `RecorderConfigResult`：

- `success`
- `errorCode`
- `warnings`
- `configId`
- `config`
- `message`

## 可能触发 warning 的操作

- 扫描到重复 `configId`。
- 迁移旧配置。
- 自动生成空 `configId`。
- 修复非法 `configId`。
- 自动补空 `displayName`。
- `videoSaveDirectory` 为空，运行时回退默认目录。
- `customFFmpegPath` 为空，运行时回退默认 FFmpeg。
- 帧率、缩放、重连参数非法并被修复。

## 会失败的情况

- 配置目录无法创建或扫描：`ConfigDirectoryMissing`
- 配置不存在：`ConfigNotFound`
- 删除模板配置：`ConfigDeleteFailed`
- 保存配置失败：`ConfigSaveFailed`
- 删除配置失败：`ConfigDeleteFailed`
- 配置为空：`ConfigNull`

## SmokeTest 覆盖

`RecorderSdkSmokeTest` 已增加配置测试：

- 加载所有模板。
- 重复 `configId` 检测。
- 旧配置迁移。
- 空 `displayName` 修复。
- 空 `configId` 修复。
- `currentConfigId` 找不到时回退 Medium。
- 删除用户配置。
- 禁止删除 template 配置。
- 克隆 template 成 user profile。
- 保存后 JSON 不含 `configName` / `fileName`。

## 仍需 Unity 真机验证

- 在真实 StreamingAssets 配置目录中扫描和保存。
- UI 操作接入 Registry 后的完整交互。
- Windows/Linux 平台切换时 UseWin/UseLinux 指针回退。
- Unity BatchMode 当前因 License Client IPC 超时无法进入脚本编译，继续作为环境限制记录。
