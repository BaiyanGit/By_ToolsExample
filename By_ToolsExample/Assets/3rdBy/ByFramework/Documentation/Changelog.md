# ByFramework Changelog

## 2026-06

### 文档

* 建立 ByFramework 框架文档体系
* 建立 ByFramework 编码规范文档，并更新 README 与 AGENTS 工作规范
* 更新 README 中对框架安装目录的说明，避免要求固定目录

### 事件系统

* 合并 EventManager 与旧事件中心的运行时监听能力
* EventManager 新增 Type/Enum 事件监听、移除与广播 API
* 移除旧事件中心兼容门面，运行时监听能力统一由 EventManager 提供
* 明确 Runtime Listener 主入口仅使用 AddListener、Broadcast、RemoveListener
* 明确 Enum 事件使用枚举实例本身作为 Key，避免同一枚举类型下不同枚举值共用事件
* 新增 P0-1 EventManager Runtime Listener 最小验证示例
* 新增 EventManager 模块 README
* 完成 EventManager 目录整理后的规范化检查
* 完成 IEvent 拆分后的引用检查
* 确认 TypePool 已同步为 EventTypePool 引用
* 补齐 EventManager 模块代码规范注释
* 恢复 EventManager 模块命名空间兼容性，修正目录整理后的 using 错误
* 新增 EventCallback.cs，将 CallBack 委托迁移到 EventManager 模块
* 删除旧事件中心兼容层代码，解除 EventManager 对旧事件中心命名空间的依赖

### 路径系统

* 新增 ByFrameworkPathUtility，通过 AGENTS.md、README.md 与 Documentation/Architecture.md 自动定位框架根目录
* UIAutoCreatePathSetting 改为通过 ByFrameworkPathUtility 获取框架内部路径
* UI 自动生成工具移除对固定 ByFramework 安装目录的依赖
* 全项目确认无 Assets/3rdBy/ByFramework、Assets/3rdBy/MetaFramework、3rdBy/ByFramework、3rdBy/MetaFramework 固定路径残留
* P0-0.1A：为 UIAutoCreate 配置与代码模板加载补充空引用保护和清晰错误信息
* P0-0.1B：GetRuntimePath 与 GetSamplesPath 在目标目录不存在时改为明确抛出异常
* P0-0.1C：ByFrameworkPathUtility 缓存根目录失效时自动重新扫描
* 关闭 P0-0.1 移除框架硬编码路径整改

### MetaFramework -> ByFramework

### 修复

* UIAutoCreatePathSetting
* JSaver
* VoiceData
* TestVoiceMsg
* TestGetAudioClip
