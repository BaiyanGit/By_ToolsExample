# ToolAssetRegistry 初始模板

> 本文件用于记录 ByFramework 项目中的历史工具资产。  
> 工具进入 ByFramework Runtime 前，必须先经过审计。  
> 本文件初期以 Markdown 维护，后续可扩展为 JSON 或 ScriptableObject。

---

# 一、状态说明

```text
Active          当前可用
Reusable        可复用
NeedRefactor    需要重构
ReferenceOnly   仅作参考
Deprecated      建议废弃
Unknown         未审计
```

---

# 二、归属说明

```text
ByFramework        纳入 ByFramework
ByTools            保持独立工具
ExternalPlugin     外部插件
Legacy             历史遗留
FeatureModule      业务模块
PlatformCandidate  平台候选能力
```

---

# 三、工具资产清单

## 1. Excel 转 Json 工具

```text
工具名称：Excel 转 Json 工具
工具类型：数据转换工具
建议归属：ByTools
ByFramework 关系：ByFramework 可调用生成结果
是否可独立导出：是
当前状态：Reusable
处理建议：保持独立，不重写
```

---

## 2. FFMPEG Recorder

```text
工具名称：FFMPEG Recorder
工具类型：媒体录制工具
建议归属：ByTools / MediaTools
ByFramework 关系：Platform MediaSystem / RecorderSystem 预留接口
是否可独立导出：是
当前状态：Reusable
处理建议：保持独立，后期按需接入
```

---

## 3. 推流工具

```text
工具名称：推流工具
工具类型：媒体推流工具
建议归属：ByTools / StreamingTool
ByFramework 关系：Platform StreamingSystem 预留接口
是否可独立导出：是
当前状态：Reusable
处理建议：保持独立，后期按需接入
```

---

## 4. Downloader

```text
工具名称：Downloader
工具类型：大文件下载工具
建议归属：NetworkCore + ByTools 双形态
ByFramework 关系：NetworkSystem Downloader 重构来源
是否可独立导出：是
当前状态：NeedRefactor
处理建议：审计后重构，不直接复制
```

---

## 5. 串口工具

```text
工具名称：串口工具
工具类型：设备通信工具
建议归属：DeviceIntegration 基础实现来源
ByFramework 关系：Platform SerialPort 能力候选
是否可独立导出：待确认
当前状态：NeedRefactor
处理建议：审计后重构
```

---

## 6. Json 工具

```text
工具名称：Json 工具
工具类型：序列化 / 反序列化工具
建议归属：ByTools
ByFramework 关系：按需调用
是否可独立导出：是
当前状态：Reusable
处理建议：保持独立
```

---

## 7. 历史 Editor 工具

```text
工具名称：历史 Editor 工具
工具类型：Unity Editor 工具
建议归属：分类处理
ByFramework 关系：部分纳入 EditorTools，部分保持 ByTools
是否可独立导出：视具体工具而定
当前状态：Unknown
处理建议：逐个审计
```
