# ByFramework Changelog

## 2026-06-10 Post-R9 Compile Fix

* 修复 `JsonFileConfigProvider` 可访问性低于 `PersistentConfigProvider` / `StreamingAssetsConfigProvider` / `EditorConfigProvider` 的问题。
* 修复 `MockInputBackend.SetResolvedState` 未公开实现 `IInputBackend.SetResolvedState` 的问题。
* 修复 `MockInputBackend` public 类型暴露 internal backend 类型导致的潜在可访问性冲突。
* 修复 `InputService` public 构造函数暴露 internal `MockInputBackend` 的潜在 CS0051 风险。
* 同步当前文档状态：R1~R9 CLOSED，R10.1 PASS / CLOSED，R10.2 暂缓等待编译错误修复验证。
* 新增 / 同步 `NetworkSystemRuntimeAPIFreeze.md`，作为 R10.2 后续实现依据。

## 2026-06-10 R10.1 NetworkSystem Runtime API Freeze

* 新增 `NetworkSystemRuntimeAPIFreeze.md`。
* 冻结 R10.1 NetworkSystem 最小 Runtime API 面。
* 明确 NetworkCore 必须保持纯 C#、低依赖、可独立抽离。
* 明确 NetworkSystem Adapter 才负责接入 ByFramework Platform。
* 明确 NetworkSystem 不负责 SimulationSync、业务协议语义、数据库业务或 License 规则。

## 2026-06-09 R9.2 UISystem Runtime Implementation

* R9.2 UISystem Runtime Implementation 通过架构审查。
* R9 UISystem CLOSED。
